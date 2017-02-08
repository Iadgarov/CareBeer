using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRtc.CommonAudio.SignalProcessing;

namespace WebRtc.CommonAudio.Vad
{
    static class VadFilterBank
    {
        // Constants used in LogOfEnergy().
        const Int16 kLogConst = 24660;  // 160*log10(2) in Q9.
        const Int16 kLogEnergyIntPart = 14336;  // 14 in Q10

        // Coefficients used by HighPassFilter, Q14.
        static readonly Int16[] kHpZeroCoefs = { 6631, -13262, 6631 };
        static readonly Int16[] kHpPoleCoefs = { 16384, -7756, 5620 };

        // Allpass filter coefficients, upper and lower, in Q15.
        // Upper: 0.64, Lower: 0.17
        static readonly Int16[] kAllPassCoefsQ15 = { 20972, 5571 };

        // Adjustment for division with two in SplitFilter.
        static readonly Int16[] kOffsetVector = { 368, 368, 272, 176, 176, 176 };

        // High pass filtering, with a cut-off frequency at 80 Hz, if the |data_in| is
        // sampled at 500 Hz.
        //
        // - data_in      [i]   : Input audio data sampled at 500 Hz.
        // - data_length  [i]   : Length of input and output data.
        // - filter_state [i/o] : State of the filter.
        // - data_out     [o]   : Output audio data in the frequency interval
        //                        80 - 250 Hz.
        static void HighPassFilter(Int16[] data_in, int data_length, Int16[] filter_state, Int16[] data_out)
        {
            int i;
            ref Int16[] in_ptr = ref data_in;
            ref Int16[] out_ptr = ref data_out;
            Int32 tmp32 = 0;
            int in_idx = 0, out_idx = 0;


            // The sum of the absolute values of the impulse response:
            // The zero/pole-filter has a max amplification of a single sample of: 1.4546
            // Impulse response: 0.4047 -0.6179 -0.0266  0.1993  0.1035  -0.0194
            // The all-zero section has a max amplification of a single sample of: 1.6189
            // Impulse response: 0.4047 -0.8094  0.4047  0       0        0
            // The all-pole section has a max amplification of a single sample of: 1.9931
            // Impulse response: 1.0000  0.4734 -0.1189 -0.2187 -0.0627   0.04532

            for (i = 0; i < data_length; i++)
            {
                // All-zero section (filter coefficients in Q14).
                tmp32 = kHpZeroCoefs[0] * in_ptr[in_idx];
                tmp32 += kHpZeroCoefs[1] * filter_state[0];
                tmp32 += kHpZeroCoefs[2] * filter_state[1];
                filter_state[1] = filter_state[0];
                filter_state[0] = in_ptr[in_idx];
                in_idx++;

                // All-pole section (filter coefficients in Q14).
                tmp32 -= kHpPoleCoefs[1] * filter_state[2];
                tmp32 -= kHpPoleCoefs[2] * filter_state[3];
                filter_state[3] = filter_state[2];
                filter_state[2] = (Int16)(tmp32 >> 14);
                out_ptr[out_idx] = filter_state[2];
                out_idx++;
            }
        }



        // All pass filtering of |data_in|, used before splitting the signal into two
        // frequency bands (low pass vs high pass).
        // Note that |data_in| and |data_out| can NOT correspond to the same address.
        //
        // - data_in            [i]   : Input audio signal given in Q0.
        // - data_length        [i]   : Length of input and output data.
        // - filter_coefficient [i]   : Given in Q15.
        // - filter_state       [i/o] : State of the filter given in Q(-1).
        // - data_out           [o]   : Output audio signal given in Q(-1).
        static void AllPassFilter(Int16[] data_in, int in_start, int data_length,
                                  Int16 filter_coefficient, ref Int16 filter_state,
                                  Int16[] data_out)
        {
            // The filter can only cause overflow (in the w16 output variable)
            // if more than 4 consecutive input numbers are of maximum value and
            // has the the same sign as the impulse responses first taps.
            // First 6 taps of the impulse response:
            // 0.6399 0.5905 -0.3779 0.2418 -0.1547 0.0990

            int i;
            Int16 tmp16 = 0;
            Int32 tmp32 = 0;
            Int32 state32 = filter_state << 16;  // Q15

            int in_idx = in_start, out_idx = 0;
            for (i = 0; i < data_length; i++)
            {
                tmp32 = state32 + Macros.WEBRTC_SPL_MUL_16_16(filter_coefficient, data_in[in_idx]);
                tmp16 = (Int16)(tmp32 >> 16);  // Q(-1)
                data_out[out_idx++] = tmp16;
                state32 = (((Int32)(data_in[in_idx])) << 14); // Q14
                state32 -= Macros.WEBRTC_SPL_MUL_16_16(filter_coefficient, tmp16);  // Q14
                state32 <<= 1;  // Q15.
                in_idx += 2;
            }

            filter_state = (Int16)(state32 >> 16);  // Q(-1)
        }



