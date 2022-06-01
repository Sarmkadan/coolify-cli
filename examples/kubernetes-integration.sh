#!/bin/bash

# =============================================================================
# Kubernetes Integration Script
# Deploy Coolify CLI in Kubernetes and manage applications
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

set -euo pipefail

NAMESPACE="${NAMESPACE:-coolify}"
APP_ID="${1:?Error: APP_ID required}"
KUBECONFIG="${KUBECONFIG:-$HOME/.kube/config}"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[✓]${NC} $1"; }
log_error() { echo -e "${RED}[✗]${NC} $1"; }

# Check if kubectl is available
check_kubectl() {
    if ! command -v kubectl &> /dev/null; then
        log_error "kubectl not found. Please install kubectl."
        exit 1
    fi
    log_success "kubectl found"
}

# Create Kubernetes namespace
create_namespace() {
    log_info "Creating Kubernetes namespace: $NAMESPACE"

    if ! kubectl get namespace "$NAMESPACE" > /dev/null 2>&1; then
        kubectl create namespace "$NAMESPACE"
        log_success "Namespace created"
    else
        log_info "Namespace already exists"
    fi
}

# Create ConfigMap for Coolify CLI configuration
create_configmap() {
    log_info "Creating ConfigMap for Coolify CLI configuration..."

    kubectl create configmap coolify-config \
        --from-literal=COOLIFY_API_URL="$COOLIFY_API_URL" \
        --from-literal=COOLIFY_VERBOSE="false" \
        --from-literal=COOLIFY_CACHE_ENABLED="true" \
        --namespace="$NAMESPACE" \
        --dry-run=client -o yaml | kubectl apply -f -

    log_success "ConfigMap created"
}

# Create Secret for API key
create_secret() {
    log_info "Creating Secret for API key..."

    if [ -z "${COOLIFY_API_KEY:-}" ]; then
        log_error "COOLIFY_API_KEY environment variable not set"
        exit 1
    fi

    kubectl create secret generic coolify-secret \
        --from-literal=api-key="$COOLIFY_API_KEY" \
        --namespace="$NAMESPACE" \
        --dry-run=client -o yaml | kubectl apply -f -

    log_success "Secret created"
}

# Deploy Coolify CLI as a Kubernetes Job
deploy_as_job() {
    local app_id=$1
    local job_name="coolify-deploy-$app_id-$(date +%s)"

    log_info "Creating Kubernetes Job: $job_name"

    kubectl apply -f - <<EOF
apiVersion: batch/v1
kind: Job
metadata:
  name: $job_name
  namespace: $NAMESPACE
spec:
  ttlSecondsAfterFinished: 86400
  template:
    spec:
      containers:
      - name: coolify-cli
        image: sarmkadan/coolify-cli:latest
        imagePullPolicy: IfNotPresent
        env:
        - name: COOLIFY_API_URL
          valueFrom:
            configMapKeyRef:
              name: coolify-config
              key: COOLIFY_API_URL
        - name: COOLIFY_API_KEY
          valueFrom:
            secretKeyRef:
              name: coolify-secret
              key: api-key
        - name: COOLIFY_VERBOSE
          valueFrom:
            configMapKeyRef:
              name: coolify-config
              key: COOLIFY_VERBOSE
        command:
        - coolify-cli
        - app
        - deploy
        - "$app_id"
        resources:
          requests:
            memory: "128Mi"
            cpu: "100m"
          limits:
            memory: "256Mi"
            cpu: "500m"
      restartPolicy: Never
  backoffLimit: 3
EOF

    log_success "Job created: $job_name"
    echo "$job_name"
}

# Deploy Coolify CLI as CronJob for scheduled deployments
deploy_as_cronjob() {
    local app_id=$1
    local schedule=$2  # Cron format (e.g., "0 2 * * *" for 2 AM daily)
    local cronjob_name="coolify-deploy-$app_id"

    log_info "Creating Kubernetes CronJob: $cronjob_name"

    kubectl apply -f - <<EOF
apiVersion: batch/v1
kind: CronJob
metadata:
  name: $cronjob_name
  namespace: $NAMESPACE
spec:
  schedule: "$schedule"
  jobTemplate:
    spec:
      template:
        spec:
          containers:
          - name: coolify-cli
            image: sarmkadan/coolify-cli:latest
            imagePullPolicy: IfNotPresent
            env:
            - name: COOLIFY_API_URL
              valueFrom:
                configMapKeyRef:
                  name: coolify-config
                  key: COOLIFY_API_URL
            - name: COOLIFY_API_KEY
              valueFrom:
                secretKeyRef:
                  name: coolify-secret
                  key: api-key
            command:
            - coolify-cli
            - app
            - deploy
            - "$app_id"
            resources:
              requests:
                memory: "128Mi"
                cpu: "100m"
              limits:
                memory: "256Mi"
                cpu: "500m"
          restartPolicy: OnFailure
      backoffLimit: 3
EOF

    log_success "CronJob created: $cronjob_name"
}

