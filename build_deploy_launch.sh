#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TARGET_SCRIPT="$SCRIPT_DIR/build-deploy-run.sh"

if [[ ! -f "$TARGET_SCRIPT" ]]; then
  echo "Error: expected script not found: $TARGET_SCRIPT"
  exit 1
fi

exec bash "$TARGET_SCRIPT" "$@"
