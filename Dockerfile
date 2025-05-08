# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Multi-stage build for optimized image size

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /build

# Copy project files
COPY . .

# Restore and build
RUN dotnet restore
RUN dotnet build -c Release --no-restore

# Publish
RUN dotnet publish -c Release -o /publish --no-build

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/runtime:10.0

LABEL maintainer="Vladyslav Zaiets <contact@sarmkadan.com>"
LABEL org.opencontainers.image.source="https://github.com/Sarmkadan/coolify-cli"
LABEL org.opencontainers.image.documentation="https://github.com/Sarmkadan/coolify-cli/tree/main/docs"
LABEL org.opencontainers.image.authors="Vladyslav Zaiets"
LABEL org.opencontainers.image.title="Coolify CLI"
LABEL org.opencontainers.image.description=".NET CLI for managing Coolify infrastructure"

# Install curl for health checks
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy from builder
COPY --from=builder /publish .

# Create non-root user
RUN groupadd -r coolify && useradd -r -g coolify coolify
USER coolify

# Environment variables
ENV COOLIFY_VERBOSE=false
ENV COOLIFY_TIMEOUT=30
ENV COOLIFY_CACHE_ENABLED=true
ENV COOLIFY_CACHE_TTL=300

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD coolify-cli health || exit 1

# Entrypoint
ENTRYPOINT ["dotnet", "coolify-cli.dll"]
CMD ["--help"]