        // Splits |data_in| into |hp_data_out| and |lp_data_out| corresponding to
        // an upper (high pass) part and a lower (low pass) part respectively.
        //
        // - data_in      [i]   : Input audio data to be split into two frequency bands.
        // - data_length  [i]   : Length of |data_in|.
        // - upper_state  [i/o] : State of the upper filter, given in Q(-1).
        // - lower_state  [i/o] : State of the lower filter, given in Q(-1).
        // - hp_data_out  [o]   : Output audio data of the upper half of the spectrum.
        //                        The length is |data_length| / 2.
        // - lp_data_out  [o]   : Output audio data of the lower half of the spectrum.
        //                        The length is |data_length| / 2.
        static void SplitFilter(Int16[] data_in, int data_length,
                                ref Int16 upper_state, ref Int16 lower_state,
                                Int16[] hp_data_out, Int16[] lp_data_out)
        {
            int i;
            int half_length = data_length >> 1;  // Downsampling by 2.
            Int16 tmp_out;

            // All-pass filtering upper branch.
            AllPassFilter(data_in, 0, half_length, kAllPassCoefsQ15[0], ref upper_state,
                          hp_data_out);

            // All-pass filtering lower branch.
            AllPassFilter(data_in, 1, half_length, kAllPassCoefsQ15[1], ref lower_state,
                          lp_data_out);

            // Make LP and HP signals.
            int hp_idx = 0, lp_idx = 0;
            for (i = 0; i < half_length; i++)
            {
                tmp_out = hp_data_out[hp_idx];
                hp_data_out[hp_idx++] -= lp_data_out[lp_idx];
                lp_data_out[lp_idx++] += tmp_out;
            }
        }



