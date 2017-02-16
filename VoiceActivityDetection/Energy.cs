using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRtc.CommonAudio.SignalProcessing
{
    static class Energy
    {
        public static Int32 WebRtcSpl_Energy(Int16[] vector, int vector_length, out int scale_factor)
        {
            Int32 en = 0;
            int i;
            int scaling = ScalingSquare.WebRtcSpl_GetScalingSquare(vector, vector_length, vector_length);
            int looptimes = vector_length;
            Int16[] vectorptr = vector;

            for (i = 0; i < looptimes; i++)
            {
                en += Macros.WEBRTC_SPL_MUL_16_16_RSFT(vectorptr[i], vectorptr[i], (short)scaling);
                //vectorptr++;
            }
            scale_factor = scaling;

            return en;
        }
    }
}
