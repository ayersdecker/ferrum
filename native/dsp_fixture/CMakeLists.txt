cmake_minimum_required(VERSION 3.21)
project(ferrum_dsp_fixture LANGUAGES C)

include(../cmake/ferrum_helpers.cmake)

ferrum_add_static_library(ferrum_dsp_fixture
    SOURCES src/ferrum_dsp.c
    INCLUDES include
)

include(GNUInstallDirs)
install(TARGETS ferrum_dsp_fixture
        ARCHIVE DESTINATION "${CMAKE_INSTALL_LIBDIR}"
)
install(DIRECTORY include/ DESTINATION "${CMAKE_INSTALL_INCLUDEDIR}")
