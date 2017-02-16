using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRtc.CommonAudio.SignalProcessing
{
    static class ResampleBy2Internal
    {
        // allpass filter coefficients.
        static readonly Int16[][] kResampleAllpass = new Int16[][] {
            new Int16[]{821, 6110, 12382},
            new Int16[]{3050, 9368, 15063}
        };

        //
        //   decimator
        // input:  Int32 (shifted 15 positions to the left, + offset 16384) OVERWRITTEN!
        // output: Int16 (saturated) (of length len/2)
        // state:  filter state array; length = 8

        public static void WebRtcSpl_DownBy2IntToShort(Int32[] input, Int32 len, Int16[] output, Int32[] state)
        {
            Int32 tmp0, tmp1, diff;
            Int32 i;
            int in_start = 0;

            len >>= 1;

            // lower allpass filter (operates on even input samples)
            for (i = 0; i < len; i++)
            {
                tmp0 = input[in_start + (i << 1)];
                diff = tmp0 - state[1];
                // scale down and round
                diff = (diff + (1 << 13)) >> 14;
                tmp1 = state[0] + diff * kResampleAllpass[1][0];
                state[0] = tmp0;
                diff = tmp1 - state[2];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                tmp0 = state[1] + diff * kResampleAllpass[1][1];
                state[1] = tmp1;
                diff = tmp0 - state[3];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                state[3] = state[2] + diff * kResampleAllpass[1][2];
                state[2] = tmp0;

                // divide by two and store temporarily
                input[in_start + (i << 1)] = (state[3] >> 1);
            }

            in_start++;

            // upper allpass filter (operates on odd input samples)
            for (i = 0; i < len; i++)
            {
                tmp0 = input[in_start + (i << 1)];
                diff = tmp0 - state[5];
                // scale down and round
                diff = (diff + (1 << 13)) >> 14;
                tmp1 = state[4] + diff * kResampleAllpass[0][0];
                state[4] = tmp0;
                diff = tmp1 - state[6];
                // scale down and round
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                tmp0 = state[5] + diff * kResampleAllpass[0][1];
                state[5] = tmp1;
                diff = tmp0 - state[7];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                state[7] = state[6] + diff * kResampleAllpass[0][2];
                state[6] = tmp0;

                // divide by two and store temporarily
                input[in_start + (i << 1)] = (state[7] >> 1);
            }

            in_start--;

            // combine allpass outputs
            for (i = 0; i < len; i += 2)
            {
                // divide by two, add both allpass outputs and round
                tmp0 = (input[in_start + (i << 1)] + input[in_start + (i << 1) + 1]) >> 15;
                tmp1 = (input[in_start + (i << 1) + 2] + input[in_start + (i << 1) + 3]) >> 15;
                if (tmp0 > (Int32)0x00007FFF)
                    tmp0 = 0x00007FFF;
                if (tmp0 < unchecked((Int32)0xFFFF8000))
                    tmp0 = unchecked((Int32)0xFFFF8000);
                output[i] = (Int16)tmp0;
                if (tmp1 > (Int32)0x00007FFF)
                    tmp1 = 0x00007FFF;
                if (tmp1 < unchecked((Int32)0xFFFF8000))
                    tmp1 = unchecked((Int32)0xFFFF8000);
                output[i + 1] = (Int16)tmp1;
            }
        }



        //
        //   decimator
        // input:  Int16
        // output: Int32 (shifted 15 positions to the left, + offset 16384) (of length len/2)
        // state:  filter state array; length = 8

        public static void WebRtcSpl_DownBy2ShortToInt(Int16[] input, Int32 len, Int32[] output, int out_start, Int32[] state)
        {
            Int32 tmp0, tmp1, diff;
            Int32 i;
            int in_start = 0;

            len >>= 1;

            // lower allpass filter (operates on even input samples)
            for (i = 0; i < len; i++)
            {
                tmp0 = (input[in_start + (i << 1)] << 15) + (1 << 14);
                diff = tmp0 - state[1];
                // scale down and round
                diff = (diff + (1 << 13)) >> 14;
                tmp1 = state[0] + diff * kResampleAllpass[1][0];
                state[0] = tmp0;
                diff = tmp1 - state[2];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                tmp0 = state[1] + diff * kResampleAllpass[1][1];
                state[1] = tmp1;
                diff = tmp0 - state[3];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                state[3] = state[2] + diff * kResampleAllpass[1][2];
                state[2] = tmp0;

                // divide by two and store temporarily
                output[out_start + i] = (state[3] >> 1);
            }

            in_start++;

            // upper allpass filter (operates on odd input samples)
            for (i = 0; i < len; i++)
            {
                tmp0 = (input[in_start + (i << 1)] << 15) + (1 << 14);
                diff = tmp0 - state[5];
                // scale down and round
                diff = (diff + (1 << 13)) >> 14;
                tmp1 = state[4] + diff * kResampleAllpass[0][0];
                state[4] = tmp0;
                diff = tmp1 - state[6];
                // scale down and round
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                tmp0 = state[5] + diff * kResampleAllpass[0][1];
                state[5] = tmp1;
                diff = tmp0 - state[7];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                state[7] = state[6] + diff * kResampleAllpass[0][2];
                state[6] = tmp0;

                // divide by two and store temporarily
                output[out_start + i] += (state[7] >> 1);
            }

            in_start--;
        }


        //
        //   interpolator
        // input:  Int16
        // output: Int32 (normalized, not saturated) (of length len*2)
        // state:  filter state array; length = 8
        public static void WebRtcSpl_UpBy2ShortToInt(Int16[] input, Int32 len, Int32[] output, int out_start, Int32[] state)
        {
            Int32 tmp0, tmp1, diff;
            Int32 i;

            // upper allpass filter (generates odd output samples)
            for (i = 0; i < len; i++)
            {
                tmp0 = (input[i] << 15) + (1 << 14);
                diff = tmp0 - state[5];
                // scale down and round
                diff = (diff + (1 << 13)) >> 14;
                tmp1 = state[4] + diff * kResampleAllpass[0][0];
                state[4] = tmp0;
                diff = tmp1 - state[6];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                tmp0 = state[5] + diff * kResampleAllpass[0][1];
                state[5] = tmp1;
                diff = tmp0 - state[7];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                state[7] = state[6] + diff * kResampleAllpass[0][2];
                state[6] = tmp0;

                // scale down, round and store
                output[out_start + (i << 1)] = state[7] >> 15;
            }

            out_start++;

            // lower allpass filter (generates even output samples)
            for (i = 0; i < len; i++)
            {
                tmp0 = ((Int32)input[i] << 15) + (1 << 14);
                diff = tmp0 - state[1];
                // scale down and round
                diff = (diff + (1 << 13)) >> 14;
                tmp1 = state[0] + diff * kResampleAllpass[1][0];
                state[0] = tmp0;
                diff = tmp1 - state[2];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                tmp0 = state[1] + diff * kResampleAllpass[1][1];
                state[1] = tmp1;
                diff = tmp0 - state[3];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                state[3] = state[2] + diff * kResampleAllpass[1][2];
                state[2] = tmp0;

                // scale down, round and store
                output[out_start + (i << 1)] = state[3] >> 15;
            }
        }



        //
        //   interpolator
        // input:  Int32 (shifted 15 positions to the left, + offset 16384)
        // output: Int32 (shifted 15 positions to the left, + offset 16384) (of length len*2)
        // state:  filter state array; length = 8
        public static void WebRtcSpl_UpBy2IntToInt(Int32[] input, int in_start, Int32 len, Int32[] output, Int32[] state)
        {
            Int32 tmp0, tmp1, diff;
            Int32 i;
            int out_start = 0;

            // upper allpass filter (generates odd output samples)
            for (i = 0; i < len; i++)
            {
                tmp0 = input[in_start + i];
                diff = tmp0 - state[5];
                // scale down and round
                diff = (diff + (1 << 13)) >> 14;
                tmp1 = state[4] + diff * kResampleAllpass[0][0];
                state[4] = tmp0;
                diff = tmp1 - state[6];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                tmp0 = state[5] + diff * kResampleAllpass[0][1];
                state[5] = tmp1;
                diff = tmp0 - state[7];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                state[7] = state[6] + diff * kResampleAllpass[0][2];
                state[6] = tmp0;

                // scale down, round and store
                output[out_start + (i << 1)] = state[7];
            }

            out_start++;

            // lower allpass filter (generates even output samples)
            for (i = 0; i < len; i++)
            {
                tmp0 = input[in_start + i];
                diff = tmp0 - state[1];
                // scale down and round
                diff = (diff + (1 << 13)) >> 14;
                tmp1 = state[0] + diff * kResampleAllpass[1][0];
                state[0] = tmp0;
                diff = tmp1 - state[2];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                tmp0 = state[1] + diff * kResampleAllpass[1][1];
                state[1] = tmp1;
                diff = tmp0 - state[3];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                state[3] = state[2] + diff * kResampleAllpass[1][2];
                state[2] = tmp0;

                // scale down, round and store
                output[out_start + (i << 1)] = state[3];
            }
        }



        //
        //   interpolator
        // input:  Int32 (shifted 15 positions to the left, + offset 16384)
        // output: Int16 (saturated) (of length len*2)
        // state:  filter state array; length = 8
        public static void WebRtcSpl_UpBy2IntToShort(Int32[] input, Int32 len, Int16[] output, Int32[] state)
        {
            Int32 tmp0, tmp1, diff;
            Int32 i;
            int out_start = 0;

            // upper allpass filter (generates odd output samples)
            for (i = 0; i < len; i++)
            {
                tmp0 = input[i];
                diff = tmp0 - state[5];
                // scale down and round
                diff = (diff + (1 << 13)) >> 14;
                tmp1 = state[4] + diff * kResampleAllpass[0][0];
                state[4] = tmp0;
                diff = tmp1 - state[6];
                // scale down and round
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                tmp0 = state[5] + diff * kResampleAllpass[0][1];
                state[5] = tmp1;
                diff = tmp0 - state[7];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                state[7] = state[6] + diff * kResampleAllpass[0][2];
                state[6] = tmp0;

                // scale down, saturate and store
                tmp1 = state[7] >> 15;
                if (tmp1 > 0x00007FFF)
                    tmp1 = 0x00007FFF;
                if (tmp1 < unchecked((Int32)0xFFFF8000))
                    tmp1 = unchecked((Int32)0xFFFF8000);
                output[out_start + (i << 1)] = (Int16)tmp1;
            }

            out_start++;

            // lower allpass filter (generates even output samples)
            for (i = 0; i < len; i++)
            {
                tmp0 = input[i];
                diff = tmp0 - state[1];
                // scale down and round
                diff = (diff + (1 << 13)) >> 14;
                tmp1 = state[0] + diff * kResampleAllpass[1][0];
                state[0] = tmp0;
                diff = tmp1 - state[2];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                tmp0 = state[1] + diff * kResampleAllpass[1][1];
                state[1] = tmp1;
                diff = tmp0 - state[3];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                state[3] = state[2] + diff * kResampleAllpass[1][2];
                state[2] = tmp0;

                // scale down, saturate and store
                tmp1 = state[3] >> 15;
                if (tmp1 > 0x00007FFF)
                    tmp1 = 0x00007FFF;
                if (tmp1 < unchecked((Int32)0xFFFF8000))
                    tmp1 = unchecked((Int32)0xFFFF8000);
                output[out_start + (i << 1)] = (Int16)tmp1;
            }
        }



        //   lowpass filter
        // input:  Int16
        // output: Int32 (normalized, not saturated)
        // state:  filter state array; length = 8
        public static void WebRtcSpl_LPBy2ShortToInt(Int16[] input, Int32 len, Int32[] output, int out_start, Int32[] state)
        {
            Int32 tmp0, tmp1, diff;
            Int32 i;
            int in_start = 0;

            len >>= 1;

            // lower allpass filter: odd input -> even output samples
            in_start++;
            // initial state of polyphase delay element
            tmp0 = state[12];
            for (i = 0; i < len; i++)
            {
                diff = tmp0 - state[1];
                // scale down and round
                diff = (diff + (1 << 13)) >> 14;
                tmp1 = state[0] + diff * kResampleAllpass[1][0];
                state[0] = tmp0;
                diff = tmp1 - state[2];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                tmp0 = state[1] + diff * kResampleAllpass[1][1];
                state[1] = tmp1;
                diff = tmp0 - state[3];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                state[3] = state[2] + diff * kResampleAllpass[1][2];
                state[2] = tmp0;

                // scale down, round and store
                output[out_start + (i << 1)] = state[3] >> 1;
                tmp0 = (input[in_start + (i << 1)] << 15) + (1 << 14);
            }
            in_start--;

            // upper allpass filter: even input -> even output samples
            for (i = 0; i < len; i++)
            {
                tmp0 = (input[in_start + (i << 1)] << 15) + (1 << 14);
                diff = tmp0 - state[5];
                // scale down and round
                diff = (diff + (1 << 13)) >> 14;
                tmp1 = state[4] + diff * kResampleAllpass[0][0];
                state[4] = tmp0;
                diff = tmp1 - state[6];
                // scale down and round
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                tmp0 = state[5] + diff * kResampleAllpass[0][1];
                state[5] = tmp1;
                diff = tmp0 - state[7];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                state[7] = state[6] + diff * kResampleAllpass[0][2];
                state[6] = tmp0;

                // average the two allpass outputs, scale down and store
                output[out_start + (i << 1)] = (output[out_start + (i << 1)] + (state[7] >> 1)) >> 15;
            }

            // switch to odd output samples
            out_start++;

            // lower allpass filter: even input -> odd output samples
            for (i = 0; i < len; i++)
            {
                tmp0 = (input[in_start + (i << 1)] << 15) + (1 << 14);
                diff = tmp0 - state[9];
                // scale down and round
                diff = (diff + (1 << 13)) >> 14;
                tmp1 = state[8] + diff * kResampleAllpass[1][0];
                state[8] = tmp0;
                diff = tmp1 - state[10];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                tmp0 = state[9] + diff * kResampleAllpass[1][1];
                state[9] = tmp1;
                diff = tmp0 - state[11];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                state[11] = state[10] + diff * kResampleAllpass[1][2];
                state[10] = tmp0;

                // scale down, round and store
                output[out_start + (i << 1)] = state[11] >> 1;
            }

            // upper allpass filter: odd input -> odd output samples
            in_start++;
            for (i = 0; i < len; i++)
            {
                tmp0 = (input[in_start + (i << 1)] << 15) + (1 << 14);
                diff = tmp0 - state[13];
                // scale down and round
                diff = (diff + (1 << 13)) >> 14;
                tmp1 = state[12] + diff * kResampleAllpass[0][0];
                state[12] = tmp0;
                diff = tmp1 - state[14];
                // scale down and round
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                tmp0 = state[13] + diff * kResampleAllpass[0][1];
                state[13] = tmp1;
                diff = tmp0 - state[15];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                state[15] = state[14] + diff * kResampleAllpass[0][2];
                state[14] = tmp0;

                // average the two allpass outputs, scale down and store
                output[out_start + (i << 1)] = (output[out_start + (i << 1)] + (state[15] >> 1)) >> 15;
            }
        }



        //   lowpass filter
        // input:  Int32 (shifted 15 positions to the left, + offset 16384)
        // output: Int32 (normalized, not saturated)
        // state:  filter state array; length = 8
        public static void WebRtcSpl_LPBy2IntToInt(Int32[] input, int in_start, Int32 len, Int32[] output, int out_start, Int32[] state)
        {
            Int32 tmp0, tmp1, diff;
            Int32 i;

            len >>= 1;

            // lower allpass filter: odd input -> even output samples
            in_start++;
            // initial state of polyphase delay element
            tmp0 = state[12];
            for (i = 0; i < len; i++)
            {
                diff = tmp0 - state[1];
                // scale down and round
                diff = (diff + (1 << 13)) >> 14;
                tmp1 = state[0] + diff * kResampleAllpass[1][0];
                state[0] = tmp0;
                diff = tmp1 - state[2];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                tmp0 = state[1] + diff * kResampleAllpass[1][1];
                state[1] = tmp1;
                diff = tmp0 - state[3];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                state[3] = state[2] + diff * kResampleAllpass[1][2];
                state[2] = tmp0;

                // scale down, round and store
                output[out_start + (i << 1)] = state[3] >> 1;
                tmp0 = input[in_start + (i << 1)];
            }
            in_start--;

            // upper allpass filter: even input -> even output samples
            for (i = 0; i < len; i++)
            {
                tmp0 = input[in_start + (i << 1)];
                diff = tmp0 - state[5];
                // scale down and round
                diff = (diff + (1 << 13)) >> 14;
                tmp1 = state[4] + diff * kResampleAllpass[0][0];
                state[4] = tmp0;
                diff = tmp1 - state[6];
                // scale down and round
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                tmp0 = state[5] + diff * kResampleAllpass[0][1];
                state[5] = tmp1;
                diff = tmp0 - state[7];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                state[7] = state[6] + diff * kResampleAllpass[0][2];
                state[6] = tmp0;

                // average the two allpass outputs, scale down and store
                output[out_start + (i << 1)] = (output[out_start + (i << 1)] + (state[7] >> 1)) >> 15;
            }

            // switch to odd output samples
            out_start++;

            // lower allpass filter: even input -> odd output samples
            for (i = 0; i < len; i++)
            {
                tmp0 = input[in_start + (i << 1)];
                diff = tmp0 - state[9];
                // scale down and round
                diff = (diff + (1 << 13)) >> 14;
                tmp1 = state[8] + diff * kResampleAllpass[1][0];
                state[8] = tmp0;
                diff = tmp1 - state[10];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                tmp0 = state[9] + diff * kResampleAllpass[1][1];
                state[9] = tmp1;
                diff = tmp0 - state[11];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                state[11] = state[10] + diff * kResampleAllpass[1][2];
                state[10] = tmp0;

                // scale down, round and store
                output[out_start + (i << 1)] = state[11] >> 1;
            }

            // upper allpass filter: odd input -> odd output samples
            in_start++;
            for (i = 0; i < len; i++)
            {
                tmp0 = input[in_start + (i << 1)];
                diff = tmp0 - state[13];
                // scale down and round
                diff = (diff + (1 << 13)) >> 14;
                tmp1 = state[12] + diff * kResampleAllpass[0][0];
                state[12] = tmp0;
                diff = tmp1 - state[14];
                // scale down and round
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                tmp0 = state[13] + diff * kResampleAllpass[0][1];
                state[13] = tmp1;
                diff = tmp0 - state[15];
                // scale down and truncate
                diff = diff >> 14;
                if (diff < 0)
                    diff += 1;
                state[15] = state[14] + diff * kResampleAllpass[0][2];
                state[14] = tmp0;

                // average the two allpass outputs, scale down and store
                output[out_start + (i << 1)] = (output[out_start + (i << 1)] + (state[15] >> 1)) >> 15;
            }
        }
    }
}
