# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | :white_check_mark: |

## Reporting a Vulnerability

We take security seriously. If you discover a security vulnerability, please follow these steps:

### 1. Do Not Disclose Publicly

Please do **NOT** create a public GitHub issue for security vulnerabilities.

### 2. Contact Us Privately

Send an email to: **security@slipverification.example.com**

Include the following information:
- Description of the vulnerability
- Steps to reproduce
- Potential impact
- Suggested fix (if any)

### 3. Response Timeline

- **Acknowledgment**: Within 48 hours
- **Initial Assessment**: Within 1 week
- **Resolution**: Depends on severity (Critical: 7 days, High: 14 days, Medium: 30 days)

## Security Measures

### Authentication & Authorization

- JWT-based authentication with refresh tokens
- Role-based access control (RBAC)
- Password hashing with BCrypt
- Session timeout and token expiration

### Data Protection

- TLS 1.3 encryption in transit
- AES-256 encryption at rest for sensitive data
- Input validation and sanitization
- Parameterized queries (SQL injection prevention)

### Infrastructure

- Web Application Firewall (WAF)
- Rate limiting
- DDoS protection
- Security headers (HSTS, CSP, X-Frame-Options)

### Compliance

- PDPA (Personal Data Protection Act) compliant
- GDPR considerations for international users
- Regular security audits
- Vulnerability scanning with Trivy

## Security Best Practices for Contributors

1. **Never commit secrets** - Use environment variables
2. **Validate all inputs** - Use FluentValidation
3. **Keep dependencies updated** - Run `npm audit` and `dotnet list package --vulnerable`
4. **Follow secure coding guidelines** - OWASP recommendations

## Acknowledgments

We appreciate the security research community's efforts in responsibly disclosing vulnerabilities.

---

*Last Updated: December 2024*