        // Calculates the energy of |data_in| in dB, and also updates an overall
        // |total_energy| if necessary.
        //
        // - data_in      [i]   : Input audio data for energy calculation.
        // - data_length  [i]   : Length of input data.
        // - offset       [i]   : Offset value added to |log_energy|.
        // - total_energy [i/o] : An external energy updated with the energy of
        //                        |data_in|.
        //                        NOTE: |total_energy| is only updated if
        //                        |total_energy| <= |kMinEnergy|.
        // - log_energy   [o]   : 10 * log10("energy of |data_in|") given in Q4.
        static void LogOfEnergy(Int16[] data_in, int data_length,
                                Int16 offset, ref Int16 total_energy,
                                out Int16 log_energy)
        {
            // |tot_rshifts| accumulates the number of right shifts performed on |energy|.
            int tot_rshifts = 0;
            // The |energy| will be normalized to 15 bits. We use unsigned integer because
            // we eventually will mask out the fractional part.
            UInt32 energy = 0;

            Debug.Assert(data_in != null);
            Debug.Assert(data_length > 0);

            energy = (UInt32)Energy.WebRtcSpl_Energy(data_in, data_length,
                                                 out tot_rshifts);

            if (energy != 0)
            {
                // By construction, normalizing to 15 bits is equivalent with 17 leading
                // zeros of an unsigned 32 bit value.
                int normalizing_rshifts = 17 - SplInline.WebRtcSpl_NormU32(energy);
                // In a 15 bit representation the leading bit is 2^14. log2(2^14) in Q10 is
                // (14 << 10), which is what we initialize |log2_energy| with. For a more
                // detailed derivations, see below.
                Int16 log2_energy = kLogEnergyIntPart;

                tot_rshifts += normalizing_rshifts;
                // Normalize |energy| to 15 bits.
                // |tot_rshifts| is now the total number of right shifts performed on
                // |energy| after normalization. This means that |energy| is in
                // Q(-tot_rshifts).
                if (normalizing_rshifts < 0)
                {
                    energy <<= -normalizing_rshifts;
                }
                else
                {
                    energy >>= normalizing_rshifts;
                }

                // Calculate the energy of |data_in| in dB, in Q4.
                //
                // 10 * log10("true energy") in Q4 = 2^4 * 10 * log10("true energy") =
                // 160 * log10(|energy| * 2^|tot_rshifts|) =
                // 160 * log10(2) * log2(|energy| * 2^|tot_rshifts|) =
                // 160 * log10(2) * (log2(|energy|) + log2(2^|tot_rshifts|)) =
                // (160 * log10(2)) * (log2(|energy|) + |tot_rshifts|) =
                // |kLogConst| * (|log2_energy| + |tot_rshifts|)
                //
                // We know by construction that |energy| is normalized to 15 bits. Hence,
                // |energy| = 2^14 + frac_Q15, where frac_Q15 is a fractional part in Q15.
                // Further, we'd like |log2_energy| in Q10
                // log2(|energy|) in Q10 = 2^10 * log2(2^14 + frac_Q15) =
                // 2^10 * log2(2^14 * (1 + frac_Q15 * 2^-14)) =
                // 2^10 * (14 + log2(1 + frac_Q15 * 2^-14)) ~=
                // (14 << 10) + 2^10 * (frac_Q15 * 2^-14) =
                // (14 << 10) + (frac_Q15 * 2^-4) = (14 << 10) + (frac_Q15 >> 4)
                //
                // Note that frac_Q15 = (|energy| & 0x00003FFF)

                // Calculate and add the fractional part to |log2_energy|.
                log2_energy += (Int16)((energy & 0x00003FFF) >> 4);

                // |kLogConst| is in Q9, |log2_energy| in Q10 and |tot_rshifts| in Q0.
                // Note that we in our derivation above have accounted for an output in Q4.
                log_energy = (Int16)(Macros.WEBRTC_SPL_MUL_16_16_RSFT(
                    kLogConst, log2_energy, 19) +
                    Macros.WEBRTC_SPL_MUL_16_16_RSFT((short)tot_rshifts, kLogConst, 9));

                if (log_energy < 0)
                {
                    log_energy = 0;
                }
            }
            else
            {
                log_energy = offset;
                return;
            }

            log_energy += offset;

            // Update the approximate |total_energy| with the energy of |data_in|, if
            // |total_energy| has not exceeded |kMinEnergy|. |total_energy| is used as an
            // energy indicator in WebRtcVad_GmmProbability() in vad_core.c.
            if (total_energy <= VadCore.kMinEnergy)
            {
                if (tot_rshifts >= 0)
                {
                    // We know by construction that the |energy| > |kMinEnergy| in Q0, so add
                    // an arbitrary value such that |total_energy| exceeds |kMinEnergy|.
                    total_energy += VadCore.kMinEnergy + 1;
                }
                else
                {
                    // By construction |energy| is represented by 15 bits, hence any number of
                    // right shifted |energy| will fit in an Int16. In addition, adding the
                    // value to |total_energy| is wrap around safe as long as
                    // |kMinEnergy| < 8192.
                    total_energy += (Int16)(energy >> -tot_rshifts);  // Q0.
                }
            }
        }



