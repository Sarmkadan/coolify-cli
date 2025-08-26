# Security Policy

## Reporting Security Issues

**Please do not open public issues for security vulnerabilities.**

If you discover a security vulnerability, please email security details to:
📧 **security@sarmkadan.com**

Include:
- Description of vulnerability
- Steps to reproduce
- Potential impact
- Suggested fix (if available)

We will acknowledge receipt within 48 hours and provide updates as we investigate.

## Security Best Practices

### API Key Management

**Never commit API keys to version control:**

```bash
# ❌ Bad - API key exposed
export COOLIFY_API_KEY="sk_prod_xxxxxxxxxx"

# ✅ Good - Use environment files
# Add .env to .gitignore
echo "COOLIFY_API_KEY=sk_prod_xxxxxxxxxx" > .env
export $(cat .env | xargs)

# ✅ Better - Use CI/CD secrets
# GitHub Actions: Settings > Secrets
# GitLab CI: Project > CI/CD > Variables
# Jenkins: Credentials > System > Global credentials
```

### Configuration Security

**Best practices for configuration:**

1. **Use strong, unique API keys**
   - Regenerate keys regularly
   - Rotate keys in case of compromise
   - Use environment-specific keys

2. **Restrict API key permissions**
   - Grant minimum required permissions
   - Use read-only keys for monitoring
   - Separate deployment keys from read keys

3. **Secure network communication**
   - Always use HTTPS (verify TLS/SSL)
   - Validate SSL certificates
   - Use VPN for sensitive operations

4. **Protect configuration files**
   ```bash
   # Set restrictive permissions
   chmod 600 ~/.coolify-cli.config
   chmod 600 .env
   ```

### Authentication

**API authentication methods:**

1. **API Key Authentication** (recommended)
   ```bash
   export COOLIFY_API_KEY="your-api-key"
   ```

2. **Token-based Authentication**
   - Use short-lived tokens
   - Refresh tokens regularly
   - Store tokens securely

### Authorization

**The CLI respects Coolify's authorization model:**

- Verify API key has required permissions
- Check application/database ownership
- Validate user roles and permissions
- Use principle of least privilege

## Vulnerability Management

### Dependency Security

**Dependencies are regularly scanned for vulnerabilities:**

```bash
# Check for vulnerable dependencies
dotnet list package --vulnerable

# Update vulnerable packages
dotnet package update --security
```

### Code Security

**Security features implemented:**

1. **Input Validation**
   - Validate all user input
   - Prevent injection attacks
   - Sanitize output for display

2. **Error Handling**
   - Don't expose sensitive information in errors
   - Log security events
   - Handle secrets securely in memory

3. **Secrets Management**
   - API keys cleared from memory after use
   - Sensitive data not logged
   - Secure storage of cached credentials

4. **Rate Limiting**
   - API requests rate-limited
   - Exponential backoff for retries
   - Protection against brute force

### Secure Coding Practices

**Code review checklist for security:**

- [ ] No hardcoded secrets or credentials
- [ ] Input validation on all user-provided data
- [ ] Proper error handling without information leaks
- [ ] Use of secure APIs (no deprecated crypto)
- [ ] Parameterized queries for database access
- [ ] HTTPS for all external communications
- [ ] No serialization of sensitive objects
- [ ] Proper disposal of resources
- [ ] No buffer overflows or memory issues

## Security Updates

### Notification

You will be notified of security updates via:
- GitHub Security Advisories
- Email notifications for security releases
- Release notes in CHANGELOG.md

### Patching Timeline

- **Critical (CVSS 9-10)**: 24-48 hours
- **High (CVSS 7-8.9)**: 1 week
- **Medium (CVSS 4-6.9)**: 2 weeks
- **Low (CVSS 0-3.9)**: Next minor release

## Compliance

### Supported Versions

Security patches are provided for:
- **Current version**: Full support
- **Previous minor version**: Security patches only
- **Older versions**: No support (upgrade recommended)

### Security Audit

The codebase undergoes regular security reviews:
- Static code analysis
- Dependency vulnerability scanning
- Manual security assessment
- Penetration testing

## Docker Security

**Docker image best practices:**

```bash
# Always use specific versions (not latest)
docker run sarmkadan/coolify-cli:1.0.0

# Run as non-root user
docker run -u 1000:1000 sarmkadan/coolify-cli

# Use read-only filesystem when possible
docker run --read-only sarmkadan/coolify-cli

# Don't pass secrets via environment
# Use Docker secrets or CI/CD secret management
docker secret create coolify_api_key -
docker service create --secret coolify_api_key \
  sarmkadan/coolify-cli
```

## CI/CD Security

### GitHub Actions

**Secure CI/CD pipeline:**

```yaml
# Use specific action versions
- uses: actions/checkout@v4
- uses: actions/setup-dotnet@v4

# Use secrets for sensitive data
env:
  COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}

# Run security checks
- name: Run SAST
  run: dotnet format --verify-no-changes

- name: Check dependencies
  run: dotnet list package --vulnerable
```

### Secrets Management

**Guidelines for CI/CD secrets:**

1. Never print secrets in logs
2. Mask secrets in output
3. Use separate keys per environment
4. Rotate keys regularly
5. Audit secret access

## Web Security

**If running CLI output on web:**

- Sanitize HTML output
- Prevent XSS attacks
- Use Content Security Policy headers
- Validate and escape JSON output

## Memory Safety

**Memory protection:**

- Sensitive data overwritten after use
- No sensitive data in debug output
- Proper resource disposal
- Memory leak prevention through testing

## Network Security

**Network hardening:**

1. **TLS/SSL Configuration**
   - Enforce TLS 1.3 minimum
   - Validate certificates
   - Use secure ciphers

2. **HTTP Headers**
   ```
   Strict-Transport-Security: max-age=31536000
   X-Content-Type-Options: nosniff
   X-Frame-Options: DENY
   ```

3. **API Rate Limiting**
   - Prevent brute force attacks
   - Monitor for suspicious patterns
   - Log security events

## Incident Response

**In case of security incident:**

1. **Immediate Action**
   - Regenerate compromised API keys
   - Revoke tokens
   - Disable affected accounts

2. **Investigation**
   - Review logs
   - Identify scope of compromise
   - Check for unauthorized access

3. **Notification**
   - Inform affected users
   - Publish security advisory
   - Provide remediation steps

4. **Prevention**
   - Implement fixes
   - Update security practices
   - Add monitoring

## Security Checklist

**Before deploying Coolify CLI in production:**

- [ ] API key stored securely (environment variable or secrets manager)
- [ ] COOLIFY_API_URL uses HTTPS
- [ ] TLS certificate validation enabled
- [ ] Audit logging configured
- [ ] Rate limiting enabled
- [ ] API key regularly rotated
- [ ] Network access restricted (firewall/VPN)
- [ ] Monitoring and alerting configured
- [ ] Incident response plan in place
- [ ] Regular security updates applied

## Resources

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [CWE/SANS Top 25](https://cwe.mitre.org/top25/)
- [Microsoft Security Best Practices](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/security-rules)
- [.NET Security Documentation](https://docs.microsoft.com/en-us/dotnet/standard/security/)

## Contact

For security concerns:
- 📧 Email: security@sarmkadan.com
- 📱 Telegram: @sarmkadan
- 🌐 Website: https://sarmkadan.com
