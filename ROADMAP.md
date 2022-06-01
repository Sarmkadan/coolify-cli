# Product Roadmap

This document outlines the planned features and improvements for Coolify CLI. Items are prioritized based on user feedback and strategic goals.

## Version History

- **v1.2.0** (Current - May 2026)
  - Deployment strategies support
  - Advanced logging and filtering
  - Performance optimizations

- **v1.1.0** (April 2026)
  - Database management commands
  - Health check improvements
  - Configuration caching

- **v1.0.0** (January 2026)
  - Initial release
  - Core application commands
  - Basic monitoring

## Planned Releases

### v1.3.0 (Q3 2026)

**Focus: Enhanced Automation & Intelligence**

- [ ] **Scheduled Deployments**
  - Deploy at specific times
  - Cron-like scheduling support
  - Timezone-aware scheduling

- [ ] **Smart Notifications**
  - Email alerts on deployment failure
  - Slack integration for real-time updates
  - Custom webhook support

- [ ] **Advanced Monitoring**
  - Metrics aggregation
  - Trend analysis
  - Anomaly detection

**Status**: In Planning

### v1.4.0 (Q4 2026)

**Focus: Infrastructure as Code**

- [ ] **Configuration as Code**
  - YAML/JSON deployment manifests
  - Version control integration
  - Configuration validation

- [ ] **Multi-Environment Support**
  - Environment profiles
  - Configuration promotion workflow
  - Environment templates

- [ ] **Policy Engine**
  - Deployment policies
  - Resource quotas
  - Compliance checks

**Status**: Scoped

### v2.0.0 (2027)

**Focus: Enterprise Features**

- [ ] **RBAC (Role-Based Access Control)**
  - Fine-grained permissions
  - Team management
  - Audit logging

- [ ] **Advanced Analytics**
  - Deployment analytics
  - Cost tracking
  - Performance insights

- [ ] **Multi-Cloud Support**
  - AWS, Azure, GCP integration
  - Kubernetes support
  - Hybrid cloud management

- [ ] **GraphQL API**
  - Complex queries support
  - Real-time subscriptions
  - Advanced filtering

**Status**: Vision

## Planned Features

### High Priority

#### 1. Webhook Integrations
- Trigger deployments from external events
- Publish events to webhooks
- **Timeline**: v1.3.0
- **Status**: Design phase

#### 2. Batch Operations
- Deploy multiple applications at once
- Bulk environment variable updates
- **Timeline**: v1.3.0
- **Status**: Development started

#### 3. Metrics Export
- Prometheus metrics export
- Grafana dashboard templates
- **Timeline**: v1.3.0
- **Status**: Not started

### Medium Priority

#### 4. Configuration Validation
- Pre-deployment validation
- Policy enforcement
- **Timeline**: v1.4.0
- **Status**: Not started

#### 5. Database Migration Tools
- Schema migration support
- Data export/import
- **Timeline**: v1.4.0
- **Status**: Not started

#### 6. Blue-Green Deployment Enhancements
- Automated traffic switching
- Health validation
- **Timeline**: v1.3.0
- **Status**: In progress

### Low Priority

#### 7. GUI Dashboard
- Web-based dashboard
- Real-time monitoring
- **Timeline**: v2.0.0
- **Status**: Not started

#### 8. Mobile App
- iOS/Android companion app
- Notifications
- **Timeline**: v2.0.0
- **Status**: Future consideration

## Current Work

### In Progress

**Performance Optimization (v1.2.1)**
- [ ] Connection pooling improvements
- [ ] Cache efficiency optimization
- [ ] Memory footprint reduction
- **ETA**: June 2026

**Docker Image Optimization (v1.2.1)**
- [ ] Multi-stage build for smaller image
- [ ] Security vulnerability scan
- [ ] Performance benchmarking
- **ETA**: June 2026

### Recently Completed

✅ **Deployment Strategies (v1.2.0)**
- Blue-green deployments
- Canary deployments
- Rolling deployments

✅ **Advanced Logging (v1.2.0)**
- Log filtering
- Color-coded output
- Multiple formatters

✅ **Health Check Service (v1.1.0)**
- Database health monitoring
- Application health checks
- Performance metrics

## Feature Requests & Voting

We welcome feature requests from the community! Please:

1. Check if feature is already planned
2. Search existing issues
3. Open new issue with:
   - Clear description
   - Use case
   - Proposed solution
   - Examples

**Top Requested Features:**
1. Scheduled deployments (45 votes)
2. Slack integration (38 votes)
3. Batch operations (32 votes)
4. Configuration files (28 votes)
5. Metrics export (25 votes)

## Technical Debt

Items to address in future releases:

- [ ] Refactor CoolifyApiClient for better testability
- [ ] Simplify middleware chain
- [ ] Improve error messages
- [ ] Add more integration tests
- [ ] Optimize memory usage
- [ ] Improve logging performance

**Priority**: Medium
**Timeline**: Ongoing

## Breaking Changes

Planned breaking changes for v2.0.0:

- Requires .NET 10+ (dropping .NET 8 support)
- API response structure changes
- New environment variable names
- Command syntax updates

**Migration guide** will be provided for smooth transition.

## Platform Support

### Currently Supported

- ✅ Linux (Ubuntu 20.04+, CentOS 8+, Debian 11+)
- ✅ macOS (11.0+)
- ✅ Windows 10/11

### Planned

- 🔄 Windows Server 2019+
- 🔄 Alpine Linux
- 🔄 ARM64 (Raspberry Pi)

## Dependencies & Integrations

### Current

- ✅ .NET 10
- ✅ System.CommandLine
- ✅ HttpClient for API communication
- ✅ JSON serialization

### Planned

- 🔄 Kubernetes client library
- 🔄 Prometheus client library
- 🔄 gRPC support
- 🔄 Message queue support

## Community Contributions

We're looking for contributions in these areas:

1. **Documentation**
   - Tutorials
   - Use case examples
   - Video guides

2. **Integrations**
   - CI/CD plugins
   - IDE extensions
   - Monitoring system integration

3. **Features**
   - Command implementations
   - Output formatters
   - Middleware components

4. **Testing**
   - Test cases
   - Edge case scenarios
   - Performance tests

See [CONTRIBUTING.md](CONTRIBUTING.md) for details.

## Feedback & Suggestions

Share your thoughts:

- **GitHub Issues**: https://github.com/Sarmkadan/coolify-cli/issues
- **Discussions**: https://github.com/Sarmkadan/coolify-cli/discussions
- **Email**: feedback@sarmkadan.com
- **Telegram**: @sarmkadan

Your input shapes the future of Coolify CLI!

## Release Schedule

Releases follow semantic versioning:

- **Major versions**: Annual or when significant features arrive
- **Minor versions**: Quarterly
- **Patch versions**: As needed for bug fixes
- **Pre-release versions**: Available for testing

### Next Releases

| Version | Target Date | Focus |
|---------|------------|-------|
| v1.2.1 | June 2026 | Bug fixes & optimization |
| v1.3.0 | September 2026 | Automation & Intelligence |
| v1.4.0 | December 2026 | Infrastructure as Code |
| v2.0.0 | June 2027 | Enterprise Features |

## Deprecation Policy

Features are deprecated with:

1. Announcement in CHANGELOG
2. Warning messages in CLI output
3. 2 minor version grace period
4. Removal in major version

Example: Feature deprecated in v1.2.0 is removed in v2.0.0.

---

Thank you for your interest in Coolify CLI's future! We're excited to build something great together.
