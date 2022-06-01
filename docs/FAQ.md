// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Frequently Asked Questions

### General Questions

**Q: What is Coolify CLI?**

A: Coolify CLI is a .NET-based command-line tool for managing Coolify infrastructure from your terminal. It allows you to deploy applications, manage databases, monitor health, and stream logs without using the web dashboard.

**Q: Why would I use CLI instead of the web dashboard?**

A: CLI is better for:
- Automation and scripting
- CI/CD pipelines
- Batch operations
- Faster workflows for power users
- Infrastructure-as-code approaches
- Remote servers without GUI

**Q: Can I use both CLI and web dashboard?**

A: Yes! They work together perfectly. Changes in the web dashboard are reflected in CLI and vice versa.

**Q: Is CLI production-ready?**

A: Yes. Coolify CLI is used in production environments managing thousands of deployments daily.

### Installation & Setup

**Q: What are the system requirements?**

A: 
- .NET 10 SDK or runtime
- 50MB disk space
- Network access to Coolify instance
- 20-30MB memory during operation

**Q: How do I get an API key?**

A: 
1. Log in to Coolify dashboard
2. Go to Settings → API Keys
3. Click "New API Key"
4. Copy the generated key
5. Store securely (can't be retrieved later)

**Q: Can I use the same API key on multiple machines?**

A: Yes, but it's recommended to create separate keys per machine for security and audit logging.

**Q: How do I uninstall CLI?**

A: 
```bash
# If using symlink
sudo rm /usr/local/bin/coolify-cli

# If using package manager
brew uninstall coolify-cli  # macOS
snap remove coolify-cli     # Linux

# If using Docker
docker image rm sarmkadan/coolify-cli:latest
```

### Configuration

**Q: Where should I store the API key?**

A: Options in order of preference:
1. Environment variable (temporary sessions)
2. `.env` file (project-level)
3. Password manager integration
4. Secured file with restricted permissions

**Q: Can I use multiple API keys?**

A: Not simultaneously, but you can switch:
```bash
export COOLIFY_API_KEY="key1"
coolify-cli app list

export COOLIFY_API_KEY="key2"
coolify-cli app list
```

**Q: What if I forget my API key?**

A: You'll need to generate a new one in the Coolify dashboard. The old key becomes invalid.

**Q: Can I configure CLI per project?**

A: Yes, use `.env` files:
```bash
# Project root
cat > .env << EOF
COOLIFY_API_KEY=sk_xxx
COOLIFY_API_URL=https://coolify.example.com
EOF

# Load it
set -a
source .env
set +a
```

**Q: How do I use different configs for different environments?**

A: Create environment-specific files:
```bash
# .env.dev
COOLIFY_API_URL=https://coolify-dev.example.com

# .env.prod
COOLIFY_API_URL=https://coolify-prod.example.com

# Use as needed
source .env.prod
coolify-cli app deploy 1
```

### Commands & Usage

**Q: How do I list all available commands?**

A:
```bash
coolify-cli --help
coolify-cli app --help
coolify-cli db --help
```

**Q: Can I get output in JSON format?**

A: Yes, most list commands support `--format json`:
```bash
coolify-cli app list --format json | jq '.[] | select(.status=="running")'
```

**Q: How do I filter results?**

A: Use `--filter` option:
```bash
coolify-cli app list --filter running
coolify-cli logs 1 --filter ERROR
```

**Q: Can I combine multiple options?**

A: Yes:
```bash
coolify-cli app list --format json --filter running --limit 50
```

**Q: How long does a deployment take?**

A: Typically 2-5 minutes depending on:
- Application size
- Build time
- Deployment strategy
- Health check duration

Check with:
```bash
coolify-cli app status 1
```

### Deployments

**Q: What's the difference between deployment strategies?**

A:
- **Blue-green**: Zero downtime, instant rollback, needs 2x resources
- **Canary**: Gradual rollout, catch issues early, slower
- **Rolling**: Update instances one by one, no extra resources needed

**Q: Can I deploy multiple apps at once?**

A: Yes, in a script:
```bash
for id in 1 2 3; do
    coolify-cli app deploy "$id" &
done
wait
```

**Q: How do I rollback a deployment?**

A:
```bash
coolify-cli app rollback 1

# Or to specific version
coolify-cli app rollback 1 --version abc123
```

**Q: Can I deploy while maintaining zero downtime?**

A: Yes, use blue-green or canary strategy:
```bash
coolify-cli app deploy 1 --strategy blue-green
```

**Q: What happens if deployment fails?**

A: Automatic rollback to previous version. You can:
```bash
# Check status
coolify-cli app status 1

# View failure logs
coolify-cli logs 1 --lines 200 --filter ERROR

# Manually rollback if needed
coolify-cli app rollback 1
```

### Databases

**Q: How do I backup a database?**

A:
```bash
coolify-cli db backup create 1
```

**Q: Can I backup on a schedule?**

A: Yes, using cron:
```bash
# Daily backup at 2 AM
0 2 * * * coolify-cli db backup create 1
```

**Q: How do I restore a backup?**

A:
```bash
# List backups
coolify-cli db backup list 1

# Restore
coolify-cli db backup restore 1 backup_id
```

**Q: How do I check database health?**

A:
```bash
coolify-cli db health 1
```

**Q: Can I run database migrations?**

A: Yes:
```bash
coolify-cli db migrate 1
```

### Logs

**Q: How do I view logs?**

A:
```bash
# Last 100 lines
coolify-cli logs 1

# Last 50 lines
coolify-cli logs 1 --lines 50

# Stream live
coolify-cli logs 1 --follow
```

**Q: Can I filter logs?**

A: Yes:
```bash
# Only errors
coolify-cli logs 1 --filter ERROR

# Only warnings
coolify-cli logs 1 --filter WARN
```

**Q: How long are logs retained?**

A: Depends on Coolify configuration, typically 7-30 days.

**Q: Can I export logs?**

A: Yes:
```bash
# To file
coolify-cli logs 1 > app.log

# As JSON
coolify-cli logs 1 --format json > app.json
```

### Environment Variables

**Q: How do I set environment variables?**

A:
```bash
coolify-cli app env set 1 MY_VAR "my_value"
```

**Q: Can I encrypt environment variables?**

A: Yes:
```bash
coolify-cli app env set 1 API_KEY "secret" --encrypted
```

**Q: How do I manage secrets securely?**

A:
1. Use `--encrypted` flag
2. Never log them
3. Rotate regularly
4. Use minimal privilege keys

**Q: Can I delete environment variables?**

A: Yes:
```bash
coolify-cli app env delete 1 MY_VAR
```

### Scripting & Automation

**Q: Can I use CLI in scripts?**

A: Yes, it's designed for automation:
```bash
#!/bin/bash
for app_id in 1 2 3; do
    coolify-cli app deploy "$app_id"
done
```

**Q: How do I handle errors in scripts?**

A: Check exit codes:
```bash
if ! coolify-cli app deploy 1; then
    echo "Deployment failed!"
    exit 1
fi
```

**Q: Can I integrate with CI/CD?**

A: Yes:
```yaml
# GitHub Actions example
- name: Deploy
  env:
    COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}
  run: coolify-cli app deploy 1
```

**Q: How do I retry failed operations?**

A: Manual retry or use a loop:
```bash
retry_count=0
until coolify-cli app deploy 1; do
    retry_count=$((retry_count + 1))
    if [ $retry_count -ge 3 ]; then
        exit 1
    fi
    sleep 10
done
```

### Performance & Optimization

**Q: Why is my command slow?**

A: Check:
1. Network latency: `ping coolify.example.com`
2. API server: `coolify-cli health --verbose`
3. Request timeout: increase `COOLIFY_TIMEOUT`
4. Disable caching: `COOLIFY_CACHE_ENABLED=false`

**Q: Can I cache results?**

A: Yes, caching is enabled by default:
```bash
export COOLIFY_CACHE_ENABLED=true
export COOLIFY_CACHE_TTL=300  # 5 minutes
```

**Q: How do I improve batch operation speed?**

A: Run operations in parallel:
```bash
for id in 1 2 3 4 5; do
    coolify-cli app deploy "$id" &  # Background
done
wait  # Wait for all
```

### Troubleshooting

**Q: Connection fails with timeout?**

A:
```bash
# Increase timeout
export COOLIFY_TIMEOUT=60

# Check connectivity
curl -v https://your-coolify-instance.com

# Check network
ping your-coolify-instance.com
```

**Q: "Invalid API key" error?**

A:
```bash
# Verify key
echo $COOLIFY_API_KEY

# Check format (should start with sk_ or pk_)
# Regenerate in dashboard if needed
```

**Q: High memory usage?**

A:
```bash
# Disable caching
export COOLIFY_CACHE_ENABLED=false

# Paginate large result sets
coolify-cli app list --limit 50
```

**Q: Intermittent connection failures?**

A:
```bash
# Enable retries
export COOLIFY_RETRY_ATTEMPTS=5
export COOLIFY_RETRY_DELAY_MS=2000

# Check network stability
mtr your-coolify-instance.com
```

### Security

**Q: Is API communication encrypted?**

A: Yes, HTTPS is required. HTTP requests are rejected.

**Q: Can someone intercept my API key?**

A: If using HTTPS (which is required), traffic is encrypted. However:
- Don't commit API keys to version control
- Don't share API keys
- Use separate keys for different machines
- Rotate keys regularly

**Q: Can I use API key in scripts?**

A: Yes, but store in:
- Environment variables
- `.env` files with restricted permissions
- Secrets manager integration
- Not in plain text in scripts

**Q: How do I rotate API keys?**

A:
1. Generate new key in Coolify dashboard
2. Update all scripts/configs
3. Delete old key
4. Verify all systems work

### Advanced Usage

**Q: Can I use CLI with other tools?**

A: Yes:
```bash
# With jq for JSON processing
coolify-cli app list --format json | jq '.[] | .name'

# With grep for filtering
coolify-cli logs 1 | grep ERROR

# With awk for parsing
coolify-cli app list | awk '{print $2}'
```

**Q: Can I extend CLI with custom commands?**

A: Not directly, but you can create wrapper scripts:
```bash
#!/bin/bash
# my-deploy.sh

export COOLIFY_API_KEY="sk_xxx"
coolify-cli app deploy "$@"
```

**Q: Can I use CLI in Docker?**

A: Yes:
```bash
docker run --rm \
  -e COOLIFY_API_KEY \
  -e COOLIFY_API_URL \
  sarmkadan/coolify-cli:latest \
  coolify-cli app list
```

### Contributing & Support

**Q: How do I report a bug?**

A: Open an issue on [GitHub](https://github.com/Sarmkadan/coolify-cli/issues) with:
- Reproduction steps
- Expected behavior
- Actual behavior
- Output with `--verbose`

**Q: Can I contribute?**

A: Yes! See [Contributing](../README.md#contributing) section.

**Q: Where do I get help?**

A: 
- Check this FAQ first
- Read documentation at https://github.com/Sarmkadan/coolify-cli
- Open an issue on GitHub
- Contact maintainer

**Q: Is there commercial support?**

A: Contact via https://sarmkadan.com for enterprise support.

### Version & Updates

**Q: How do I update CLI?**

A:
```bash
# From source
git pull
dotnet build -c Release

# From package manager
brew upgrade coolify-cli  # macOS
snap refresh coolify-cli  # Linux

# From Docker
docker pull sarmkadan/coolify-cli:latest
```

**Q: What's the versioning scheme?**

A: Semantic Versioning:
- `MAJOR.MINOR.PATCH`
- `1.0.0` - breaking changes
- `1.1.0` - new features
- `1.0.1` - bug fixes

**Q: How long are versions supported?**

A: Latest 2 major versions receive updates. Check [CHANGELOG](../CHANGELOG.md).

**Q: Can I pin a specific version?**

A: Yes:
```bash
# From Docker
docker run sarmkadan/coolify-cli:1.2.0

# From source
git checkout v1.2.0
```

---

For more help, see [Getting Started](GETTING_STARTED.md) or [API Reference](API_REFERENCE.md).
