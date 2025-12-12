#!/bin/bash
set -e

REPO_ROOT="$(cd "$(dirname "$0")" && pwd)"
OUTPUT_DIR="$REPO_ROOT/cli-output"
MODULES_DIR="$OUTPUT_DIR/modules"

# Detect runtime identifier (RID) based on platform
detect_rid() {
  local os=$(uname -s)
  local arch=$(uname -m)

  case "$os" in
    Linux*)
      case "$arch" in
        x86_64) echo "linux-x64" ;;
        aarch64|arm64) echo "linux-arm64" ;;
        *) echo "linux-x64" ;;
      esac
      ;;
    Darwin*)
      case "$arch" in
        arm64) echo "osx-arm64" ;;
        x86_64) echo "osx-x64" ;;
        *) echo "osx-x64" ;;
      esac
      ;;
    *)
      echo "linux-x64"
      ;;
  esac
}

# Get version and RID from arguments
VERSION="${1:-0.3.0-dev}"
RID="${2:-$(detect_rid)}"

echo "Building Conductor CLI tools v$VERSION for $RID..."
echo "Using single-file, self-contained executables"

# Clean output directory
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"
mkdir -p "$MODULES_DIR"

# Common publish arguments
PUBLISH_ARGS=(
  -c Release
  -r "$RID"
  --self-contained true
  -p:Version="$VERSION"
  -p:PublishSingleFile=true
  -p:EnableCompressionInSingleFile=true
  -p:DebugType=embedded
  -p:IncludeNativeLibrariesForSelfExtract=true
  -p:GenerateDocumentationFile=false
)

# Publish CLI tools
echo "Publishing conductor CLI..."
dotnet publish "$REPO_ROOT/src/FulcrumLabs.Conductor.Cli.Executor/FulcrumLabs.Conductor.Cli.Executor.csproj" \
    "${PUBLISH_ARGS[@]}" \
    -o "$OUTPUT_DIR"

echo "Publishing conductor-agent CLI..."
dotnet publish "$REPO_ROOT/src/FulcrumLabs.Conductor.Cli.Agent/FulcrumLabs.Conductor.Cli.Agent.csproj" \
    "${PUBLISH_ARGS[@]}" \
    -o "$OUTPUT_DIR"

# Publish modules
echo "Publishing debug module..."
dotnet publish "$REPO_ROOT/modules/src/FulcrumLabs.Conductor.Modules.Debug/FulcrumLabs.Conductor.Modules.Debug.csproj" \
    "${PUBLISH_ARGS[@]}" \
    -o "$MODULES_DIR"

echo "Publishing shell module..."
dotnet publish "$REPO_ROOT/modules/src/FulcrumLabs.Conductor.Modules.Shell/FulcrumLabs.Conductor.Modules.Shell.csproj" \
    "${PUBLISH_ARGS[@]}" \
    -o "$MODULES_DIR"

echo "✓ CLI tools built successfully in $OUTPUT_DIR"
echo "✓ Modules built in $MODULES_DIR"

# Make executables executable (Unix)
chmod +x "$OUTPUT_DIR/conductor" 2>/dev/null || true
chmod +x "$OUTPUT_DIR/conductor-agent" 2>/dev/null || true
chmod +x "$MODULES_DIR/conductor-module-"* 2>/dev/null || true

# Show file sizes
echo ""
echo "Executable sizes:"
ls -lh "$OUTPUT_DIR"/conductor* 2>/dev/null | awk '{print $9, $5}' || true
ls -lh "$MODULES_DIR"/conductor-module-* 2>/dev/null | awk '{print $9, $5}' || true
