#!/bin/bash

# =============================================================================
# Coolify CLI Development Setup Script
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Functions
print_header() {
    echo ""
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}========================================${NC}"
    echo ""
}

print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[✓]${NC} $1"
}

print_error() {
    echo -e "${RED}[✗]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[!]${NC} $1"
}

# Check prerequisites
check_prerequisites() {
    print_header "Checking Prerequisites"

    local missing=0

    # Check .NET SDK
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK not found"
        missing=$((missing + 1))
    else
        local dotnet_version=$(dotnet --version)
        print_success ".NET SDK: $dotnet_version"

        # Check for .NET 10
        if ! echo "$dotnet_version" | grep -q "^10\|^[1-9][0-9]"; then
            print_warning ".NET 10 or later required. Current: $dotnet_version"
        fi
    fi

    # Check Git
    if ! command -v git &> /dev/null; then
        print_error "Git not found"
        missing=$((missing + 1))
    else
        print_success "Git: $(git --version)"
    fi

    # Check optional tools
    if command -v code &> /dev/null; then
        print_success "VS Code found"
    else
        print_info "VS Code not found (optional)"
    fi

    if command -v docker &> /dev/null; then
        print_success "Docker: $(docker --version)"
    else
        print_info "Docker not found (optional)"
    fi

    if [ $missing -gt 0 ]; then
        print_error "Missing $missing required tool(s)"
        return 1
    fi

    return 0
}

# Clone/update repository
setup_repository() {
    print_header "Setting Up Repository"

    cd "$PROJECT_ROOT"

    if [ -d ".git" ]; then
        print_info "Repository already initialized"
        print_info "Updating main branch..."
        git fetch origin main || true
    else
        print_error "Git repository not initialized"
        return 1
    fi

    print_success "Repository setup complete"
}

# Install .NET dependencies
install_dependencies() {
    print_header "Installing Dependencies"

    cd "$PROJECT_ROOT"

    print_info "Restoring NuGet packages..."
    if ! dotnet restore; then
        print_error "Failed to restore dependencies"
        return 1
    fi

    print_success "Dependencies installed"
}

# Build project
build_project() {
    print_header "Building Project"

    cd "$PROJECT_ROOT"

    print_info "Building in Debug configuration..."
    if ! dotnet build --configuration Debug; then
        print_error "Build failed"
        return 1
    fi

    print_success "Build successful"
}

# Setup test environment
setup_test_environment() {
    print_header "Setting Up Test Environment"

    cd "$PROJECT_ROOT"

    # Create test directories
    mkdir -p "test-results"
    mkdir -p "test-coverage"

    print_success "Test environment setup complete"
}

# Setup development environment files
setup_dev_environment() {
    print_header "Setting Up Development Environment"

    cd "$PROJECT_ROOT"

    # Copy environment template if not exists
    if [ ! -f ".env" ]; then
        if [ -f "configs/dev.env.example" ]; then
            cp "configs/dev.env.example" ".env"
            print_success "Created .env from template"
            print_warning "Edit .env with your development settings"
        fi
    else
        print_info ".env already exists"
    fi

    # Create necessary directories
    mkdir -p "logs"
    mkdir -p "temp"
    mkdir -p "backups"

    print_success "Development environment setup complete"
}

# Setup Git hooks
setup_git_hooks() {
    print_header "Setting Up Git Hooks"

    cd "$PROJECT_ROOT"

    # Pre-commit hook
    mkdir -p ".git/hooks"

    cat > ".git/hooks/pre-commit" <<'EOF'
#!/bin/bash
# Prevent commits with debug code

if git diff --cached | grep -qE "TODO|FIXME|DEBUG"; then
    echo "WARNING: Found TODO/FIXME/DEBUG markers in staged changes"
fi

exit 0
EOF

    chmod +x ".git/hooks/pre-commit"
    print_success "Git hooks installed"
}

