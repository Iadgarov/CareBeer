using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRtc.CommonAudio.SignalProcessing
{
    static class ResampleFractional
    {
        // interpolation coefficients
        static readonly Int16[][] kCoefficients48To32 = new Int16[][]{
           new Int16[] {778, -2050, 1087, 23285, 12903, -3783, 441, 222},
           new Int16[] {222, 441, -3783, 12903, 23285, 1087, -2050, 778}
        };

        static readonly Int16[][] kCoefficients32To24 = new Int16[][] {
            new Int16[] {767, -2362, 2434, 24406, 10620, -3838, 721, 90},
            new Int16[] {386, -381, -2646, 19062, 19062, -2646, -381, 386},
            new Int16[] {90, 721, -3838, 10620, 24406, 2434, -2362, 767}
        };

        static readonly Int16[][] kCoefficients44To32 = new Int16[][] {
            new Int16[] {117, -669, 2245, -6183, 26267, 13529, -3245, 845, -138},
            new Int16[] {-101, 612, -2283, 8532, 29790, -5138, 1789, -524, 91},
            new Int16[] {50, -292, 1016, -3064, 32010, 3933, -1147, 315, -53},
            new Int16[] {-156, 974, -3863, 18603, 21691, -6246, 2353, -712, 126}
        };


        //   Resampling ratio: 2/3
        // input:  Int32 (normalized, not saturated) :: size 3 * K
        // output: Int32 (shifted 15 positions to the left, + offset 16384) :: size 2 * K
        //      K: number of blocks

        public static void WebRtcSpl_Resample48khzTo32khz(Int32[] In, Int32[] Out, Int32 K)
        {
            /////////////////////////////////////////////////////////////
            // Filter operation:
            //
            // Perform resampling (3 input samples -> 2 output samples);
            // process in sub blocks of size 3 samples.
            Int32 tmp;
            Int32 m;
            int in_start = 0, out_start = 0;

            for (m = 0; m < K; m++)
            {
                tmp = 1 << 14;
                tmp += kCoefficients48To32[0][0] * In[in_start + 0];
                tmp += kCoefficients48To32[0][1] * In[in_start + 1];
                tmp += kCoefficients48To32[0][2] * In[in_start + 2];
                tmp += kCoefficients48To32[0][3] * In[in_start + 3];
                tmp += kCoefficients48To32[0][4] * In[in_start + 4];
                tmp += kCoefficients48To32[0][5] * In[in_start + 5];
                tmp += kCoefficients48To32[0][6] * In[in_start + 6];
                tmp += kCoefficients48To32[0][7] * In[in_start + 7];
                Out[out_start + 0] = tmp;

                tmp = 1 << 14;
                tmp += kCoefficients48To32[1][0] * In[in_start + 1];
                tmp += kCoefficients48To32[1][1] * In[in_start + 2];
                tmp += kCoefficients48To32[1][2] * In[in_start + 3];
                tmp += kCoefficients48To32[1][3] * In[in_start + 4];
                tmp += kCoefficients48To32[1][4] * In[in_start + 5];
                tmp += kCoefficients48To32[1][5] * In[in_start + 6];
                tmp += kCoefficients48To32[1][6] * In[in_start + 7];
                tmp += kCoefficients48To32[1][7] * In[in_start + 8];
                Out[out_start + 1] = tmp;

                // update pointers
                in_start += 3;
                out_start += 2;
            }
        }



        //   Resampling ratio: 3/4
        // input:  Int32 (normalized, not saturated) :: size 4 * K
        // output: Int32 (shifted 15 positions to the left, + offset 16384) :: size 3 * K
        //      K: number of blocks

        public static void WebRtcSpl_Resample32khzTo24khz(Int32[] In, Int32[] Out, Int32 K)
        {
            /////////////////////////////////////////////////////////////
            // Filter operation:
            //
            // Perform resampling (4 input samples -> 3 output samples);
            // process in sub blocks of size 4 samples.
            Int32 m;
            Int32 tmp;
            int in_start = 0, out_start = 0;

            for (m = 0; m < K; m++)
            {
                tmp = 1 << 14;
                tmp += kCoefficients32To24[0][0] * In[in_start + 0];
                tmp += kCoefficients32To24[0][1] * In[in_start + 1];
                tmp += kCoefficients32To24[0][2] * In[in_start + 2];
                tmp += kCoefficients32To24[0][3] * In[in_start + 3];
                tmp += kCoefficients32To24[0][4] * In[in_start + 4];
                tmp += kCoefficients32To24[0][5] * In[in_start + 5];
                tmp += kCoefficients32To24[0][6] * In[in_start + 6];
                tmp += kCoefficients32To24[0][7] * In[in_start + 7];
                Out[out_start + 0] = tmp;

                tmp = 1 << 14;
                tmp += kCoefficients32To24[1][0] * In[in_start + 1];
                tmp += kCoefficients32To24[1][1] * In[in_start + 2];
                tmp += kCoefficients32To24[1][2] * In[in_start + 3];
                tmp += kCoefficients32To24[1][3] * In[in_start + 4];
                tmp += kCoefficients32To24[1][4] * In[in_start + 5];
                tmp += kCoefficients32To24[1][5] * In[in_start + 6];
                tmp += kCoefficients32To24[1][6] * In[in_start + 7];
                tmp += kCoefficients32To24[1][7] * In[in_start + 8];
                Out[out_start + 1] = tmp;

                tmp = 1 << 14;
                tmp += kCoefficients32To24[2][0] * In[in_start + 2];
                tmp += kCoefficients32To24[2][1] * In[in_start + 3];
                tmp += kCoefficients32To24[2][2] * In[in_start + 4];
                tmp += kCoefficients32To24[2][3] * In[in_start + 5];
                tmp += kCoefficients32To24[2][4] * In[in_start + 6];
                tmp += kCoefficients32To24[2][5] * In[in_start + 7];
                tmp += kCoefficients32To24[2][6] * In[in_start + 8];
                tmp += kCoefficients32To24[2][7] * In[in_start + 9];
                Out[out_start + 2] = tmp;

                // update pointers
                in_start += 4;
                out_start += 3;
            }
        }



        //
        // fractional resampling filters
        //   Fout = 11/16 * Fin
        //   Fout =  8/11 * Fin
        //

        // compute two inner-products and store them to output array
        //static void WebRtcSpl_ResampDotProduct(Int32[] in1, Int32[] in2,
        //                               Int16[] coef_ptr, Int32[] out1,
        //                               Int32[] out2)
        //{
        //    Int32 tmp1 = 16384;
        //    Int32 tmp2 = 16384;
        //    Int16 coef;

        //    coef = coef_ptr[0];
        //    tmp1 += coef * in1[0];
        //    tmp2 += coef * in2[-0];

        //    coef = coef_ptr[1];
        //    tmp1 += coef * in1[1];
        //    tmp2 += coef * in2[-1];

        //    coef = coef_ptr[2];
        //    tmp1 += coef * in1[2];
        //    tmp2 += coef * in2[-2];

        //    coef = coef_ptr[3];
        //    tmp1 += coef * in1[3];
        //    tmp2 += coef * in2[-3];

        //    coef = coef_ptr[4];
        //    tmp1 += coef * in1[4];
        //    tmp2 += coef * in2[-4];

        //    coef = coef_ptr[5];
        //    tmp1 += coef * in1[5];
        //    tmp2 += coef * in2[-5];

        //    coef = coef_ptr[6];
        //    tmp1 += coef * in1[6];
        //    tmp2 += coef * in2[-6];

        //    coef = coef_ptr[7];
        //    tmp1 += coef * in1[7];
        //    tmp2 += coef * in2[-7];

        //    coef = coef_ptr[8];
        //    *out1 = tmp1 + coef * in1[8];
        //    *out2 = tmp2 + coef * in2[-8];
        //}



        //   Resampling ratio: 8/11
        // input:  Int32 (normalized, not saturated) :: size 11 * K
        // output: Int32 (shifted 15 positions to the left, + offset 16384) :: size  8 * K
        //      K: number of blocks

        //public static void WebRtcSpl_Resample44khzTo32khz(Int32[] In, Int32[] Out, Int32 K)
        //{
        //    /////////////////////////////////////////////////////////////
        //    // Filter operation:
        //    //
        //    // Perform resampling (11 input samples -> 8 output samples);
        //    // process in sub blocks of size 11 samples.
        //    Int32 tmp;
        //    Int32 m;

        //    for (m = 0; m < K; m++)
        //    {
        //        tmp = 1 << 14;

        //        // first output sample
        //        Out[0] = ((Int32)In[3] << 15) + tmp;

        //        // sum and accumulate filter coefficients and input samples
        //        tmp += kCoefficients44To32[3][0] * In[5];
        //        tmp += kCoefficients44To32[3][1] * In[6];
        //        tmp += kCoefficients44To32[3][2] * In[7];
        //        tmp += kCoefficients44To32[3][3] * In[8];
        //        tmp += kCoefficients44To32[3][4] * In[9];
        //        tmp += kCoefficients44To32[3][5] * In[10];
        //        tmp += kCoefficients44To32[3][6] * In[11];
        //        tmp += kCoefficients44To32[3][7] * In[12];
        //        tmp += kCoefficients44To32[3][8] * In[13];
        //        Out[4] = tmp;

        //        // sum and accumulate filter coefficients and input samples
        //        WebRtcSpl_ResampDotProduct(&In[0], &In[17], kCoefficients44To32[0], &Out[1], &Out[7]);

        //        // sum and accumulate filter coefficients and input samples
        //        WebRtcSpl_ResampDotProduct(&In[2], &In[15], kCoefficients44To32[1], &Out[2], &Out[6]);

        //        // sum and accumulate filter coefficients and input samples
        //        WebRtcSpl_ResampDotProduct(&In[3], &In[14], kCoefficients44To32[2], &Out[3], &Out[5]);

        //        // update pointers
        //        In += 11;
        //        Out += 8;
        //    }
        //}
    }
}
