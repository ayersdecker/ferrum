/*
 * ferrum_dsp.c — Implementation of the Ferrum DSP test fixture.
 * See ferrum_dsp.h for documentation.
 */
#include "ferrum_dsp.h"

void ferrum_dsp_scale(float* buf, int32_t len, float factor)
{
    for (int32_t i = 0; i < len; ++i)
        buf[i] *= factor;
}

void ferrum_dsp_stats(const float* buf, int32_t len, FerrumDspStats* result)
{
    if (len <= 0 || !buf || !result)
        return;

    float min_val = buf[0];
    float max_val = buf[0];
    float sum     = 0.0f;

    for (int32_t i = 0; i < len; ++i)
    {
        float v = buf[i];
        if (v < min_val) min_val = v;
        if (v > max_val) max_val = v;
        sum += v;
    }

    result->min_val = min_val;
    result->max_val = max_val;
    result->mean    = sum / (float)len;
    result->count   = len;
}
