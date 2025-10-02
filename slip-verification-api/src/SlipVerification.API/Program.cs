using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using SlipVerification.API.Middleware;
using SlipVerification.API.Services;
using SlipVerification.Application.Interfaces;
using SlipVerification.Domain.Interfaces;
using SlipVerification.Infrastructure.Data;
using SlipVerification.Infrastructure.Data.Repositories;
using SlipVerification.Infrastructure.Extensions;
using SlipVerification.Infrastructure.Hubs;
using SlipVerification.Infrastructure.Services;
using SlipVerification.Infrastructure.Services.Realtime;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
var elasticsearchUri = builder.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Application", "SlipVerification")
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(elasticsearchUri))
    {
        IndexFormat = "slip-verification-{0:yyyy.MM.dd}",
        AutoRegisterTemplate = true,
        NumberOfShards = 2,
        NumberOfReplicas = 1,
        MinimumLogEventLevel = Serilog.Events.LogEventLevel.Information
    })
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Register Prometheus metrics service
builder.Services.AddSingleton<IMetrics, MetricsService>();

// Configure Response Compression for performance
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "image/svg+xml" });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

// Configure PostgreSQL Database with performance optimizations
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.CommandTimeout(30);
            npgsqlOptions.EnableRetryOnFailure(3);
            npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        }
    );
    
    // Enable query caching by default with NoTracking
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    
    // Disable detailed errors in production for performance
    if (builder.Environment.IsProduction())
    {
        options.EnableSensitiveDataLogging(false);
        options.EnableDetailedErrors(false);
    }
});

// Configure Redis
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();
}

// Add memory cache for rate limiting
builder.Services.AddMemoryCache();

// Add HttpContextAccessor for audit logging
builder.Services.AddHttpContextAccessor();

// Configure security options
builder.Services.Configure<SlipVerification.Application.Configuration.JwtConfiguration>(
    builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<SlipVerification.Application.Configuration.PasswordPolicyOptions>(
    builder.Configuration.GetSection("PasswordPolicy"));
builder.Services.Configure<SlipVerification.Application.Configuration.RateLimitOptions>(
    builder.Configuration.GetSection("RateLimit"));

// Register repositories and unit of work
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register performance-optimized repositories
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<SlipVerification.Application.Interfaces.Repositories.ISlipVerificationRepository, 
    SlipVerification.Infrastructure.Data.Repositories.SlipVerificationRepository>();

// Register cached repository decorators (if Redis is available)
if (!string.IsNullOrEmpty(redisConnection))
{
    // Register the cached version as the primary interface
    builder.Services.AddScoped<SlipVerification.Application.Interfaces.Repositories.IOrderRepository>(sp =>
    {
        var innerRepository = sp.GetRequiredService<OrderRepository>();
        var cache = sp.GetRequiredService<ICacheService>();
        return new SlipVerification.Infrastructure.Services.CachedOrderRepository(innerRepository, cache);
    });
}
else
{
    // Register the non-cached version if Redis is not available
    builder.Services.AddScoped<SlipVerification.Application.Interfaces.Repositories.IOrderRepository, OrderRepository>();
}

// Register cache warmup service
builder.Services.AddHostedService<SlipVerification.Infrastructure.Services.CacheWarmupService>();

// Register security repositories
builder.Services.AddScoped<SlipVerification.Application.Interfaces.Repositories.IUserRepository, 
    SlipVerification.Infrastructure.Data.Repositories.Auth.UserRepository>();
builder.Services.AddScoped<SlipVerification.Application.Interfaces.Repositories.IRefreshTokenRepository, 
    SlipVerification.Infrastructure.Data.Repositories.Auth.RefreshTokenRepository>();

// Register security services
builder.Services.AddScoped<SlipVerification.Application.Interfaces.IPasswordHasher, 
    SlipVerification.Infrastructure.Services.Security.BcryptPasswordHasher>();
builder.Services.AddScoped<SlipVerification.Application.Interfaces.IPasswordValidator, 
    SlipVerification.Infrastructure.Services.Security.PasswordValidator>();
builder.Services.AddScoped<SlipVerification.Application.Interfaces.IAuthenticationService, 
    SlipVerification.Infrastructure.Services.Security.AuthenticationService>();
builder.Services.AddScoped<SlipVerification.Application.Interfaces.IUserPermissionService, 
    SlipVerification.Infrastructure.Services.Security.UserPermissionService>();
builder.Services.AddScoped<SlipVerification.Application.Interfaces.IAuditLogger, 
    SlipVerification.Infrastructure.Services.Security.AuditLogger>();

// Register authorization handler
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, 
    SlipVerification.API.Authorization.PermissionAuthorizationHandler>();

// Register application services
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IFileStorageService>(sp =>
{
    var basePath = builder.Configuration["FileStorage:BasePath"] ?? "uploads";
    var baseUrl = builder.Configuration["FileStorage:BaseUrl"] ?? "http://localhost:5000/uploads";
    return new LocalFileStorageService(basePath, baseUrl);
});

// Register notification services
builder.Services.AddNotificationServices(builder.Configuration);

// Register message queue services
builder.Services.AddMessageQueueServices(builder.Configuration);

// Register real-time notification service
builder.Services.AddScoped<IRealtimeNotificationService, RealtimeNotificationService>();

// Configure MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
    Assembly.Load("SlipVerification.Application")));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT secret not configured");
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Configure SignalR authentication events
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/ws"))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    // Role-based policies
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole(SlipVerification.Application.Authorization.Roles.Admin));
    
    options.AddPolicy("ManagerOrAdmin", policy => 
        policy.RequireRole(
            SlipVerification.Application.Authorization.Roles.Manager, 
            SlipVerification.Application.Authorization.Roles.Admin));
    
    // Permission-based policies
    options.AddPolicy("CanViewSlips", policy =>
        policy.AddRequirements(new SlipVerification.Application.Authorization.PermissionRequirement(
            SlipVerification.Application.Authorization.Permissions.ViewSlips)));
    
    options.AddPolicy("CanUploadSlips", policy =>
        policy.AddRequirements(new SlipVerification.Application.Authorization.PermissionRequirement(
            SlipVerification.Application.Authorization.Permissions.UploadSlips)));
    
    options.AddPolicy("CanVerifySlips", policy =>
        policy.AddRequirements(new SlipVerification.Application.Authorization.PermissionRequirement(
            SlipVerification.Application.Authorization.Permissions.VerifySlips)));
    
    options.AddPolicy("CanDeleteSlips", policy =>
        policy.AddRequirements(new SlipVerification.Application.Authorization.PermissionRequirement(
            SlipVerification.Application.Authorization.Permissions.DeleteSlips)));
    
    options.AddPolicy("CanManageUsers", policy =>
        policy.AddRequirements(new SlipVerification.Application.Authorization.PermissionRequirement(
            SlipVerification.Application.Authorization.Permissions.ManageUsers)));
});

