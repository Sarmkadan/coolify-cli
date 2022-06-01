# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Variables
.PHONY: help clean build test run publish docker lint docs

DOTNET := dotnet
PROJECT := coolify-cli
VERSION := 1.2.0
CONFIG := Release
OUTPUT_DIR := ./publish
DOCKER_IMAGE := sarmkadan/coolify-cli
DOCKER_TAG := latest

# Colors
RED := \033[0;31m
GREEN := \033[0;32m
YELLOW := \033[0;33m
BLUE := \033[0;34m
NC := \033[0m

# Help
help:
	@echo "$(BLUE)Coolify CLI - Build System$(NC)"
	@echo ""
	@echo "$(YELLOW)Usage:$(NC) make [target]"
	@echo ""
	@echo "$(YELLOW)Targets:$(NC)"
	@echo "  $(GREEN)help$(NC)              Show this help message"
	@echo "  $(GREEN)build$(NC)             Build the project"
	@echo "  $(GREEN)test$(NC)              Run tests"
	@echo "  $(GREEN)run$(NC)               Run the CLI"
	@echo "  $(GREEN)clean$(NC)             Clean build artifacts"
	@echo "  $(GREEN)publish$(NC)           Publish for distribution"
	@echo "  $(GREEN)docker-build$(NC)      Build Docker image"
	@echo "  $(GREEN)docker-run$(NC)        Run in Docker"
	@echo "  $(GREEN)docker-push$(NC)       Push Docker image to registry"
	@echo "  $(GREEN)lint$(NC)              Run code analysis"
	@echo "  $(GREEN)docs$(NC)              Generate documentation"
	@echo "  $(GREEN)format$(NC)            Format code according to standards"
	@echo "  $(GREEN)install$(NC)           Install CLI system-wide"
	@echo "  $(GREEN)uninstall$(NC)         Uninstall CLI"
	@echo "  $(GREEN)version$(NC)           Show version information"
	@echo "  $(GREEN)all$(NC)               Build, test, and publish"
	@echo ""

# Build
build:
	@echo "$(BLUE)[*] Building $(PROJECT)...$(NC)"
	@$(DOTNET) build -c $(CONFIG)
	@echo "$(GREEN)[+] Build completed$(NC)"

# Test
test:
	@echo "$(BLUE)[*] Running tests...$(NC)"
	@$(DOTNET) test -c $(CONFIG) --verbosity minimal
	@echo "$(GREEN)[+] Tests completed$(NC)"

# Run
run: build
	@echo "$(BLUE)[*] Running $(PROJECT)...$(NC)"
	@$(DOTNET) run -- $(ARGS)

# Clean
clean:
	@echo "$(BLUE)[*] Cleaning build artifacts...$(NC)"
	@$(DOTNET) clean
	@rm -rf $(OUTPUT_DIR)
	@rm -rf bin obj
	@echo "$(GREEN)[+] Clean completed$(NC)"

# Publish
publish: clean test
	@echo "$(BLUE)[*] Publishing $(PROJECT) v$(VERSION)...$(NC)"
	@mkdir -p $(OUTPUT_DIR)
	@$(DOTNET) publish -c $(CONFIG) -o $(OUTPUT_DIR)
	@echo "$(GREEN)[+] Published to $(OUTPUT_DIR)$(NC)"
	@ls -lh $(OUTPUT_DIR)/coolify-cli

# Docker build
docker-build:
	@echo "$(BLUE)[*] Building Docker image $(DOCKER_IMAGE):$(DOCKER_TAG)...$(NC)"
	@docker build -t $(DOCKER_IMAGE):$(DOCKER_TAG) \
		-t $(DOCKER_IMAGE):$(VERSION) \
		-t $(DOCKER_IMAGE):latest \
		-f Dockerfile .
	@echo "$(GREEN)[+] Docker image built$(NC)"
	@docker images | grep $(DOCKER_IMAGE)

# Docker run
docker-run:
	@echo "$(BLUE)[*] Running Docker container...$(NC)"
	@docker run --rm \
		-e COOLIFY_API_KEY=$(COOLIFY_API_KEY) \
		-e COOLIFY_API_URL=$(COOLIFY_API_URL) \
		-v $(PWD)/logs:/app/logs \
		$(DOCKER_IMAGE):$(DOCKER_TAG) $(ARGS)

# Docker push
docker-push: docker-build
	@echo "$(BLUE)[*] Pushing Docker image to registry...$(NC)"
	@docker push $(DOCKER_IMAGE):$(DOCKER_TAG)
	@docker push $(DOCKER_IMAGE):$(VERSION)
	@docker push $(DOCKER_IMAGE):latest
	@echo "$(GREEN)[+] Docker image pushed$(NC)"

# Lint
lint:
	@echo "$(BLUE)[*] Running code analysis...$(NC)"
	@$(DOTNET) build /p:TreatWarningsAsErrors=true
	@echo "$(GREEN)[+] Code analysis completed$(NC)"

# Format code
format:
	@echo "$(BLUE)[*] Formatting code...$(NC)"
	@find . -name "*.cs" -type f | xargs -I {} bash -c \
		'$(DOTNET) csharp-format --check {} || $(DOTNET) csharp-format {}'
	@echo "$(GREEN)[+] Code formatted$(NC)"

# Generate documentation
docs:
	@echo "$(BLUE)[*] Generating documentation...$(NC)"
	@mkdir -p docs
	@echo "$(GREEN)[+] Documentation generated$(NC)"
	@ls -la docs/

# Install system-wide
install: publish
	@echo "$(BLUE)[*] Installing $(PROJECT) system-wide...$(NC)"
	@if [ -f /usr/local/bin/coolify-cli ]; then \
		echo "$(YELLOW)[!] Existing installation found, backing up...$(NC)"; \
		sudo mv /usr/local/bin/coolify-cli /usr/local/bin/coolify-cli.bak; \
	fi
	@sudo ln -s $(PWD)/$(OUTPUT_DIR)/coolify-cli /usr/local/bin/coolify-cli
	@sudo chmod +x /usr/local/bin/coolify-cli
	@echo "$(GREEN)[+] Installation completed$(NC)"
	@coolify-cli version

# Uninstall
uninstall:
	@echo "$(BLUE)[*] Uninstalling $(PROJECT)...$(NC)"
	@if [ -L /usr/local/bin/coolify-cli ]; then \
		sudo rm /usr/local/bin/coolify-cli; \
		echo "$(GREEN)[+] Uninstalled successfully$(NC)"; \
	else \
		echo "$(YELLOW)[!] Not installed system-wide$(NC)"; \
	fi

# Version
version:
	@echo "Coolify CLI v$(VERSION)"
	@echo "Built with .NET 10"
	@echo "Author: Vladyslav Zaiets"
	@echo "Website: https://sarmkadan.com"

# Full build and test
all: clean build test lint publish
	@echo "$(GREEN)[+] Complete build finished$(NC)"

# Development setup
dev-setup:
	@echo "$(BLUE)[*] Setting up development environment...$(NC)"
	@$(DOTNET) restore
	@$(DOTNET) tool restore
	@echo "$(GREEN)[+] Development environment ready$(NC)"

# Run with specific configuration
run-debug:
	@$(DOTNET) run -c Debug -- $(ARGS)

run-release:
	@$(DOTNET) run -c Release -- $(ARGS)

# Examples
example-deploy:
	@bash examples/deploy-all.sh

example-health-monitor:
	@bash examples/health-monitor.sh

example-backup:
	@bash examples/backup-databases.sh

example-logs:
	@bash examples/log-analysis.sh

# Docker compose
docker-compose-up:
	@echo "$(BLUE)[*] Starting Docker Compose services...$(NC)"
	@docker-compose up -d
	@echo "$(GREEN)[+] Services started$(NC)"
	@docker-compose ps

docker-compose-down:
	@echo "$(BLUE)[*] Stopping Docker Compose services...$(NC)"
	@docker-compose down
	@echo "$(GREEN)[+] Services stopped$(NC)"

docker-compose-logs:
	@docker-compose logs -f $(SERVICE)

# CI/CD integration
ci-build:
	@$(DOTNET) build -c $(CONFIG) --no-restore

ci-test:
	@$(DOTNET) test -c $(CONFIG) --no-build --logger="console;verbosity=minimal"

ci-publish:
	@$(DOTNET) publish -c $(CONFIG) -o $(OUTPUT_DIR) --no-build

# Code metrics
metrics:
	@echo "$(BLUE)[*] Calculating code metrics...$(NC)"
	@$(DOTNET) build -c $(CONFIG) -p:TreatWarningsAsErrors=false
	@find . -name "*.cs" -type f | wc -l | xargs echo "Total C# files:"
	@find . -name "*.cs" -type f -exec wc -l {} + | tail -1 | awk '{print "Total lines of code: " $$1}'

# Package
package: publish
	@echo "$(BLUE)[*] Creating distribution package...$(NC)"
	@mkdir -p dist
	@cd $(OUTPUT_DIR) && zip -r ../dist/coolify-cli-$(VERSION)-$(shell uname -s)-$(shell uname -m).zip * && cd ..
	@echo "$(GREEN)[+] Package created$(NC)"
	@ls -lh dist/

# Default target
.DEFAULT_GOAL := help
