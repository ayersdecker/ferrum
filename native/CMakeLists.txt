cmake_minimum_required(VERSION 3.21)
project(ferrum_native LANGUAGES C CXX)

set(CMAKE_C_STANDARD 11)
set(CMAKE_C_STANDARD_REQUIRED ON)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# ---------------------------------------------------------------------------
# Public helper macros used by consumer projects and by CI.
# ---------------------------------------------------------------------------
include(cmake/ferrum_helpers.cmake)

# ---------------------------------------------------------------------------
# Optional: build the in-tree test stub so CI can validate the pipeline
# without any external consumer source.  A real consumer would instead call
#   ferrum_add_static_library(my_lib SOURCES ...) from their own CMakeLists.
# ---------------------------------------------------------------------------
option(FERRUM_BUILD_TEST_STUB    "Build the in-tree add() test stub"    ON)
option(FERRUM_BUILD_DSP_FIXTURE  "Build the in-tree DSP test fixture"   ON)

if(FERRUM_BUILD_TEST_STUB)
    add_subdirectory(test_stub)
endif()

if(FERRUM_BUILD_DSP_FIXTURE)
    add_subdirectory(dsp_fixture)
endif()