// Configure API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Slip Verification API",
        Version = "v1",
        Description = "API for payment slip verification system with QR code support",
        Contact = new OpenApiContact
        {
            Name = "Slip Verification Team",
            Email = "support@slipverification.com"
        }
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "*" };
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure SignalR with Redis backplane
var signalRBuilder = builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.MaximumReceiveMessageSize = 32 * 1024; // 32 KB
});

// Add Redis backplane if Redis is configured
if (!string.IsNullOrEmpty(redisConnection))
{
    signalRBuilder.AddStackExchangeRedis(redisConnection, options =>
    {
        options.Configuration.ChannelPrefix = "SignalR";
    });
}

// Response Compression is already configured above with optimized settings

// Configure Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "")
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

var app = builder.Build();

// Initialize message queues
app.Services.InitializeMessageQueues();

// Configure the HTTP request pipeline
app.UseSerilogRequestLogging();

// Add metrics middleware
app.UseMiddleware<MetricsMiddleware>();

// Enable HTTP metrics collection
app.UseHttpMetrics();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Slip Verification API v1");
        options.RoutePrefix = "swagger";
    });
}

// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

// Enable Response Compression
app.UseResponseCompression();

// Enable CORS
app.UseCors();

// Security headers middleware
app.UseMiddleware<SlipVerification.API.Middleware.Security.SecurityHeadersMiddleware>();

// Input sanitization middleware
app.UseMiddleware<SlipVerification.API.Middleware.Security.InputSanitizationMiddleware>();

// Rate limiting middleware (custom implementation)
app.UseMiddleware<SlipVerification.API.Middleware.Security.RateLimitingMiddleware>();

// Enable Rate Limiting (built-in)
app.UseRateLimiter();

// Enable Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Serve static files (for uploaded slip images)
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.MapControllers();

// Map SignalR Hub endpoint
app.MapHub<NotificationHub>("/ws", options =>
{
    options.Transports = 
        HttpTransportType.WebSockets | 
        HttpTransportType.LongPolling;
});

// Map Health Check endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.ToString()
            }),
            totalDuration = report.TotalDuration.ToString()
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapGet("/", () => Results.Redirect("/swagger"));

// Map Prometheus metrics endpoint
app.MapMetrics("/metrics");

Log.Information("Starting Slip Verification API...");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible to integration tests
public partial class Program { }
