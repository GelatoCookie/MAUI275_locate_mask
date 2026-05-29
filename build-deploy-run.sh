#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

PROJECT_FILE=""
CONFIGURATION="Debug"
FRAMEWORK=""
DEVICE_SERIAL=""
SKIP_BUILD=0

print_help() {
  cat <<'EOF'
Usage:
  ./build-deploy-run.sh [options]

Options:
  -p, --project <path>       Path to .csproj (default: first .csproj in script directory)
  -c, --configuration <cfg>  Build configuration (Debug or Release, default: Debug)
  -f, --framework <tfm>      Target framework (default: first Android TFM from project)
  -s, --serial <device_id>   ADB device serial (default: first connected device)
      --skip-build           Skip dotnet build and reuse latest APK from bin/
  -h, --help                 Show help

Examples:
  ./build-deploy-run.sh
  ./build-deploy-run.sh -c Release
  ./build-deploy-run.sh -s R3CN30ABC12
EOF
}

command_exists() {
  command -v "$1" >/dev/null 2>&1
}

first_csproj() {
  find "$SCRIPT_DIR" -maxdepth 1 -type f -name '*.csproj' | head -n 1
}

first_android_framework() {
  local csproj="$1"
  local tfm_line tfm

  tfm_line=$(grep -E '<TargetFrameworks?>.*</TargetFrameworks?>' "$csproj" | head -n 1 || true)
  if [[ -z "$tfm_line" ]]; then
    echo ""
    return
  fi

  tfm_line=$(printf '%s' "$tfm_line" | sed -E 's/.*<TargetFrameworks?>//; s#</TargetFrameworks?>.*##')

  tfm=$(printf '%s' "$tfm_line" | tr ';' '\n' | grep -E 'android' | head -n 1 || true)
  printf '%s' "$tfm"
}

app_id_from_csproj() {
  local csproj="$1"
  local appid

  appid=$(grep -E '<ApplicationId>[^<]+</ApplicationId>' "$csproj" | head -n 1 | sed -E 's#.*<ApplicationId>([^<]+)</ApplicationId>.*#\1#' || true)
  printf '%s' "$appid"
}

pick_device_serial() {
  local requested="$1"
  if [[ -n "$requested" ]]; then
    printf '%s' "$requested"
    return
  fi

  adb devices | awk 'NR>1 && $2=="device" { print $1 }' | head -n 1
}

latest_apk() {
  local config="$1"
  local framework="$2"
  local signed any

  signed=$(find "$SCRIPT_DIR/bin/$config/$framework" -type f -name '*-Signed.apk' 2>/dev/null | sort | tail -n 1 || true)
  if [[ -n "$signed" ]]; then
    printf '%s' "$signed"
    return
  fi

  any=$(find "$SCRIPT_DIR/bin/$config/$framework" -type f -name '*.apk' 2>/dev/null | sort | tail -n 1 || true)
  printf '%s' "$any"
}

cleanup_resizetizer_output() {
  local config="$1"
  local framework="$2"
  rm -rf "$SCRIPT_DIR/obj/$config/$framework/resizetizer" 2>/dev/null || true
}

run_build() {
  local project_file="$1"
  local framework="$2"
  local config="$3"
  local build_log

  build_log="$SCRIPT_DIR/obj/build-deploy-run.last-build.log"
  mkdir -p "$SCRIPT_DIR/obj"

  # Avoid stale file locks from long-lived build workers.
  dotnet build-server shutdown >/dev/null 2>&1 || true
  cleanup_resizetizer_output "$config" "$framework"

  if dotnet build "$project_file" -f "$framework" -c "$config" 2>&1 | tee "$build_log"; then
    return 0
  fi

  if grep -q 'MAUIR0004' "$build_log" && grep -q 'being used by another process' "$build_log"; then
    echo "Detected transient MAUI resource lock. Retrying build once..."
    dotnet build-server shutdown >/dev/null 2>&1 || true
    cleanup_resizetizer_output "$config" "$framework"
    dotnet build "$project_file" -f "$framework" -c "$config"
    return $?
  fi

  return 1
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    -p|--project)
      PROJECT_FILE="$2"
      shift 2
      ;;
    -c|--configuration)
      CONFIGURATION="$2"
      shift 2
      ;;
    -f|--framework)
      FRAMEWORK="$2"
      shift 2
      ;;
    -s|--serial)
      DEVICE_SERIAL="$2"
      shift 2
      ;;
    --skip-build)
      SKIP_BUILD=1
      shift
      ;;
    -h|--help)
      print_help
      exit 0
      ;;
    *)
      echo "Unknown option: $1"
      print_help
      exit 1
      ;;
  esac
done

if ! command_exists dotnet; then
  echo "Error: dotnet not found in PATH"
  exit 1
fi

if ! command_exists adb; then
  echo "Error: adb not found in PATH"
  exit 1
fi

if [[ -z "$PROJECT_FILE" ]]; then
  PROJECT_FILE=$(first_csproj)
fi

if [[ -z "$PROJECT_FILE" || ! -f "$PROJECT_FILE" ]]; then
  echo "Error: could not find .csproj file"
  exit 1
fi

if [[ -z "$FRAMEWORK" ]]; then
  FRAMEWORK=$(first_android_framework "$PROJECT_FILE")
fi

if [[ -z "$FRAMEWORK" ]]; then
  echo "Error: could not detect Android target framework in $PROJECT_FILE"
  exit 1
fi

APP_ID=$(app_id_from_csproj "$PROJECT_FILE")
if [[ -z "$APP_ID" ]]; then
  echo "Error: could not detect <ApplicationId> in $PROJECT_FILE"
  exit 1
fi

adb start-server >/dev/null
TARGET_SERIAL=$(pick_device_serial "$DEVICE_SERIAL")

if [[ -z "$TARGET_SERIAL" ]]; then
  echo "Error: no connected Android device found"
  echo "Tip: connect a device or start an emulator, then run: adb devices"
  exit 1
fi

ADB_ARGS=(-s "$TARGET_SERIAL")

echo "Project:       $PROJECT_FILE"
echo "Configuration: $CONFIGURATION"
echo "Framework:     $FRAMEWORK"
echo "App ID:        $APP_ID"
echo "Device:        $TARGET_SERIAL"

echo "\n[1/3] Building app..."
if [[ "$SKIP_BUILD" -eq 0 ]]; then
  run_build "$PROJECT_FILE" "$FRAMEWORK" "$CONFIGURATION"
else
  echo "Skipping build (--skip-build)"
fi

APK_PATH=$(latest_apk "$CONFIGURATION" "$FRAMEWORK")
if [[ -z "$APK_PATH" || ! -f "$APK_PATH" ]]; then
  echo "Error: could not find APK under bin/$CONFIGURATION/$FRAMEWORK"
  exit 1
fi

echo "\n[2/3] Installing APK..."
echo "APK: $APK_PATH"
adb "${ADB_ARGS[@]}" install -r "$APK_PATH"

echo "\n[3/3] Launching app..."
adb "${ADB_ARGS[@]}" shell monkey -p "$APP_ID" -c android.intent.category.LAUNCHER 1 >/dev/null

echo "\nDone. App launched on device $TARGET_SERIAL."
