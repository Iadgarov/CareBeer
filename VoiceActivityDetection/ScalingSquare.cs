using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRtc.CommonAudio.SignalProcessing
{
    static class ScalingSquare
    {
        public static Int16 WebRtcSpl_GetScalingSquare(Int16[] in_vector, int in_vector_length, int times)
        {
            Int16 nbits = SplInline.WebRtcSpl_GetSizeInBits((uint)times);
            int i;
            Int16 smax = -1;
            Int16 sabs;
            ref Int16[] sptr = ref in_vector;
            Int16 t;
            int looptimes = in_vector_length;

            int idx = 0;
            for (i = looptimes; i > 0; i--)
            {
                sabs = (sptr[idx] > 0 ? sptr[idx++] : (Int16)(-sptr[idx++]));
                smax = (sabs > smax ? sabs : smax);
            }
            t = SplInline.WebRtcSpl_NormW32(Macros.WEBRTC_SPL_MUL(smax, smax));

            if (smax == 0)
            {
                return 0; // Since norm(0) returns 0
            }
            else
            {
                return (t > nbits) ? (Int16)0 : (Int16)(nbits - t);
            }
        }
    }
}
