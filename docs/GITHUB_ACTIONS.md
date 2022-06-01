# GitHub Actions Integration

Complete guide for integrating Coolify CLI with GitHub Actions for automated deployments.

## Quick Start

### 1. Add Secrets

In your GitHub repository settings:

1. Go to **Settings** → **Secrets and variables** → **Actions**
2. Add these secrets:
   - `COOLIFY_API_KEY`: Your Coolify API key
   - `COOLIFY_API_URL`: Your Coolify instance URL

### 2. Create Workflow

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Deploy with Coolify CLI
        uses: docker://sarmkadan/coolify-cli:latest
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}
          COOLIFY_API_URL: ${{ secrets.COOLIFY_API_URL }}
        with:
          args: app deploy 1
```

## Complete Workflow Example

```yaml
name: Deploy Application

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  COOLIFY_API_URL: ${{ secrets.COOLIFY_API_URL }}

jobs:
  test:
    name: Test
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Run tests
        run: dotnet test

  deploy-staging:
    name: Deploy to Staging
    runs-on: ubuntu-latest
    needs: test
    if: github.ref == 'refs/heads/develop'
    steps:
      - name: Deploy Application
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY_STAGING }}
        run: |
          curl -sSL https://sh.sarmkadan.com/install.sh | sh
          coolify-cli app deploy 2

      - name: Health Check
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY_STAGING }}
        run: |
          coolify-cli app status 2 --wait

  deploy-production:
    name: Deploy to Production
    runs-on: ubuntu-latest
    needs: test
    if: github.ref == 'refs/heads/main' && startsWith(github.ref, 'refs/tags/')
    environment: production
    steps:
      - name: Deploy Application
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}
        run: |
          coolify-cli app deploy 1

      - name: Verify Deployment
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}
        run: |
          coolify-cli app status 1
          coolify-cli health

  rollback:
    name: Rollback on Failure
    runs-on: ubuntu-latest
    needs: deploy-production
    if: failure()
    steps:
      - name: Rollback Deployment
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}
        run: |
          coolify-cli app rollback 1

      - name: Notify Team
        uses: actions/github-script@v6
        with:
          github-token: ${{ secrets.GITHUB_TOKEN }}
          script: |
            github.rest.issues.createComment({
              issue_number: context.issue.number,
              owner: context.repo.owner,
              repo: context.repo.repo,
              body: '❌ Deployment failed and was rolled back'
            })
```

## Matrix Deployments

Deploy to multiple applications:

```yaml
jobs:
  deploy:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        app_id: [1, 2, 3]
        include:
          - app_id: 1
            name: api
            environment: staging
          - app_id: 2
            name: web
            environment: staging
          - app_id: 3
            name: worker
            environment: production
    steps:
      - name: Deploy ${{ matrix.name }}
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}
        run: coolify-cli app deploy ${{ matrix.app_id }}
```

## Environment Promotion

Deploy through environments:

```yaml
jobs:
  deploy-dev:
    name: Deploy to Dev
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/develop'
    environment: development
    steps:
      - name: Deploy
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY_DEV }}
        run: coolify-cli app deploy 1

  deploy-staging:
    name: Deploy to Staging
    runs-on: ubuntu-latest
    needs: deploy-dev
    environment: staging
    steps:
      - name: Request Approval
        uses: actions/github-script@v6
        with:
          script: |
            console.log('Awaiting approval for staging deployment')

      - name: Deploy
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY_STAGING }}
        run: coolify-cli app deploy 2

  deploy-production:
    name: Deploy to Production
    runs-on: ubuntu-latest
    needs: deploy-staging
    if: github.ref == 'refs/heads/main'
    environment: production
    steps:
      - name: Deploy
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}
        run: coolify-cli app deploy 3
```

## Database Operations

```yaml
jobs:
  database-backup:
    name: Backup Database
    runs-on: ubuntu-latest
    if: github.event_name == 'schedule'
    steps:
      - name: Create Database Backup
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}
        run: coolify-cli db backup create 1

      - name: Upload Backup
        uses: actions/upload-artifact@v3
        with:
          name: database-backup
          path: ./backups/

  database-restore:
    name: Restore Database
    runs-on: ubuntu-latest
    if: github.event_name == 'workflow_dispatch'
    inputs:
      backup_id:
        description: 'Backup ID to restore'
        required: true
    steps:
      - name: Restore Database
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}
        run: coolify-cli db restore 1 ${{ github.event.inputs.backup_id }}
