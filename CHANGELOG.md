// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Changelog

All notable changes to Coolify CLI are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-05-04

### Added
- New `app status` command for real-time deployment monitoring
- HTML report generation for log analysis
- Canary deployment strategy with automatic rollback
- Multi-application deployment orchestration
- Concurrent deployment support
- Environment variable scoping (application vs global)
- Batch database backup operations
- Performance metrics collection and reporting
- Webhook integration for deployment events
- Slack and email alerting for health checks
- JSON output format for all list commands
- CSV export functionality
- Health check endpoint configuration
- Resource quota management and enforcement
- Rate limiting configuration per environment

### Changed
- Improved error messages with actionable suggestions
- Enhanced logging with structured output
- Faster API response caching (5-minute TTL)
- Database health checks now include error rate metrics
- Deployment timeout increased from 300s to 600s default
- Log retrieval now supports filtering by timestamp
- Refactored middleware chain for better performance
- Updated documentation with more examples
- Improved test coverage (85% → 92%)

### Fixed
- Fixed race condition in concurrent deployments
- Resolved memory leak in log streaming
- Fixed API key validation for special characters
- Corrected timezone handling in log timestamps
- Fixed pagination in large result sets
- Resolved connection pooling issues
- Fixed intermittent timeouts on slow networks

### Deprecated
- `--format table` will be removed in v2.0, use `--format text` instead

### Security
- Added HTTPS enforcement for all API communication
- Implemented secret masking in verbose logs
- Added API key rotation support
- Improved input validation and sanitization
- Fixed potential command injection vulnerability
- Added rate limit protection

## [1.1.0] - 2026-04-01

### Added
- Blue-green deployment strategy
- Rolling deployment strategy
- Database backup and restore functionality
- Application environment variable management
- Health check service with metrics
- Log filtering by log level
- Verbose logging mode
- Configuration validation on startup
- Automatic retry with exponential backoff
- Memory-based response caching
- Pre-deployment validation checks
- Post-deployment health verification

### Changed
- Improved CLI command hierarchy
- Enhanced error messages
- Better performance metrics reporting
- Simplified environment configuration

### Fixed
- Fixed API connection timeout handling
- Resolved issue with special characters in passwords
- Fixed log streaming interruption

## [1.0.0] - 2026-02-15

### Added
- Initial release of Coolify CLI
- Application listing and management
- Application deployment
- Database listing and health checks
- Log viewing functionality
- System health checks
- Environment variable management
- Basic command-line interface
- Configuration via environment variables
- .env file support
- Documentation and examples

### Features
- List all applications with status
- Get detailed application information
- Deploy applications
- View application logs
- List databases
- Check database health
- View system health
- Manage environment variables
- Support for multiple deployment strategies
- Color-coded console output
- JSON output format

### Infrastructure
- .NET 10 build system
- Cross-platform support (Linux, macOS, Windows)
- Docker containerization
- GitHub Actions CI/CD workflow

---

## Version History

| Version | Release Date | Status | Notes |
|---------|-------------|--------|-------|
| 1.2.0   | 2026-05-04  | Latest | Full production features |
| 1.1.0   | 2026-04-01  | Stable | Deployment strategies added |
| 1.0.0   | 2026-02-15  | Stable | Initial release |

---

## Upgrade Guide

### From 1.0.0 to 1.1.0

No breaking changes. Simply update the binary:

```bash
dotnet publish -c Release
```

### From 1.1.0 to 1.2.0

No breaking changes. New features are optional.

```bash
# Update configuration for new features (optional)
export CANARY_INITIAL_TRAFFIC=5
export CANARY_INCREMENT=10
```

---

## Roadmap

### Next Release (1.3.0) - Q3 2026

- [ ] Interactive TUI mode for guided operations
- [ ] Plugin system for custom commands
- [ ] Webhook server for receiving Coolify events
- [ ] Configuration profiles for multiple environments
- [ ] Multi-account support
- [ ] Metrics export (Prometheus)
- [ ] Scheduled deployments

### Future Releases

- [ ] OIDC/OAuth authentication
- [ ] Local workspace management
- [ ] Data persistence layer
- [ ] Advanced log querying with full-text search
- [ ] Cost estimation and tracking
- [ ] Capacity planning tools
- [ ] Performance optimization recommendations
- [ ] Integration with monitoring platforms (DataDog, New Relic)

---

## End of Support

| Version | Released | Support Ends | Notes |
|---------|----------|-------------|-------|
| 1.2.x   | 2026-05  | 2027-05     | LTS (18 months) |
| 1.1.x   | 2026-04  | 2026-11     | 6 months |
| 1.0.x   | 2026-02  | 2026-08     | 6 months |

---

## Known Issues

### v1.2.0

- **Issue**: Large log files (>10MB) may consume excessive memory
  - **Workaround**: Retrieve logs with `--lines` limit
  - **Fix**: Scheduled for v1.3.0

- **Issue**: Canary deployment may not update all instances immediately
  - **Workaround**: Use blue-green deployment for critical apps
  - **Status**: Under investigation

### v1.1.0

- **Issue**: Blue-green deployment fails with some custom domains
  - **Workaround**: Use rolling deployment
  - **Status**: Fixed in v1.2.0

---

## Contributors

- **Vladyslav Zaiets** (@Sarmkadan) - Author, CTO & Software Architect

## License

MIT License - See LICENSE file for details.

---

For more information, visit:
- Repository: https://github.com/Sarmkadan/coolify-cli
- Website: https://sarmkadan.com
- Issues: https://github.com/Sarmkadan/coolify-cli/issues
