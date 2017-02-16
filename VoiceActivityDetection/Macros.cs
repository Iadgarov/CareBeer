using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRtc.CommonAudio.SignalProcessing
{
    static class Macros
    {
        // Macros specific for the fixed point implementation
        public const int WEBRTC_SPL_WORD16_MAX = 32767;
        public const int WEBRTC_SPL_WORD16_MIN = -32768;
        public const int WEBRTC_SPL_WORD32_MAX = 0x7fffffff;
        public const uint WEBRTC_SPL_WORD32_MIN = 0x80000000;
        public const int WEBRTC_SPL_MAX_LPC_ORDER = 14;

        public static int WEBRTC_SPL_MIN(int A, int B) { return (A < B ? A : B); } // Get min value
        public static int WEBRTC_SPL_MAX(int A, int B) { return (A > B ? A : B); } // Get max value
                                                                                   // TODO(kma/bjorn): For the next two macros, investigate how to correct the code
                                                                                   // for inputs of a = WEBRTC_SPL_WORD16_MIN or WEBRTC_SPL_WORD32_MIN.
        public static int WEBRTC_SPL_ABS_W16(Int16 a)
        {
            return (((Int16)a >= 0) ? ((Int16)a) : -((Int16)a));
        }
        public static int WEBRTC_SPL_ABS_W32(Int32 a) { return (((Int32)a >= 0) ? ((Int32)a) : -((Int32)a)); }


        public static int WEBRTC_SPL_MUL(int a, int b) { return ((Int32)((Int32)(a) * (Int32)(b))); }

        public static uint WEBRTC_SPL_UMUL(uint a, uint b) { return (uint)((int)a * (int)b); }

        public static uint WEBRTC_SPL_UMUL_32_16(UInt32 a, UInt16 b) { return (uint)((Int32)a * b); }

        public static int WEBRTC_SPL_MUL_16_U16(Int16 a, UInt16 b) { return a * b; }

        // CHECK !!!
        //# ifndef WEBRTC_ARCH_ARM_V7
        // For ARMv7 platforms, these are inline functions in spl_inl_armv7.h
        //# ifndef MIPS32_LE
        // For MIPS platforms, these are inline functions in spl_inl_mips.h
        public static int WEBRTC_SPL_MUL_16_16(Int16 a, Int16 b) { return a * b; }

        public static int WEBRTC_SPL_MUL_16_32_RSFT16(Int16 a, Int32 b)
        {
            return (WEBRTC_SPL_MUL_16_16(a, (Int16)(b >> 16))
                + ((WEBRTC_SPL_MUL_16_16(a, (Int16)((b & 0xffff) >> 1)) + 0x4000) >> 15));
        }

        //#endif
        //#endif

        public static int WEBRTC_SPL_MUL_16_32_RSFT11(short a, int b)
        {
            return ((WEBRTC_SPL_MUL_16_16(a, (short)(b >> 16)) << 5)
                + (((WEBRTC_SPL_MUL_16_U16(a, (UInt16)(b)) >> 1) + 0x0200) >> 10));
        }

        public static int WEBRTC_SPL_MUL_16_32_RSFT14(Int16 a, Int32 b)
        {
            return ((WEBRTC_SPL_MUL_16_16(a, (Int16)(b >> 16)) << 2)
                + (((WEBRTC_SPL_MUL_16_U16(a, (UInt16)(b)) >> 1) + 0x1000) >> 13));
        }

        public static int WEBRTC_SPL_MUL_16_32_RSFT15(Int16 a, Int32 b)
        {
            return ((WEBRTC_SPL_MUL_16_16(a, (Int16)(b >> 16)) << 1)
                + (((WEBRTC_SPL_MUL_16_U16(a, (UInt16)(b)) >> 1) + 0x2000) >> 14));
        }


        public static int WEBRTC_SPL_MUL_16_16_RSFT(Int16 a, Int16 b, Int16 c)
        {
            return (WEBRTC_SPL_MUL_16_16(a, b) >> (c));
        }

        public static int WEBRTC_SPL_MUL_16_16_RSFT_WITH_ROUND(Int16 a, Int16 b, Int16 c)
        {
            return ((WEBRTC_SPL_MUL_16_16(a, b) + (1 << ((c) - 1))) >> (c));
        }


        // C + the 32 most significant bits of A * B
        public static int WEBRTC_SPL_SCALEDIFF32(int A, int B, int C)
        {
            return (int)(C + (B >> 16) * A + (((UInt32)(0x0000FFFF & B) * A) >> 16));
        }


        public static int WEBRTC_SPL_SAT(int a, int b, int c) { return (b > a ? a : b < c ? c : b); }

        // Shifting with negative numbers allowed
        // Positive means left shift
        public static int WEBRTC_SPL_SHIFT_W32(int x, int c) { return (((c) >= 0) ? ((x) << (c)) : ((x) >> (-(c)))); }


        // Shifting with negative numbers not allowed
        // We cannot do casting here due to signed/unsigned problem
        public static int WEBRTC_SPL_LSHIFT_W32(int x, int c) { return ((x) << (c)); }

        public static uint WEBRTC_SPL_RSHIFT_U32(uint x, uint c) { return (uint)((int)x >> (int)c); }

        public static int WEBRTC_SPL_RAND(short a) { return ((Int16)(WEBRTC_SPL_MUL_16_16_RSFT((a), 18816, 7) & 0x00007fff)); }


    }
}
