﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WebRtc.CommonAudio.Vad
{
    using VadInst = VadInstT;

    static class WebRtcVad
    {
        const int kInitCheck = 42;
        static readonly int[] kValidRates = { 8000, 16000, 32000, 48000 };
        const uint kRatesSize = 4 * sizeof(int);
        const int kMaxFrameLengthMs = 30;

        public static VadInst WebRtcVad_Create()
        {
            VadInst self = new VadInst()
            {

                //CHECK: WebRtcSpl_Init();

                init_flag = 0
            };

            return self;
        }


        public static int WebRtcVad_Init(VadInst handle)
        {
            // Initialize the core VAD component.
            return VadCore.WebRtcVad_InitCore(handle);
        }


        // TODO(bjornv): Move WebRtcVad_set_mode_core() code here.
        public static int WebRtcVad_set_mode(VadInst handle, int mode)
        {
            VadInst self = handle;

            if (handle == null)
            {
                return -1;
            }
            if (self.init_flag != kInitCheck)
            {
                return -1;
            }

            return VadCore.WebRtcVad_set_mode_core(self, mode);
        }


        public static int WebRtcVad_Process(VadInst handle, int fs, Int16[] audio_frame, int frame_length)
        {
            int vad = -1;
            VadInst self = handle;

            if (handle == null)
            {
                return -1;
            }

            if (self.init_flag != kInitCheck)
            {
                return -1;
            }
            if (audio_frame == null)
            {
                return -1;
            }
            if (WebRtcVad_ValidRateAndFrameLength(fs, frame_length) != 0)
            {
                return -1;
            }

            if (fs == 48000)
            {
                vad = VadCore.WebRtcVad_CalcVad48khz(self, audio_frame, frame_length);
            }
            else if (fs == 32000)
            {
                vad = VadCore.WebRtcVad_CalcVad32khz(self, audio_frame, frame_length);
            }
            else if (fs == 16000)
            {
                vad = VadCore.WebRtcVad_CalcVad16khz(self, audio_frame, frame_length);
            }
            else if (fs == 8000)
            {
                vad = VadCore.WebRtcVad_CalcVad8khz(self, audio_frame, frame_length);
            }

            if (vad > 0)
            {
                vad = 1;
            }
            return vad;
        }



        public static int WebRtcVad_ValidRateAndFrameLength(int rate, int frame_length)
        {
            int return_value = -1;
            uint i;
            int valid_length_ms;
            int valid_length;

            // We only allow 10, 20 or 30 ms frames. Loop through valid frame rates and
            // see if we have a matching pair.
            for (i = 0; i < kRatesSize; i++)
            {
                if (kValidRates[i] == rate)
                {
                    for (valid_length_ms = 10; valid_length_ms <= kMaxFrameLengthMs;
                        valid_length_ms += 10)
                    {
                        valid_length = (kValidRates[i] / 1000 * valid_length_ms);
                        if (frame_length == valid_length)
                        {
                            return_value = 0;
                            break;
                        }
                    }
                    break;
                }
            }

            return return_value;
        }




    }
}
