using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRtc.CommonAudio.SignalProcessing;

namespace WebRtc.CommonAudio.Vad
{
    static class VadSp
    {
        // Allpass filter coefficients, upper and lower, in Q13.
        // Upper: 0.64, Lower: 0.17.
        static readonly Int16[] kAllPassCoefsQ13 = { 5243, 1392 };  // Q13.
        const Int16 kSmoothingDown = 6553;  // 0.2 in Q15.
        const Int16 kSmoothingUp = 32439;  // 0.99 in Q15.

        // TODO(bjornv): Move this function to vad_filterbank.c.
        // Downsampling filter based on splitting filter and allpass functions.
        public static void WebRtcVad_Downsampling(Int16[] signal_in, Int16[] signal_out, Int32[] filter_state, int in_length, int filt_idx)
        {
            Int16 tmp16_1 = 0, tmp16_2 = 0;
            Int32 tmp32_1 = filter_state[filt_idx + 0];
            Int32 tmp32_2 = filter_state[filt_idx + 1];
            int n = 0;
            int half_length = (in_length >> 1);  // Downsampling by 2 gives half length.

            // Filter coefficients in Q13, filter state in Q0.
            int in_idx = 0, out_idx = 0;
            for (n = 0; n < half_length; n++)
            {
                // All-pass filtering upper branch.
                tmp16_1 = (Int16)((tmp32_1 >> 1) +
                    Macros.WEBRTC_SPL_MUL_16_16_RSFT(kAllPassCoefsQ13[0], signal_in[in_idx], 14));
                signal_out[out_idx] = tmp16_1;
                tmp32_1 = (Int32)(signal_in[in_idx]) -
                    Macros.WEBRTC_SPL_MUL_16_16_RSFT(kAllPassCoefsQ13[0], tmp16_1, 12);
                in_idx++;

                // All-pass filtering lower branch.
                tmp16_2 = (Int16)((tmp32_2 >> 1) +
                Macros.WEBRTC_SPL_MUL_16_16_RSFT(kAllPassCoefsQ13[1], signal_in[in_idx], 14));
                signal_out[out_idx] += tmp16_2;
                out_idx++;
                tmp32_2 = (Int32)(signal_in[in_idx]) -
                    Macros.WEBRTC_SPL_MUL_16_16_RSFT(kAllPassCoefsQ13[1], tmp16_2, 12);
                in_idx++;
            }
            // Store the filter states.
            filter_state[filt_idx + 0] = tmp32_1;
            filter_state[filt_idx + 1] = tmp32_2;
        }


        // Inserts |feature_value| into |low_value_vector|, if it is one of the 16
        // smallest values the last 100 frames. Then calculates and returns the median
        // of the five smallest values.
        public static Int16 WebRtcVad_FindMinimum(VadInstT self, Int16 feature_value, int channel)
        {
            int i = 0, j = 0;
            int position = -1;
            // Offset to beginning of the 16 minimum values in memory.
            int offset = (channel << 4);
            Int16 current_median = 1600;
            Int16 alpha = 0;
            Int32 tmp32 = 0;
            // Pointer to memory for the 16 minimum values and the age of each value of
            // the |channel|.
            ref Int16[] age = ref self.index_vector;
            ref Int16[] smallest_values = ref self.low_value_vector;

            Debug.Assert(channel < VadCore.kNumChannels);

            // Each value in |smallest_values| is getting 1 loop older. Update |age|, and
            // remove old values.
            for (i = 0; i < 16; i++)
            {
                if (age[offset + i] != 100)
                {
                    age[offset + i]++;
                }
                else
                {
                    // Too old value. Remove from memory and shift larger values downwards.
                    for (j = i; j < 16; j++)
                    {
                        smallest_values[offset + j] = smallest_values[offset + j + 1];
                        age[offset + j] = age[offset + j + 1];
                    }
                    age[offset + 15] = 101;
                    smallest_values[offset + 15] = 10000;
                }
            }

            // Check if |feature_value| is smaller than any of the values in
            // |smallest_values|. If so, find the |position| where to insert the new value
            // (|feature_value|).
            if (feature_value < smallest_values[offset + 7])
            {
                if (feature_value < smallest_values[offset + 3])
                {
                    if (feature_value < smallest_values[offset + 1])
                    {
                        if (feature_value < smallest_values[offset + 0])
                        {
                            position = 0;
                        }
                        else
                        {
                            position = 1;
                        }
                    }
                    else if (feature_value < smallest_values[offset + 2])
                    {
                        position = 2;
                    }
                    else
                    {
                        position = 3;
                    }
                }
                else if (feature_value < smallest_values[offset + 5])
                {
                    if (feature_value < smallest_values[offset + 4])
                    {
                        position = 4;
                    }
                    else
                    {
                        position = 5;
                    }
                }
                else if (feature_value < smallest_values[offset + 6])
                {
                    position = 6;
                }
                else
                {
                    position = 7;
                }
            }
            else if (feature_value < smallest_values[offset + 15])
            {
                if (feature_value < smallest_values[offset + 11])
                {
                    if (feature_value < smallest_values[offset + 9])
                    {
                        if (feature_value < smallest_values[offset + 8])
                        {
                            position = 8;
                        }
                        else
                        {
                            position = 9;
                        }
                    }
                    else if (feature_value < smallest_values[offset + 10])
                    {
                        position = 10;
                    }
                    else
                    {
                        position = 11;
                    }
                }
                else if (feature_value < smallest_values[offset + 13])
                {
                    if (feature_value < smallest_values[offset + 12])
                    {
                        position = 12;
                    }
                    else
                    {
                        position = 13;
                    }
                }
                else if (feature_value < smallest_values[offset + 14])
                {
                    position = 14;
                }
                else
                {
                    position = 15;
                }
            }

            // If we have detected a new small value, insert it at the correct position
            // and shift larger values up.
            if (position > -1)
            {
                for (i = 15; i > position; i--)
                {
                    smallest_values[offset + i] = smallest_values[offset + i - 1];
                    age[offset + i] = age[offset + i - 1];
                }
                smallest_values[offset + position] = feature_value;
                age[offset + position] = 1;
            }

            // Get |current_median|.
            if (self.frame_counter > 2)
            {
                current_median = smallest_values[offset + 2];
            }
            else if (self.frame_counter > 0)
            {
                current_median = smallest_values[offset + 0];
            }

            // Smooth the median value.
            if (self.frame_counter > 0)
            {
                if (current_median < self.mean_value[channel])
                {
                    alpha = kSmoothingDown;  // 0.2 in Q15.
                }
                else
                {
                    alpha = kSmoothingUp;  // 0.99 in Q15.
                }
            }
            tmp32 = (alpha + 1) * self.mean_value[channel];
            tmp32 += (Macros.WEBRTC_SPL_WORD16_MAX - alpha) * current_median;
            tmp32 += 16384;
            self.mean_value[channel] = (Int16)(tmp32 >> 15);

            return self.mean_value[channel];
        }
    }
}