        public static Int16 WebRtcVad_CalculateFeatures(VadInstT self, Int16[] data_in, int data_length, Int16[] features)
        {
            Int16 total_energy = 0;
            // We expect |data_length| to be 80, 160 or 240 samples, which corresponds to
            // 10, 20 or 30 ms in 8 kHz. Therefore, the intermediate downsampled data will
            // have at most 120 samples after the first split and at most 60 samples after
            // the second split.
            Int16[] hp_120 = new Int16[120], lp_120 = new Int16[120];
            Int16[] hp_60 = new Int16[60], lp_60 = new Int16[60];
            int half_data_length = data_length >> 1;
            int length = half_data_length;  // |data_length| / 2, corresponds to
                                            // bandwidth = 2000 Hz after downsampling.

            // Initialize variables for the first SplitFilter().
            int frequency_band = 0;
            ref Int16[] in_ptr = ref data_in;  // [0 - 4000] Hz.
            ref Int16[] hp_out_ptr = ref hp_120;  // [2000 - 4000] Hz.
            ref Int16[] lp_out_ptr = ref lp_120;  // [0 - 2000] Hz.

            Debug.Assert(data_length >= 0);
            Debug.Assert(data_length <= 240);
            Debug.Assert(4 < VadCore.kNumChannels - 1);  // Checking maximum |frequency_band|.

            // Split at 2000 Hz and downsample.
            SplitFilter(in_ptr, data_length, ref self.upper_state[frequency_band],
                        ref self.lower_state[frequency_band], hp_out_ptr, lp_out_ptr);

            // For the upper band (2000 Hz - 4000 Hz) split at 3000 Hz and downsample.
            frequency_band = 1;
            in_ptr = hp_120;  // [2000 - 4000] Hz.
            hp_out_ptr = hp_60;  // [3000 - 4000] Hz.
            lp_out_ptr = lp_60;  // [2000 - 3000] Hz.
            SplitFilter(in_ptr, length, ref self.upper_state[frequency_band],
                        ref self.lower_state[frequency_band], hp_out_ptr, lp_out_ptr);

            // Energy in 3000 Hz - 4000 Hz.
            length >>= 1;  // |data_length| / 4 <=> bandwidth = 1000 Hz.

            LogOfEnergy(hp_60, length, kOffsetVector[5], ref total_energy, out features[5]);

            // Energy in 2000 Hz - 3000 Hz.
            LogOfEnergy(lp_60, length, kOffsetVector[4], ref total_energy, out features[4]);

            // For the lower band (0 Hz - 2000 Hz) split at 1000 Hz and downsample.
            frequency_band = 2;
            in_ptr = lp_120;  // [0 - 2000] Hz.
            hp_out_ptr = hp_60;  // [1000 - 2000] Hz.
            lp_out_ptr = lp_60;  // [0 - 1000] Hz.
            length = half_data_length;  // |data_length| / 2 <=> bandwidth = 2000 Hz.
            SplitFilter(in_ptr, length, ref self.upper_state[frequency_band],
                        ref self.lower_state[frequency_band], hp_out_ptr, lp_out_ptr);

            // Energy in 1000 Hz - 2000 Hz.
            length >>= 1;  // |data_length| / 4 <=> bandwidth = 1000 Hz.
            LogOfEnergy(hp_60, length, kOffsetVector[3], ref total_energy, out features[3]);

            // For the lower band (0 Hz - 1000 Hz) split at 500 Hz and downsample.
            frequency_band = 3;
            in_ptr = lp_60;  // [0 - 1000] Hz.
            hp_out_ptr = hp_120;  // [500 - 1000] Hz.
            lp_out_ptr = lp_120;  // [0 - 500] Hz.
            SplitFilter(in_ptr, length, ref self.upper_state[frequency_band],
                        ref self.lower_state[frequency_band], hp_out_ptr, lp_out_ptr);

            // Energy in 500 Hz - 1000 Hz.
            length >>= 1;  // |data_length| / 8 <=> bandwidth = 500 Hz.
            LogOfEnergy(hp_120, length, kOffsetVector[2], ref total_energy, out features[2]);

            // For the lower band (0 Hz - 500 Hz) split at 250 Hz and downsample.
            frequency_band = 4;
            in_ptr = lp_120;  // [0 - 500] Hz.
            hp_out_ptr = hp_60;  // [250 - 500] Hz.
            lp_out_ptr = lp_60;  // [0 - 250] Hz.
            SplitFilter(in_ptr, length, ref self.upper_state[frequency_band],
                        ref self.lower_state[frequency_band], hp_out_ptr, lp_out_ptr);

            // Energy in 250 Hz - 500 Hz.
            length >>= 1;  // |data_length| / 16 <=> bandwidth = 250 Hz.
            LogOfEnergy(hp_60, length, kOffsetVector[1], ref total_energy, out features[1]);

            // Remove 0 Hz - 80 Hz, by high pass filtering the lower band.
            HighPassFilter(lp_60, length, self.hp_filter_state, hp_120);

            // Energy in 80 Hz - 250 Hz.
            LogOfEnergy(hp_120, length, kOffsetVector[0], ref total_energy, out features[0]);

            return total_energy;
        }
    }
}
