using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WebRtc.CommonAudio.Vad
{
    using VadInst = VadInstT;

    public static class WebRtcVad
    {
        const int kInitCheck = 42;
        static readonly int[] kValidRates = { 8000, 16000, 32000, 48000 };
        const uint kRatesSize = 4 * sizeof(int);
        const int kMaxFrameLengthMs = 30;


        // Creates an instance to the VAD structure.
        //
        // - handle [o] : Pointer to the VAD instance that should be created.
        //
        // returns      : 0 - (OK), -1 - (Error)
        public static VadInst WebRtcVad_Create()
        {
            VadInst self = new VadInst()
            {

                //CHECK: WebRtcSpl_Init();

                init_flag = 0
            };

            return self;
        }


        // Initializes a VAD instance.
        //
        // - handle [i/o] : Instance that should be initialized.
        //
        // returns        : 0 - (OK),
        //                 -1 - (NULL pointer or Default mode could not be set).
        public static int WebRtcVad_Init(VadInst handle)
        {
            // Initialize the core VAD component.
            return VadCore.WebRtcVad_InitCore(handle);
        }



        // Sets the VAD operating mode. A more aggressive (higher mode) VAD is more
        // restrictive in reporting speech. Put in other words the probability of being
        // speech when the VAD returns 1 is increased with increasing mode. As a
        // consequence also the missed detection rate goes up.
        //
        // - handle [i/o] : VAD instance.
        // - mode   [i]   : Aggressiveness mode (0, 1, 2, or 3).
        //
        // returns        : 0 - (OK),
        //                 -1 - (NULL pointer, mode could not be set or the VAD instance
        //                       has not been initialized).
        public static int WebRtcVad_set_mode(VadInst handle, int mode)
        {
            ref VadInst self = ref handle;

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


        // Calculates a VAD decision for the |audio_frame|. For valid sampling rates
        // frame lengths, see the description of WebRtcVad_ValidRatesAndFrameLengths().
        //
        // - handle       [i/o] : VAD Instance. Needs to be initialized by
        //                        WebRtcVad_Init() before call.
        // - fs           [i]   : Sampling frequency (Hz): 8000, 16000, or 32000
        // - audio_frame  [i]   : Audio frame buffer.
        // - frame_length [i]   : Length of audio frame buffer in number of samples.
        //
        // returns              : 1 - (Active Voice),
        //                        0 - (Non-active Voice),
        //                       -1 - (Error)
        public static int WebRtcVad_Process(VadInst handle, int fs, Int16[] audio_frame, int frame_length)
        {
            int vad = -1;
            ref VadInst self = ref handle;

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


        // Checks for valid combinations of |rate| and |frame_length|. We support 10,
        // 20 and 30 ms frames and the rates 8000, 16000 and 32000 Hz.
        //
        // - rate         [i] : Sampling frequency (Hz).
        // - frame_length [i] : Speech frame buffer length in number of samples.
        //
        // returns            : 0 - (valid combination), -1 - (invalid combination)
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
