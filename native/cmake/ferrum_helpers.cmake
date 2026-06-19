# ferrum_helpers.cmake
# Convenience macros for building Ferrum-compatible native libraries.
# Include this file from a consumer project's CMakeLists.txt:
#
#   list(APPEND CMAKE_MODULE_PATH "${FERRUM_ROOT}/native/cmake")
#   include(ferrum_helpers)
#
# ---------------------------------------------------------------------------

# ferrum_add_static_library(<target> SOURCES <file> [<file>...] [INCLUDES <dir>...])
#
# Creates a static library target with the settings required for Ferrum
# interop (position-independent code, hidden visibility, C/C++17 standards).
# On iOS this also sets the correct SDK and architecture slices.
macro(ferrum_add_static_library TARGET_NAME)
    cmake_parse_arguments(FERRUM_LIB "" "" "SOURCES;INCLUDES" ${ARGN})

    if(NOT FERRUM_LIB_SOURCES)
        message(FATAL_ERROR "ferrum_add_static_library: SOURCES must not be empty")
    endif()

    add_library(${TARGET_NAME} STATIC ${FERRUM_LIB_SOURCES})

    target_compile_options(${TARGET_NAME} PRIVATE
        -fvisibility=hidden
        -fno-exceptions     # Keep the ABI surface minimal
        $<$<COMPILE_LANGUAGE:CXX>:-fno-rtti>
    )

    set_target_properties(${TARGET_NAME} PROPERTIES
        POSITION_INDEPENDENT_CODE ON
        C_STANDARD   11
        CXX_STANDARD 17
        CXX_STANDARD_REQUIRED ON
    )

    if(FERRUM_LIB_INCLUDES)
        target_include_directories(${TARGET_NAME} PUBLIC ${FERRUM_LIB_INCLUDES})
    endif()
endmacro()

# ferrum_xcframework_name(<target> <out_var>)
# Returns the canonical XCFramework output name for a static lib target.
function(ferrum_xcframework_name TARGET_NAME OUT_VAR)
    set(${OUT_VAR} "lib${TARGET_NAME}.xcframework" PARENT_SCOPE)
endfunction()
