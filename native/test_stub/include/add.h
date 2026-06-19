/**
 * add.h — Trivial test stub for the Ferrum end-to-end pipeline.
 *
 * This header exists solely to prove the toolchain (CMake → native build →
 * [LibraryImport] P/Invoke → MAUI app) works correctly on iOS and Android.
 * It is NOT domain-specific logic and must not grow beyond a minimal smoke test.
 */
#ifndef FERRUM_TEST_ADD_H
#define FERRUM_TEST_ADD_H

#ifdef __cplusplus
extern "C" {
#endif

/**
 * Returns the sum of two integers.
 * Used as the canonical "does P/Invoke work?" validation.
 */
int ferrum_add(int a, int b);

#ifdef __cplusplus
}
#endif

#endif /* FERRUM_TEST_ADD_H */
