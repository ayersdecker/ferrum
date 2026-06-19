cmake_minimum_required(VERSION 3.21)
project(ferrum_test_stub LANGUAGES C)

include(../cmake/ferrum_helpers.cmake)

ferrum_add_static_library(ferrum_test_stub
    SOURCES src/add.c
    INCLUDES include
)

# Export a CMake target so consuming projects can find_package() this stub.
include(GNUInstallDirs)
install(TARGETS ferrum_test_stub
        ARCHIVE DESTINATION "${CMAKE_INSTALL_LIBDIR}"
)
install(DIRECTORY include/ DESTINATION "${CMAKE_INSTALL_INCLUDEDIR}")
