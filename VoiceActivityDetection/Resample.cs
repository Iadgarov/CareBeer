using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRtc.CommonAudio.SignalProcessing
{
    class WebRtcSpl_State48khzTo16khz
    {
        public Int32[] S_48_48 = new Int32[16];
        public Int32[] S_48_32 = new Int32[8];
        public Int32[] S_32_16 = new Int32[8];
    }

    class WebRtcSpl_State16khzTo48khz
    {
        public Int32[] S_16_32 = new Int32[8];
        public Int32[] S_32_24 = new Int32[8];
        public Int32[] S_24_48 = new Int32[8];
    }

    public class WebRtcSpl_State48khzTo8khz
    {
        public Int32[] S_48_24 = new Int32[8];
        public Int32[] S_24_24 = new Int32[16];
        public Int32[] S_24_16 = new Int32[8];
        public Int32[] S_16_8 = new Int32[8];
    }

    class WebRtcSpl_State8khzTo48khz
    {
        public Int32[] S_8_16 = new Int32[8];
        public Int32[] S_16_12 = new Int32[8];
        public Int32[] S_12_24 = new Int32[8];
        public Int32[] S_24_48 = new Int32[8];
    }



    static class Resample
    {
        ////////////////////////////
        ///// 48 kHz . 16 kHz /////
        ////////////////////////////

        // 48 . 16 resampler
        public static void WebRtcSpl_Resample48khzTo16khz(Int16[] input, Int16[] output,
                                            WebRtcSpl_State48khzTo16khz state, Int32[] tmpmem)
        {
            ///// 48 -. 48(LP) /////
            // Int16  in[480]
            // Int32 out[480]
            /////
            ResampleBy2Internal.WebRtcSpl_LPBy2ShortToInt(input, 480, tmpmem, 16, state.S_48_48);

            ///// 48 -. 32 /////
            // Int32  in[480]
            // Int32 out[320]
            /////
            // copy state to and from input array
            //memcpy(tmpmem + 8, state.S_48_32, 8 * sizeof(Int32));
            Array.Copy(state.S_48_32, 0, tmpmem, 8, 8);
            //memcpy(state.S_48_32, tmpmem + 488, 8 * sizeof(Int32));
            Array.Copy(tmpmem, 488, state.S_48_32, 0, 8);
            ResampleFractional.WebRtcSpl_Resample48khzTo32khz(tmpmem.Skip(8).ToArray(), tmpmem, 160);

            ///// 32 -. 16 /////
            // Int32  in[320]
            // Int16 out[160]
            /////
            ResampleBy2Internal.WebRtcSpl_DownBy2IntToShort(tmpmem, 320, output, state.S_32_16);
        }


        // initialize state of 48 . 16 resampler
        public static void WebRtcSpl_ResetResample48khzTo16khz(WebRtcSpl_State48khzTo16khz state)
        {
            Array.Clear(state.S_48_48, 0, 16);
            Array.Clear(state.S_48_32, 0, 8);
            Array.Clear(state.S_32_16, 0, 8);
            //memset(state.S_48_48, 0, 16 * sizeof(Int32));
            //memset(state.S_48_32, 0, 8 * sizeof(Int32));
            //memset(state.S_32_16, 0, 8 * sizeof(Int32));
        }

        ////////////////////////////
        ///// 16 kHz . 48 kHz /////
        ////////////////////////////

        // 16 . 48 resampler
        public static void WebRtcSpl_Resample16khzTo48khz(Int16[] input, Int16[] output,
                                            WebRtcSpl_State16khzTo48khz state, Int32[] tmpmem)
        {
            ///// 16 -. 32 /////
            // Int16  in[160]
            // Int32 out[320]
            /////
            ResampleBy2Internal.WebRtcSpl_UpBy2ShortToInt(input, 160, tmpmem, 16, state.S_16_32);

            ///// 32 -. 24 /////
            // Int32  in[320]
            // Int32 out[240]
            // copy state to and from input array
            /////
            Array.Copy(state.S_32_24, 0, tmpmem, 8, 8);
            //memcpy(tmpmem + 8, state.S_32_24, 8 * sizeof(Int32));
            Array.Copy(tmpmem, 328, state.S_32_24, 0, 8);
            //memcpy(state.S_32_24, tmpmem + 328, 8 * sizeof(Int32));
            ResampleFractional.WebRtcSpl_Resample32khzTo24khz(tmpmem.Skip(8).ToArray(), tmpmem, 80);

            ///// 24 -. 48 /////
            // Int32  in[240]
            // Int16 out[480]
            /////
            ResampleBy2Internal.WebRtcSpl_UpBy2IntToShort(tmpmem, 240, output, state.S_24_48);
        }


        // initialize state of 16 . 48 resampler
        public static void WebRtcSpl_ResetResample16khzTo48khz(WebRtcSpl_State16khzTo48khz state)
        {
            Array.Clear(state.S_16_32, 0, 8);
            Array.Clear(state.S_32_24, 0, 8);
            Array.Clear(state.S_24_48, 0, 8);
            //memset(state.S_16_32, 0, 8 * sizeof(Int32));
            //memset(state.S_32_24, 0, 8 * sizeof(Int32));
            //memset(state.S_24_48, 0, 8 * sizeof(Int32));
        }


        ////////////////////////////
        ///// 48 kHz .  8 kHz /////
        ////////////////////////////

        // 48 . 8 resampler
        public static void WebRtcSpl_Resample48khzTo8khz(Int16[] input, Int16[] output, int out_start,
                                           WebRtcSpl_State48khzTo8khz state, Int32[] tmpmem)
        {
            ///// 48 -. 24 /////
            // Int16  in[480]
            // Int32 out[240]
            /////
            ResampleBy2Internal.WebRtcSpl_DownBy2ShortToInt(input, 480, tmpmem, 256, state.S_48_24);

            ///// 24 -. 24(LP) /////
            // Int32  in[240]
            // Int32 out[240]
            /////
            ResampleBy2Internal.WebRtcSpl_LPBy2IntToInt(tmpmem, 256, 240, tmpmem, 16, state.S_24_24);

            ///// 24 -. 16 /////
            // Int32  in[240]
            // Int32 out[160]
            /////
            // copy state to and from input array
            //memcpy(tmpmem + 8, state.S_24_16, 8 * sizeof(Int32));
            Array.Copy(state.S_24_16, 0, tmpmem, 8, 8);
            //memcpy(state.S_24_16, tmpmem + 248, 8 * sizeof(Int32));
            Array.Copy(tmpmem, 248, state.S_24_16, 0, 8);
            ResampleFractional.WebRtcSpl_Resample48khzTo32khz(tmpmem.Skip(8).ToArray(), tmpmem, 80);

            ///// 16 -. 8 /////
            // Int32  in[160]
            // Int16 out[80]
            /////
            ResampleBy2Internal.WebRtcSpl_DownBy2IntToShort(tmpmem, 160, output, state.S_16_8);
        }


        // initialize state of 48 . 8 resampler
        public static void WebRtcSpl_ResetResample48khzTo8khz(WebRtcSpl_State48khzTo8khz state)
        {
            Array.Clear(state.S_48_24, 0, 8);
            Array.Clear(state.S_24_24, 0, 16);
            Array.Clear(state.S_24_16, 0, 8);
            Array.Clear(state.S_16_8, 0, 8);
            //memset(state.S_48_24, 0, 8 * sizeof(Int32));
            //memset(state.S_24_24, 0, 16 * sizeof(Int32));
            //memset(state.S_24_16, 0, 8 * sizeof(Int32));
            //memset(state.S_16_8, 0, 8 * sizeof(Int32));
        }

        ////////////////////////////
        /////  8 kHz . 48 kHz /////
        ////////////////////////////

        // 8 . 48 resampler
        public static void WebRtcSpl_Resample8khzTo48khz(Int16[] input, Int16[] output,
                                           WebRtcSpl_State8khzTo48khz state, Int32[] tmpmem)
        {
            ///// 8 -. 16 /////
            // Int16  in[80]
            // Int32 out[160]
            /////
            ResampleBy2Internal.WebRtcSpl_UpBy2ShortToInt(input, 80, tmpmem, 264, state.S_8_16);

            ///// 16 -. 12 /////
            // Int32  in[160]
            // Int32 out[120]
            /////
            // copy state to and from input array
            Array.Copy(state.S_16_12, 0, tmpmem, 256, 8);
            //memcpy(tmpmem + 256, state.S_16_12, 8 * sizeof(Int32));
            Array.Copy(tmpmem, 416, state.S_16_12, 0, 8);
            //memcpy(state.S_16_12, tmpmem + 416, 8 * sizeof(Int32));
            // CHECK
            int[] tmp = tmpmem.Skip(240).ToArray();
            ResampleFractional.WebRtcSpl_Resample32khzTo24khz(tmpmem.Skip(256).ToArray(), tmp, 40);
            Array.Copy(tmp, 0, tmpmem, 240, tmp.Length);

            ///// 12 -. 24 /////
            // Int32  in[120]
            // Int16 out[240]
            /////
            ResampleBy2Internal.WebRtcSpl_UpBy2IntToInt(tmpmem, 240, 120, tmpmem, state.S_12_24);

            ///// 24 -. 48 /////
            // Int32  in[240]
            // Int16 out[480]
            /////
            ResampleBy2Internal.WebRtcSpl_UpBy2IntToShort(tmpmem, 240, output, state.S_24_48);
        }


        // initialize state of 8 . 48 resampler
        public static void WebRtcSpl_ResetResample8khzTo48khz(WebRtcSpl_State8khzTo48khz state)
        {
            Array.Clear(state.S_8_16, 0, 8 );
            Array.Clear(state.S_16_12, 0, 8);
            Array.Clear(state.S_12_24, 0, 8);
            Array.Clear(state.S_24_48, 0, 8);
            //memset(state.S_8_16, 0, 8 * sizeof(Int32));
            //memset(state.S_16_12, 0, 8 * sizeof(Int32));
            //memset(state.S_12_24, 0, 8 * sizeof(Int32));
            //memset(state.S_24_48, 0, 8 * sizeof(Int32));
        }
    }
}
