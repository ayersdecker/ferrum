# ios.toolchain.cmake
# Minimal iOS cross-compilation toolchain for CMake 3.21+.
#
# Modern CMake (3.14+) has built-in iOS support via CMAKE_SYSTEM_NAME=iOS.
# This file configures those settings for convenience and documents the
# options consumed by the build scripts.
#
# Variables read by this toolchain:
#   PLATFORM          - "OS64"            → arm64 device (default)
#                       "SIMULATOR64"     → x86_64 simulator (Intel Mac)
#                       "SIMULATORARM64"  → arm64 simulator (Apple Silicon)
#   DEPLOYMENT_TARGET - minimum iOS version, default "15.0"
#
# Usage:
#   cmake -S native -B build/ios-device \
#         -DCMAKE_TOOLCHAIN_FILE=native/cmake/ios.toolchain.cmake \
#         -DPLATFORM=OS64 \
#         -DDEPLOYMENT_TARGET=15.0 \
#         -GXcode
# ---------------------------------------------------------------------------

cmake_minimum_required(VERSION 3.21)

# ── Platform selection ───────────────────────────────────────────────────────
if(NOT DEFINED PLATFORM)
    set(PLATFORM "OS64")
endif()

if(NOT DEFINED DEPLOYMENT_TARGET)
    set(DEPLOYMENT_TARGET "15.0")
endif()

# ── System name & processor ──────────────────────────────────────────────────
set(CMAKE_SYSTEM_NAME "iOS")

if(PLATFORM STREQUAL "OS64")
    set(CMAKE_OSX_ARCHITECTURES "arm64" CACHE STRING "")
    set(CMAKE_OSX_SYSROOT       "iphoneos" CACHE STRING "")
elseif(PLATFORM STREQUAL "SIMULATOR64")
    set(CMAKE_OSX_ARCHITECTURES "x86_64" CACHE STRING "")
    set(CMAKE_OSX_SYSROOT       "iphonesimulator" CACHE STRING "")
elseif(PLATFORM STREQUAL "SIMULATORARM64")
    set(CMAKE_OSX_ARCHITECTURES "arm64" CACHE STRING "")
    set(CMAKE_OSX_SYSROOT       "iphonesimulator" CACHE STRING "")
else()
    message(FATAL_ERROR "ios.toolchain.cmake: unknown PLATFORM '${PLATFORM}'. "
            "Valid values: OS64, SIMULATOR64, SIMULATORARM64")
endif()

set(CMAKE_OSX_DEPLOYMENT_TARGET "${DEPLOYMENT_TARGET}" CACHE STRING "")

# ── Prevent CMake from trying to link test executables during configure ───────
set(CMAKE_TRY_COMPILE_TARGET_TYPE STATIC_LIBRARY)

# ── Force static library output (needed for XCFramework assembly) ─────────────
set(BUILD_SHARED_LIBS OFF CACHE BOOL "" FORCE)
