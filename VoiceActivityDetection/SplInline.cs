using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRtc.CommonAudio.SignalProcessing
{
    static class SplInline
    {
        public static Int16 WebRtcSpl_SatW32ToW16(Int32 value32)
        {
            Int16 out16 = (Int16)value32;

            if (value32 > 32767)
                out16 = 32767;
            else if (value32 < -32768)
                out16 = -32768;

            return out16;
        }


        public static  Int32 WebRtcSpl_AddSatW32(Int32 l_var1, Int32 l_var2)
        {
            Int32 l_sum;

            // Perform long addition
            l_sum = l_var1 + l_var2;

            if (l_var1 < 0)
            {  // Check for underflow.
                if ((l_var2 < 0) && (l_sum >= 0))
                {
                    l_sum = unchecked((Int32)0x80000000);
                }
            }
            else
            {  // Check for overflow.
                if ((l_var2 > 0) && (l_sum < 0))
                {
                    l_sum = (Int32)0x7FFFFFFF;
                }
            }

            return l_sum;
        }

        public static  Int32 WebRtcSpl_SubSatW32(Int32 l_var1, Int32 l_var2)
        {
            Int32 l_diff;

            // Perform subtraction.
            l_diff = l_var1 - l_var2;

            if (l_var1 < 0)
            {  // Check for underflow.
                if ((l_var2 > 0) && (l_diff > 0))
                {
                    l_diff = unchecked((Int32)0x80000000);
                }
            }
            else
            {  // Check for overflow.
                if ((l_var2 < 0) && (l_diff < 0))
                {
                    l_diff = (Int32)0x7FFFFFFF;
                }
            }

            return l_diff;
        }

        public static  Int16 WebRtcSpl_AddSatW16(Int16 a, Int16 b)
        {
            return WebRtcSpl_SatW32ToW16((Int32)a + (Int32)b);
        }

        public static  Int16 WebRtcSpl_SubSatW16(Int16 var1, Int16 var2)
        {
            return WebRtcSpl_SatW32ToW16((Int32)var1 - (Int32)var2);
        }


        public static  Int16 WebRtcSpl_GetSizeInBits(UInt32 n)
        {
            Int16 bits;

            if ((0xFFFF0000 & n) != 0)
            {
                bits = 16;
            }
            else
            {
                bits = 0;
            }
            if ((0x0000FF00 & (n >> bits)) != 0) bits += 8;
            if ((0x000000F0 & (n >> bits)) != 0) bits += 4;
            if ((0x0000000C & (n >> bits)) != 0) bits += 2;
            if ((0x00000002 & (n >> bits)) != 0) bits += 1;
            if ((0x00000001 & (n >> bits)) != 0) bits += 1;

            return bits;
        }
        

        public static  Int16 WebRtcSpl_NormW32(Int32 a)
        {
            Int16 zeros;

            if (a == 0)
            {
                return 0;
            }
            else if (a < 0)
            {
                a = ~a;
            }

            if ((0xFFFF8000 & a) == 0)
            {
                zeros = 16;
            }
            else
            {
                zeros = 0;
            }
            if ((0xFF800000 & (a << zeros)) == 0) zeros += 8;
            if ((0xF8000000 & (a << zeros)) == 0) zeros += 4;
            if ((0xE0000000 & (a << zeros)) == 0) zeros += 2;
            if ((0xC0000000 & (a << zeros)) == 0) zeros += 1;

            return zeros;
        }


        public static  Int16 WebRtcSpl_NormU32(UInt32 a)
        {
            Int16 zeros;

            if (a == 0) return 0;

            if ((0xFFFF0000 & a) == 0)
            {
                zeros = 16;
            }
            else
            {
                zeros = 0;
            }
            if ((0xFF000000 & (a << zeros)) == 0) zeros += 8;
            if ((0xF0000000 & (a << zeros)) == 0) zeros += 4;
            if ((0xC0000000 & (a << zeros)) == 0) zeros += 2;
            if ((0x80000000 & (a << zeros)) == 0) zeros += 1;

            return zeros;
        }

        public static  Int16 WebRtcSpl_NormW16(Int16 a)
        {
            Int16 zeros;

            if (a == 0)
            {
                return 0;
            }
            else if (a < 0)
            {
                a = (Int16)(~a);
            }

            if ((0xFF80 & a) == 0)
            {
                zeros = 8;
            }
            else
            {
                zeros = 0;
            }
            if ((0xF800 & (a << zeros)) == 0) zeros += 4;
            if ((0xE000 & (a << zeros)) == 0) zeros += 2;
            if ((0xC000 & (a << zeros)) == 0) zeros += 1;

            return zeros;
        }


        public static  Int32 WebRtc_MulAccumW16(Int16 a, Int16 b, Int32 c)
        {
            return (a * b + c);
        }
    }
}