```

## Notifications

Send deployment notifications:

```yaml
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Deploy Application
        id: deploy
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}
        run: coolify-cli app deploy 1

      - name: Notify Slack on Success
        if: success()
        uses: slackapi/slack-github-action@v1.24
        with:
          payload: |
            {
              "text": "✅ Deployment successful",
              "blocks": [
                {
                  "type": "section",
                  "text": {
                    "type": "mrkdwn",
                    "text": "*Deployment Successful*\nApplication: MyApp\nBranch: main"
                  }
                }
              ]
            }
        env:
          SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}

      - name: Notify Slack on Failure
        if: failure()
        uses: slackapi/slack-github-action@v1.24
        with:
          payload: |
            {
              "text": "❌ Deployment failed",
              "blocks": [
                {
                  "type": "section",
                  "text": {
                    "type": "mrkdwn",
                    "text": "*Deployment Failed*\nApplication: MyApp\nCheck logs: ${{ github.server_url }}/${{ github.repository }}/actions/runs/${{ github.run_id }}"
                  }
                }
              ]
            }
        env:
          SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}
```

## Docker Action

Use Coolify CLI directly in a Docker container:

```yaml
jobs:
  deploy:
    runs-on: ubuntu-latest
    container:
      image: sarmkadan/coolify-cli:latest
    steps:
      - name: Deploy Application
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}
          COOLIFY_API_URL: ${{ secrets.COOLIFY_API_URL }}
        run: |
          coolify-cli app list
          coolify-cli app deploy 1
```

## Advanced Patterns

### Blue-Green Deployment

```yaml
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Deploy to Green Environment
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}
        run: coolify-cli app deploy 1 --strategy blue-green

      - name: Run Smoke Tests
        run: |
          curl -f https://staging.example.com/health || exit 1

      - name: Switch Traffic
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}
        run: coolify-cli app switch-traffic 1
```

### Canary Deployment

```yaml
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Deploy Canary
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}
        run: coolify-cli app deploy 1 --strategy canary --canary-percentage 10

      - name: Monitor Metrics
        run: |
          sleep 300
          curl -f https://monitoring.example.com/api/metrics || exit 1

      - name: Increase Traffic
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}
        run: coolify-cli app update-canary 1 --percentage 100
```

## Scheduled Deployments

Deploy on a schedule:

```yaml
name: Scheduled Deployment

on:
  schedule:
    - cron: '0 2 * * *'  # Every day at 2 AM

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Deploy Application
        env:
          COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}
        run: coolify-cli app deploy 1
```

## Troubleshooting

### Secret Not Available

Ensure secrets are available in the repository:
- Check repository settings
- Verify secret names match exactly
- Check branch/environment permissions

### Authentication Failures

```yaml
- name: Debug Authentication
  env:
    COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}
  run: |
    coolify-cli health --verbose
    echo "API URL: $COOLIFY_API_URL"
```

### Timeout Issues

```yaml
- name: Deploy with Extended Timeout
  env:
    COOLIFY_API_KEY: ${{ secrets.COOLIFY_API_KEY }}
    COOLIFY_TIMEOUT: 120
  run: coolify-cli app deploy 1
```

## Best Practices

1. **Use Environments** for production deployments
2. **Require Approvals** for production changes
3. **Store Secrets** securely in GitHub
4. **Add Notifications** for deployment status
5. **Include Health Checks** after deployment
6. **Implement Rollback** on failure
7. **Use Matrix Strategy** for multiple deployments
8. **Log Deployment Details** for troubleshooting
9. **Test in Staging** before production
10. **Document Your Workflow** for your team

## Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Coolify CLI Documentation](../README.md)
- [GitHub Secrets Documentation](https://docs.github.com/en/actions/security-guides/encrypted-secrets)
