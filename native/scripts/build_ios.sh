#!/usr/bin/env bash
# build_ios.sh — Builds the native static library for iOS device and both
# simulator slices, then packages them into a single XCFramework ready to
# be added as a NativeReference in the MAUI project.
#
# Prerequisites:
#   • macOS with Xcode and command-line tools installed
#   • CMake 3.21+
#
# Usage:
#   ./native/scripts/build_ios.sh [--target <cmake-target>] [--output <dir>]
#
# Outputs:
#   <output>/lib<target>.xcframework
#
# ---------------------------------------------------------------------------
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NATIVE_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
REPO_ROOT="$(cd "${NATIVE_ROOT}/.." && pwd)"

TARGET="ferrum_test_stub"
OUTPUT_DIR="${REPO_ROOT}/artifacts/ios"
DEPLOYMENT_TARGET="15.0"

while [[ $# -gt 0 ]]; do
    case $1 in
        --target)   TARGET="$2";      shift 2 ;;
        --output)   OUTPUT_DIR="$2";  shift 2 ;;
        *)          echo "Unknown argument: $1"; exit 1 ;;
    esac
done

BUILD_ROOT="${REPO_ROOT}/.build/ios"
TOOLCHAIN="${NATIVE_ROOT}/cmake/ios.toolchain.cmake"

echo "==> Building iOS device (arm64) ..."
cmake -S "${NATIVE_ROOT}" \
      -B "${BUILD_ROOT}/device" \
      -DCMAKE_TOOLCHAIN_FILE="${TOOLCHAIN}" \
      -DPLATFORM=OS64 \
      -DDEPLOYMENT_TARGET="${DEPLOYMENT_TARGET}" \
      -DCMAKE_BUILD_TYPE=Release \
      -GXcode
cmake --build "${BUILD_ROOT}/device" --config Release --target "${TARGET}"

echo "==> Building iOS Simulator arm64 (Apple Silicon) ..."
cmake -S "${NATIVE_ROOT}" \
      -B "${BUILD_ROOT}/sim-arm64" \
      -DCMAKE_TOOLCHAIN_FILE="${TOOLCHAIN}" \
      -DPLATFORM=SIMULATORARM64 \
      -DDEPLOYMENT_TARGET="${DEPLOYMENT_TARGET}" \
      -DCMAKE_BUILD_TYPE=Release \
      -GXcode
cmake --build "${BUILD_ROOT}/sim-arm64" --config Release --target "${TARGET}"

echo "==> Building iOS Simulator x86_64 (Intel Mac) ..."
cmake -S "${NATIVE_ROOT}" \
      -B "${BUILD_ROOT}/sim-x86_64" \
      -DCMAKE_TOOLCHAIN_FILE="${TOOLCHAIN}" \
      -DPLATFORM=SIMULATOR64 \
      -DDEPLOYMENT_TARGET="${DEPLOYMENT_TARGET}" \
      -DCMAKE_BUILD_TYPE=Release \
      -GXcode
cmake --build "${BUILD_ROOT}/sim-x86_64" --config Release --target "${TARGET}"

echo "==> Creating fat simulator library ..."
SIM_ARM64="${BUILD_ROOT}/sim-arm64/Release-iphonesimulator/lib${TARGET}.a"
SIM_X86="${BUILD_ROOT}/sim-x86_64/Release-iphonesimulator/lib${TARGET}.a"
FAT_SIM="${BUILD_ROOT}/sim-fat/lib${TARGET}.a"
mkdir -p "${BUILD_ROOT}/sim-fat"
lipo -create "${SIM_ARM64}" "${SIM_X86}" -output "${FAT_SIM}"

echo "==> Assembling XCFramework ..."
DEVICE_LIB="${BUILD_ROOT}/device/Release-iphoneos/lib${TARGET}.a"
XCF_OUT="${OUTPUT_DIR}/lib${TARGET}.xcframework"
rm -rf "${XCF_OUT}"
mkdir -p "${OUTPUT_DIR}"
xcodebuild -create-xcframework \
    -library "${DEVICE_LIB}" \
    -library "${FAT_SIM}" \
    -output  "${XCF_OUT}"

echo "==> Done: ${XCF_OUT}"