# Setup IDE configuration
setup_ide_config() {
    print_header "Setting Up IDE Configuration"

    cd "$PROJECT_ROOT"

    # VS Code settings
    if [ ! -d ".vscode" ]; then
        mkdir -p ".vscode"

        cat > ".vscode/settings.json" <<EOF
{
    "editor.formatOnSave": true,
    "editor.defaultFormatter": "ms-dotnettools.csharp",
    "[csharp]": {
        "editor.formatOnSave": true,
        "editor.defaultFormatter": "ms-dotnettools.csharp"
    },
    "dotnet.defaultSolutionOrFolder": "$PROJECT_ROOT",
    "omnisharp.path": "latest",
    "omnisharp.enableRoslynAnalyzers": true
}
EOF

        print_success "VS Code settings created"
    else
        print_info "VS Code settings already exist"
    fi
}

# Install IDE extensions (interactive)
suggest_ide_extensions() {
    print_header "Suggested IDE Extensions"

    echo "For VS Code, install these extensions:"
    echo "  - C# (ms-dotnettools.csharp)"
    echo "  - .NET Install Tool (ms-dotnettools.vscode-dotnet-runtime)"
    echo "  - Code Coverage Highlighter (ryanluker.csharp-coverage-reporter)"
    echo "  - GitLens (eamodio.gitlens)"
    echo "  - Better Comments (aaron-bond.better-comments)"
    echo ""

    read -p "Install extensions automatically? (y/n) " -r
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        code --install-extension ms-dotnettools.csharp
        code --install-extension ms-dotnettools.vscode-dotnet-runtime
        code --install-extension ryanluker.csharp-coverage-reporter
        code --install-extension eamodio.gitlens
        code --install-extension aaron-bond.better-comments
        print_success "Extensions installed"
    fi
}

# Run basic tests
run_tests() {
    print_header "Running Tests"

    cd "$PROJECT_ROOT"

    print_info "Running unit tests..."
    if ! dotnet test --configuration Debug --no-build --verbosity minimal; then
        print_error "Tests failed"
        return 1
    fi

    print_success "All tests passed"
}

# Display next steps
show_next_steps() {
    print_header "Setup Complete!"

    echo "Next steps:"
    echo ""
    echo "1. Edit .env file with your settings:"
    echo "   nano .env"
    echo ""
    echo "2. Build the project:"
    echo "   dotnet build"
    echo ""
    echo "3. Run the CLI:"
    echo "   dotnet run -- app list"
    echo ""
    echo "4. Run tests:"
    echo "   dotnet test"
    echo ""
    echo "5. Open in VS Code:"
    echo "   code ."
    echo ""
    echo "Documentation:"
    echo "  - Getting Started: docs/GETTING_STARTED.md"
    echo "  - Architecture: docs/ARCHITECTURE.md"
    echo "  - Contributing: CONTRIBUTING.md"
    echo ""
}

# Main setup flow
main() {
    print_header "Coolify CLI Development Setup"

    # Check prerequisites
    if ! check_prerequisites; then
        print_error "Prerequisites check failed"
        echo ""
        echo "Please install required tools:"
        echo "  - .NET 10 SDK: https://dotnet.microsoft.com/download/dotnet/10.0"
        echo "  - Git: https://git-scm.com/download"
        exit 1
    fi

    # Run setup steps
    setup_repository || exit 1
    install_dependencies || exit 1
    build_project || exit 1
    setup_test_environment || exit 1
    setup_dev_environment || exit 1
    setup_git_hooks || exit 1
    setup_ide_config || exit 1

    # Optional steps
    if command -v code &> /dev/null; then
        suggest_ide_extensions
    fi

    # Run tests
    if read -p "Run tests? (y/n) " -r; [[ $REPLY =~ ^[Yy]$ ]]; then
        run_tests || true
    fi

    show_next_steps
}

# Run main setup
main "$@"
