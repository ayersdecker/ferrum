#!/usr/bin/env bash
# build_android_ninja.sh — Modified build script that forces Ninja generator
# to avoid Visual Studio/MSBuild NDK conflicts
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NATIVE_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
REPO_ROOT="$(cd "${NATIVE_ROOT}/.." && pwd)"

TARGET="ferrum_test_stub"
OUTPUT_DIR="${REPO_ROOT}/artifacts/android"
API_LEVEL=24
ABIS=("arm64-v8a" "armeabi-v7a" "x86_64")

# Locate NDK
NDK_HOME="${ANDROID_NDK_HOME:-${NDK_HOME:-}}"
if [[ -z "${NDK_HOME}" ]]; then
		# Try the default SDK location
		DEFAULT_NDK="${HOME}/AppData/Local/Android/Sdk/ndk"
		if [[ -d "${DEFAULT_NDK}" ]]; then
				NDK_HOME="$(ls -td "${DEFAULT_NDK}/"* 2>/dev/null | head -1)"
		fi
fi
if [[ -z "${NDK_HOME}" || ! -d "${NDK_HOME}" ]]; then
		echo "ERROR: Android NDK not found. Set ANDROID_NDK_HOME to the NDK root."
		exit 1
fi

echo "Using NDK: ${NDK_HOME}"

TOOLCHAIN="${NDK_HOME}/build/cmake/android.toolchain.cmake"
if [[ ! -f "${TOOLCHAIN}" ]]; then
		echo "ERROR: NDK toolchain not found at '${TOOLCHAIN}'."
		exit 1
fi

BUILD_ROOT="${REPO_ROOT}/.build/android"

# Check for Ninja
if ! command -v ninja &> /dev/null; then
		echo "WARNING: Ninja not found. Will try to use default generator."
		GENERATOR_FLAG=""
else
		echo "Using Ninja generator"
		GENERATOR_FLAG="-G Ninja"
fi

for ABI in "${ABIS[@]}"; do
		echo "==> Building ${ABI} ..."
		cmake -S "${NATIVE_ROOT}" \
					-B "${BUILD_ROOT}/${ABI}" \
					${GENERATOR_FLAG} \
					-DCMAKE_TOOLCHAIN_FILE="${TOOLCHAIN}" \
					-DANDROID_ABI="${ABI}" \
					-DANDROID_PLATFORM="android-${API_LEVEL}" \
					-DCMAKE_BUILD_TYPE=Release \
					-DCMAKE_MAKE_PROGRAM="ninja" \
					-DCMAKE_LIBRARY_OUTPUT_DIRECTORY="${OUTPUT_DIR}/jniLibs/${ABI}"
		cmake --build "${BUILD_ROOT}/${ABI}" --config Release --target "${TARGET}"
done

echo "==> Done. Libraries written to: ${OUTPUT_DIR}/jniLibs/"
