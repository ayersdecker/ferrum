#!/usr/bin/env bash
# build_android.sh — Builds the native shared library (.so) for every
# supported Android ABI using the Android NDK's bundled CMake toolchain.
#
# Prerequisites:
#   • Android NDK r25+ (ANDROID_NDK_HOME or NDK_HOME must be set, or the
#     NDK must be installed via Android Studio/sdkmanager)
#   • CMake 3.21+
#
# Usage:
#   ./native/scripts/build_android.sh [--target <cmake-target>] [--output <dir>]
#
# Outputs (per ABI):
#   <output>/jniLibs/<abi>/lib<target>.so
#
# ---------------------------------------------------------------------------
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NATIVE_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
REPO_ROOT="$(cd "${NATIVE_ROOT}/.." && pwd)"

TARGET="ferrum_test_stub"
OUTPUT_DIR="${REPO_ROOT}/artifacts/android"
API_LEVEL=24
ABIS=("arm64-v8a" "armeabi-v7a" "x86_64" "x86")

while [[ $# -gt 0 ]]; do
    case $1 in
        --target)     TARGET="$2";     shift 2 ;;
        --output)     OUTPUT_DIR="$2"; shift 2 ;;
        --api-level)  API_LEVEL="$2";  shift 2 ;;
        *)            echo "Unknown argument: $1"; exit 1 ;;
    esac
done

# Locate NDK
NDK_HOME="${ANDROID_NDK_HOME:-${NDK_HOME:-}}"
if [[ -z "${NDK_HOME}" ]]; then
    # Try the default SDK location used by Android Studio
    DEFAULT_NDK="${HOME}/Library/Android/sdk/ndk"
    if [[ -d "${DEFAULT_NDK}" ]]; then
        NDK_HOME="$(ls -td "${DEFAULT_NDK}/"* 2>/dev/null | head -1)"
    fi
fi
if [[ -z "${NDK_HOME}" || ! -d "${NDK_HOME}" ]]; then
    echo "ERROR: Android NDK not found. Set ANDROID_NDK_HOME to the NDK root."
    exit 1
fi

TOOLCHAIN="${NDK_HOME}/build/cmake/android.toolchain.cmake"
if [[ ! -f "${TOOLCHAIN}" ]]; then
    echo "ERROR: NDK toolchain not found at '${TOOLCHAIN}'."
    exit 1
fi

BUILD_ROOT="${REPO_ROOT}/.build/android"

for ABI in "${ABIS[@]}"; do
    echo "==> Building ${ABI} ..."
    cmake -S "${NATIVE_ROOT}" \
          -B "${BUILD_ROOT}/${ABI}" \
          -DCMAKE_TOOLCHAIN_FILE="${TOOLCHAIN}" \
          -DANDROID_ABI="${ABI}" \
          -DANDROID_PLATFORM="android-${API_LEVEL}" \
          -DCMAKE_BUILD_TYPE=Release \
          -DCMAKE_LIBRARY_OUTPUT_DIRECTORY="${OUTPUT_DIR}/jniLibs/${ABI}"
    cmake --build "${BUILD_ROOT}/${ABI}" --config Release --target "${TARGET}"
done

echo "==> Done. Libraries written to: ${OUTPUT_DIR}/jniLibs/"
