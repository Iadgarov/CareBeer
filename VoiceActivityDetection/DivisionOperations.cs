using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRtc.CommonAudio.SignalProcessing
{
    static class DivisionOperations
    {

        public static UInt32 WebRtcSpl_DivU32U16(UInt32 num, UInt16 den)
        {
            // Guard against division with 0
            if (den != 0)
            {
                return (UInt32)(num / den);
            }
            else
            {
                return (UInt32)0xFFFFFFFF;
            }
        }


        public static  Int32 WebRtcSpl_DivW32W16(Int32 num, Int16 den)
        {
            // Guard against division with 0
            if (den != 0)
            {
                return (Int32)(num / den);
            }
            else
            {
                return (Int32)0x7FFFFFFF;
            }
        }


        public static Int16 WebRtcSpl_DivW32W16ResW16(Int32 num, Int16 den)
        {
            // Guard against division with 0
            if (den != 0)
            {
                return (Int16)(num / den);
            }
            else
            {
                return (Int16)0x7FFF;
            }
        }


        public static Int32 WebRtcSpl_DivResultInQ31(Int32 num, Int32 den)
        {
            Int32 L_num = num;
            Int32 L_den = den;
            Int32 div = 0;
            int k = 31;
            int change_sign = 0;

            if (num == 0)
                return 0;

            if (num < 0)
            {
                change_sign++;
                L_num = -num;
            }
            if (den < 0)
            {
                change_sign++;
                L_den = -den;
            }
            while (k-- > 0)
            {
                div <<= 1;
                L_num <<= 1;
                if (L_num >= L_den)
                {
                    L_num -= L_den;
                    div++;
                }
            }
            if (change_sign == 1)
            {
                div = -div;
            }
            return div;
        }


        public static Int32 WebRtcSpl_DivW32HiLow(Int32 num, Int16 den_hi, Int16 den_low)
        {
            Int16 approx, tmp_hi, tmp_low, num_hi, num_low;
            Int32 tmpW32;

            approx = (Int16)WebRtcSpl_DivW32W16((Int32)0x1FFFFFFF, den_hi);
            // result in Q14 (Note: 3FFFFFFF = 0.5 in Q30)

            // tmpW32 = 1/den = approx * (2.0 - den * approx) (in Q30)
            tmpW32 = (Macros.WEBRTC_SPL_MUL_16_16(den_hi, approx) << 1)
                    + ((Macros.WEBRTC_SPL_MUL_16_16(den_low, approx) >> 15) << 1);
            // tmpW32 = den * approx

            tmpW32 = (Int32)0x7fffffffL - tmpW32; // result in Q30 (tmpW32 = 2.0-(den*approx))

            // Store tmpW32 in hi and low format
            tmp_hi = (Int16)(tmpW32 >> 16);
            tmp_low = (Int16)((tmpW32 - ((Int32)tmp_hi << 16)) >> 1);

            // tmpW32 = 1/den in Q29
            tmpW32 = ((Macros.WEBRTC_SPL_MUL_16_16(tmp_hi, approx) + (Macros.WEBRTC_SPL_MUL_16_16(tmp_low, approx)
                    >> 15)) << 1);

            // 1/den in hi and low format
            tmp_hi = (Int16)(tmpW32 >> 16);
            tmp_low = (Int16)((tmpW32 - ((Int32)tmp_hi << 16)) >> 1);

            // Store num in hi and low format
            num_hi = (Int16)(num >> 16);
            num_low = (Int16)((num - ((Int32)num_hi << 16)) >> 1);

            // num * (1/den) by 32 bit multiplication (result in Q28)

            tmpW32 = (Macros.WEBRTC_SPL_MUL_16_16(num_hi, tmp_hi) + (Macros.WEBRTC_SPL_MUL_16_16(num_hi, tmp_low)
                    >> 15) + (Macros.WEBRTC_SPL_MUL_16_16(num_low, tmp_hi) >> 15));

            // Put result in Q31 (convert from Q28)
            tmpW32 = Macros.WEBRTC_SPL_LSHIFT_W32(tmpW32, 3);

            return tmpW32;
        }
    }
}
