/**
 * ferrum_dsp.h — Second test fixture for the Ferrum codegen/build pipeline.
 *
 * This header exercises the two real-consumer patterns that ferrum_add(int,int)
 * cannot cover:
 *   1. A function that takes a float* buffer with an int length.
 *   2. A function that writes a result through a struct out-parameter.
 *
 * Both patterns appear constantly in audio, signal, and ML processing ABIs and
 * must round-trip through ferrum-codegen without error.
 *
 * This file is NOT domain-specific application logic; it is framework test
 * infrastructure that validates the plumbing.
 */
#ifndef FERRUM_DSP_H
#define FERRUM_DSP_H

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

/**
 * Fixed-layout, blittable statistics summary.
 * All fields are 32-bit; no padding needed on any supported ABI.
 */
typedef struct {
    float   min_val; /**< Minimum value in the buffer.  */
    float   max_val; /**< Maximum value in the buffer.  */
    float   mean;    /**< Arithmetic mean of the buffer.*/
    int32_t count;   /**< Number of elements processed. */
} FerrumDspStats;

/**
 * Multiplies every element in buf[0..len-1] by factor in-place.
 * Validates the (float* buf, int32_t len) parameter pattern.
 */
void ferrum_dsp_scale(float* buf, int32_t len, float factor);

/**
 * Computes min, max, mean, and count of buf[0..len-1] and writes
 * the result through the out-parameter.
 * Validates the const-float* input + blittable-struct-out-parameter pattern.
 */
void ferrum_dsp_stats(const float* buf, int32_t len, FerrumDspStats* result);

#ifdef __cplusplus
}
#endif

#endif /* FERRUM_DSP_H */
