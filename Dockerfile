# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["SlipVerification.sln", "./"]
COPY ["src/SlipVerification.API/SlipVerification.API.csproj", "src/SlipVerification.API/"]
COPY ["src/SlipVerification.Application/SlipVerification.Application.csproj", "src/SlipVerification.Application/"]
COPY ["src/SlipVerification.Domain/SlipVerification.Domain.csproj", "src/SlipVerification.Domain/"]
COPY ["src/SlipVerification.Infrastructure/SlipVerification.Infrastructure.csproj", "src/SlipVerification.Infrastructure/"]
COPY ["src/SlipVerification.Shared/SlipVerification.Shared.csproj", "src/SlipVerification.Shared/"]

# Restore dependencies
RUN dotnet restore "src/SlipVerification.API/SlipVerification.API.csproj"

# Copy everything else
COPY . .

# Build the application
WORKDIR "/src/src/SlipVerification.API"
RUN dotnet build "SlipVerification.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "SlipVerification.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install PostgreSQL client for health checks (optional)
RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Create uploads directory
RUN mkdir -p /app/uploads

# Expose ports
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "SlipVerification.API.dll"]
