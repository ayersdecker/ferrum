cmake_minimum_required(VERSION 3.21)
project(ferrum_test_stub LANGUAGES C)

include(../cmake/ferrum_helpers.cmake)

# On Android, build as a shared library (.so)
# On iOS, build as a static library (.a) for XCFramework
if(ANDROID)
    add_library(ferrum_test_stub SHARED src/add.c)
    target_compile_options(ferrum_test_stub PRIVATE
        -fvisibility=hidden
        -fno-exceptions
    )
    set_target_properties(ferrum_test_stub PROPERTIES
        C_STANDARD 11
        CXX_STANDARD 17
        CXX_STANDARD_REQUIRED ON
    )
    target_include_directories(ferrum_test_stub PUBLIC include)
else()
    ferrum_add_static_library(ferrum_test_stub
        SOURCES src/add.c
        INCLUDES include
    )
endif()

# Export a CMake target so consuming projects can find_package() this stub.
include(GNUInstallDirs)
install(TARGETS ferrum_test_stub
        ARCHIVE DESTINATION "${CMAKE_INSTALL_LIBDIR}"
        LIBRARY DESTINATION "${CMAKE_INSTALL_LIBDIR}"
)
install(DIRECTORY include/ DESTINATION "${CMAKE_INSTALL_INCLUDEDIR}")
