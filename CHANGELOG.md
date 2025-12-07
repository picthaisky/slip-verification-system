# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Dashboard API endpoints (`/api/v1/dashboard/stats`, `/api/v1/dashboard/recent-activities`, `/api/v1/dashboard/chart-data`)
- Reports API endpoints (`/api/v1/reports/daily`, `/api/v1/reports/monthly`, `/api/v1/reports/export`)
- Reports module UI with daily/monthly reports, data tables, and CSV export
- Dashboard real-time data integration for Web and Mobile
- Mobile dashboard API integration with loading states
- CONTRIBUTING.md, SECURITY.md documentation

### Changed
- Dashboard component now fetches real data from API instead of mock data
- Mobile HomeScreen displays stats from API with fallback to mock data
- Reports page now has full functionality instead of placeholder

### Fixed
- Dashboard showing hardcoded data issue
- Reports page placeholder-only issue

## [1.0.0] - 2024-12-07

### Added
- Initial release of Slip Verification System
- Slip upload and OCR verification
- Multi-channel notifications (LINE, Email, Push)
- User authentication with JWT
- Order management system
- Transaction history
- Real-time updates via SignalR
- Docker and Kubernetes deployment support
- Monitoring with Prometheus/Grafana

### Technical Stack
- Backend: .NET Core 9, Entity Framework Core
- Frontend: Angular 20, Tailwind CSS
- Mobile: React Native
- OCR: Python/FastAPI with PaddleOCR
- Database: PostgreSQL 16
- Cache: Redis 7
- Message Queue: RabbitMQ

---

[Unreleased]: https://github.com/yourusername/slip-verification-system/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/yourusername/slip-verification-system/releases/tag/v1.0.0