# Monitor job status
monitor_job() {
    local job_name=$1

    log_info "Monitoring job: $job_name"

    local max_attempts=60
    local attempt=0

    while [ $attempt -lt $max_attempts ]; do
        local status=$(kubectl get job "$job_name" -n "$NAMESPACE" -o jsonpath='{.status.conditions[0].type}' 2>/dev/null || echo "")

        if [ "$status" = "Complete" ]; then
            log_success "Job completed successfully"

            # Get job logs
            local pod=$(kubectl get pods -n "$NAMESPACE" -l batch.kubernetes.io/job-name="$job_name" -o jsonpath='{.items[0].metadata.name}')
            log_info "Job logs:"
            kubectl logs "$pod" -n "$NAMESPACE"
            return 0
        elif [ "$status" = "Failed" ]; then
            log_error "Job failed"
            local pod=$(kubectl get pods -n "$NAMESPACE" -l batch.kubernetes.io/job-name="$job_name" -o jsonpath='{.items[0].metadata.name}')
            log_error "Job logs:"
            kubectl logs "$pod" -n "$NAMESPACE"
            return 1
        fi

        sleep 10
        attempt=$((attempt + 1))
    done

    log_error "Job monitoring timeout"
    return 1
}

# Get pod logs
get_pod_logs() {
    local pod_name=$1

    log_info "Getting logs from pod: $pod_name"
    kubectl logs "$pod_name" -n "$NAMESPACE" --tail=100 -f
}

# Deploy as Helm chart (if Helm is available)
deploy_with_helm() {
    if ! command -v helm &> /dev/null; then
        log_error "Helm not found. Please install Helm for advanced deployments."
        return 1
    fi

    log_info "Deploying Coolify CLI with Helm..."

    # Create Helm values file
    cat > /tmp/coolify-cli-values.yaml <<EOF
replicaCount: 1

image:
  repository: sarmkadan/coolify-cli
  pullPolicy: IfNotPresent
  tag: "latest"

env:
  COOLIFY_API_URL: "$COOLIFY_API_URL"
  COOLIFY_VERBOSE: "false"

secrets:
  - name: COOLIFY_API_KEY
    value: "$COOLIFY_API_KEY"

resources:
  requests:
    memory: "128Mi"
    cpu: "100m"
  limits:
    memory: "256Mi"
    cpu: "500m"
EOF

    helm install coolify-cli ./helm-chart \
        --namespace "$NAMESPACE" \
        --values /tmp/coolify-cli-values.yaml

    log_success "Deployed with Helm"
}

# Create RBAC for Coolify CLI
create_rbac() {
    log_info "Creating RBAC resources for Coolify CLI..."

    kubectl apply -f - <<EOF
---
apiVersion: v1
kind: ServiceAccount
metadata:
  name: coolify-sa
  namespace: $NAMESPACE
---
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  name: coolify-role
  namespace: $NAMESPACE
rules:
- apiGroups: ["batch"]
  resources: ["jobs"]
  verbs: ["create", "get", "list", "watch", "delete"]
- apiGroups: ["batch"]
  resources: ["cronjobs"]
  verbs: ["create", "get", "list", "watch", "delete"]
- apiGroups: [""]
  resources: ["pods", "pods/log"]
  verbs: ["get", "list", "watch"]
- apiGroups: [""]
  resources: ["configmaps", "secrets"]
  verbs: ["get", "list"]
---
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: coolify-rolebinding
  namespace: $NAMESPACE
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: Role
  name: coolify-role
subjects:
- kind: ServiceAccount
  name: coolify-sa
  namespace: $NAMESPACE
EOF

    log_success "RBAC created"
}

# Main execution
main() {
    log_info "Coolify CLI Kubernetes Integration"

    check_kubectl
    create_namespace
    create_configmap
    create_secret
    create_rbac

    # Deploy as Job
    local job_name
    job_name=$(deploy_as_job "$APP_ID")

    # Monitor job
    if monitor_job "$job_name"; then
        log_success "Deployment completed successfully"
    else
        log_error "Deployment failed"
        exit 1
    fi
}

main "$@"
