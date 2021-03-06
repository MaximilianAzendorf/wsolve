# Building wassign

## Linux

To build wassign under Linux, you need the following prerequisites:

* Tools
  * CMake (> 3.15)
  * A C++ compiler that supports C++17 (tested with GCC, where GCC < 9.1 is not supported)
* Dependencies (that are available as packages in most linux distributions)
  * Intel TBB
  * libunwind

After making sure all prerequisites are met you can build wassign:

1. `git clone --recursive https://github.com/MaximilianAzendorf/wassign`
2. `cd wassign`
3. `mkdir build && cd build`
4. `cmake ..`
5. `make`

Binaries will be written into the directory `build/bin`. If you want to execute the tests you can run the `wassign-test` executable.

## Windows

Building on Windows is currently untested.

## Building the documentation

You can build this documentation using [pandoc](https://pandoc.org/) and the respective makefile. Some figures require a LaTeX distribution, inkscape and ghostscript.
