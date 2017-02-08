using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRtc.CommonAudio.SignalProcessing;

namespace WebRtc.CommonAudio.Vad
{

    public class VadInstT
    {

        public int vad;
        public Int32[] downsampling_filter_states = new Int32[4];
        public WebRtcSpl_State48khzTo8khz state_48_to_8;
        public Int16[] noise_means = new Int16[VadCore.kTableSize];
        public Int16[] speech_means = new Int16[VadCore.kTableSize];
        public Int16[] noise_stds = new Int16[VadCore.kTableSize];
        public Int16[] speech_stds = new Int16[VadCore.kTableSize];
        // TODO(bjornv): Change to |frame_count|.
        public Int32 frame_counter;
        public Int16 over_hang; // Over Hang
        public Int16 num_of_speech;
        // TODO(bjornv): Change to |age_vector|.
        public Int16[] index_vector = new Int16[16 * VadCore.kNumChannels];
        public Int16[] low_value_vector = new Int16[16 * VadCore.kNumChannels];
        // TODO(bjornv): Change to |median|.
        public Int16[] mean_value = new Int16[VadCore.kNumChannels];
        public Int16[] upper_state = new Int16[5];
        public Int16[] lower_state = new Int16[5];
        public Int16[] hp_filter_state = new Int16[4];
        public Int16[] over_hang_max_1 = new Int16[3];
        public Int16[] over_hang_max_2 = new Int16[3];
        public Int16[] individual = new Int16[3];
        public Int16[] total = new Int16[3];

        public int init_flag;

    }


    public static class VadCore
    {
        public const int kNumChannels = 6;  // Number of frequency bands (named channels).
        public const int kNumGaussians = 2;  // Number of Gaussians per channel in the GMM.
        public const int kTableSize = kNumChannels * kNumGaussians;
        public const int kMinEnergy = 10;  // Minimum energy required to trigger audio signal.

        // Spectrum Weighting
        static readonly Int16[] kSpectrumWeight = new Int16[kNumChannels] { 6, 8, 10, 12, 14, 16 };
        static readonly Int16 kNoiseUpdateConst = 655; // Q15
        static readonly Int16 kSpeechUpdateConst = 6554; // Q15
        static readonly Int16 kBackEta = 154; // Q8
                                              // Minimum difference between the two models, Q5
        static readonly Int16[] kMinimumDifference = new Int16[kNumChannels] { 544, 544, 576, 576, 576, 576 };
        // Upper limit of mean value for speech model, Q7
        static readonly Int16[] kMaximumSpeech = new Int16[kNumChannels] { 11392, 11392, 11520, 11520, 11520, 11520 };
        // Minimum value for mean value
        static readonly Int16[] kMinimumMean = new Int16[kNumGaussians] { 640, 768 };
        // Upper limit of mean value for noise model, Q7
        static readonly Int16[] kMaximumNoise = new Int16[kNumChannels] { 9216, 9088, 8960, 8832, 8704, 8576 };
        // Start values for the Gaussian models, Q7
        // Weights for the two Gaussians for the six channels (noise)
        static readonly Int16[] kNoiseDataWeights = new Int16[kTableSize] { 34, 62, 72, 66, 53, 25, 94, 66, 56, 62, 75, 103 };
        // Weights for the two Gaussians for the six channels (speech)
        static readonly Int16[] kSpeechDataWeights = new Int16[kTableSize] { 48, 82, 45, 87, 50, 47, 80, 46, 83, 41, 78, 81 };
        // Means for the two Gaussians for the six channels (noise)
        static readonly Int16[] kNoiseDataMeans = new Int16[kTableSize] { 6738, 4892, 7065, 6715, 6771, 3369, 7646, 3863, 7820, 7266, 5020, 4362 };
        // Means for the two Gaussians for the six channels (speech)
        static readonly Int16[] kSpeechDataMeans = new Int16[kTableSize] { 8306, 10085, 10078, 11823, 11843, 6309, 9473, 9571, 10879, 7581, 8180, 7483 };
        // Stds for the two Gaussians for the six channels (noise)
        static readonly Int16[] kNoiseDataStds = new Int16[kTableSize] { 378, 1064, 493, 582, 688, 593, 474, 697, 475, 688, 421, 455 };
        // Stds for the two Gaussians for the six channels (speech)
        static readonly Int16[] kSpeechDataStds = new Int16[kTableSize] { 555, 505, 567, 524, 585, 1231, 509, 828, 492, 1540, 1079, 850 };

        // Constants used in GmmProbability().
        //
        // Maximum number of counted speech (VAD = 1) frames in a row.
        static readonly Int16 kMaxSpeechFrames = 6;
        // Minimum standard deviation for both speech and noise.
        static readonly Int16 kMinStd = 384;

        // Constants in WebRtcVad_InitCore().
        // Default aggressiveness mode.
        static readonly short kDefaultMode = 0;
        static readonly int kInitCheck = 42;

        // Constants used in WebRtcVad_set_mode_core().
        //
        // Thresholds for different frame lengths (10 ms, 20 ms and 30 ms).
        //
        // Mode 0, Quality.
        static readonly Int16[] kOverHangMax1Q = { 8, 4, 3 };
        static readonly Int16[] kOverHangMax2Q = { 14, 7, 5 };
        static readonly Int16[] kLocalThresholdQ = { 24, 21, 24 };
        static readonly Int16[] kGlobalThresholdQ = { 57, 48, 57 };
        // Mode 1, Low bitrate.
        static readonly Int16[] kOverHangMax1LBR = { 8, 4, 3 };
        static readonly Int16[] kOverHangMax2LBR = { 14, 7, 5 };
        static readonly Int16[] kLocalThresholdLBR = { 37, 32, 37 };
        static readonly Int16[] kGlobalThresholdLBR = { 100, 80, 100 };
        // Mode 2, Aggressive.
        static readonly Int16[] kOverHangMax1AGG = { 6, 3, 2 };
        static readonly Int16[] kOverHangMax2AGG = { 9, 5, 3 };
        static readonly Int16[] kLocalThresholdAGG = { 82, 78, 82 };
        static readonly Int16[] kGlobalThresholdAGG = { 285, 260, 285 };
        // Mode 3, Very aggressive.
        static readonly Int16[] kOverHangMax1VAG = { 6, 3, 2 };
        static readonly Int16[] kOverHangMax2VAG = { 9, 5, 3 };
        static readonly Int16[] kLocalThresholdVAG = { 94, 94, 94 };
        static readonly Int16[] kGlobalThresholdVAG = { 1100, 1050, 1100 };



        // Calculates the weighted average w.r.t. number of Gaussians. The |data| are
        // updated with an |offset| before averaging.
        //
        // - data     [i/o] : Data to average.
        // - offset   [i]   : An offset added to |data|.
        // - weights  [i]   : Weights used for averaging.
        // - start    [i]   : start index from which to average
        //
        // returns          : The weighted average.
        static Int32 WeightedAverage(Int16[] data, Int16 offset, Int16[] weights, int start)
        {
            Int32 weighted_average = 0;

            for (int k = 0; k < kNumGaussians; k++)
            {
                data[start + k * kNumChannels] += offset;
                weighted_average += data[start + k * kNumChannels] * weights[start + k * kNumChannels];
            }
            return weighted_average;
        }



        // Calculates the probabilities for both speech and background noise using
        // Gaussian Mixture Models (GMM). A hypothesis-test is performed to decide which
        // type of signal is most probable.
        //
        // - self           [i/o] : Pointer to VAD instance
        // - features       [i]   : Feature vector of length |kNumChannels|
        //                          = log10(energy in frequency band)
        // - total_power    [i]   : Total power in audio frame.
        // - frame_length   [i]   : Number of input samples
        //
        // - returns              : the VAD decision (0 - noise, 1 - speech).
        static Int16 GmmProbability(VadInstT self, Int16[] features, Int16 total_power, int frame_length)
        {
            int channel, k;
            Int16 feature_minimum;
            Int16 h0, h1;
            Int16 log_likelihood_ratio;
            Int16 vadflag = 0;
            Int16 shifts_h0, shifts_h1;
            Int16 tmp_s16, tmp1_s16, tmp2_s16;
            Int16 diff;
            int gaussian;
            Int16 nmk, nmk2, nmk3, smk, smk2, nsk, ssk;
            Int16 delt, ndelt;
            Int16 maxspe, maxmu;
            Int16[] deltaN = new Int16[kTableSize];
            Int16[] deltaS = new Int16[kTableSize];
            Int16[] ngprvec = new Int16[kTableSize];  // Conditional probability = 0.
            Int16[] sgprvec = new Int16[kTableSize];  // Conditional probability = 0.
            Int32 h0_test, h1_test;
            Int32 tmp1_s32, tmp2_s32;
            Int32 sum_log_likelihood_ratios = 0;
            Int32 noise_global_mean, speech_global_mean;
            Int32[] noise_probability = new Int32[kNumGaussians];
            Int32[] speech_probability = new Int32[kNumGaussians];
            Int16 overhead1, overhead2, individualTest, totalTest;

            // Set various thresholds based on frame lengths (80, 160 or 240 samples).
            if (frame_length == 80)
            {
                overhead1 = self.over_hang_max_1[0];
                overhead2 = self.over_hang_max_2[0];
                individualTest = self.individual[0];
                totalTest = self.total[0];
            }
            else if (frame_length == 160)
            {
                overhead1 = self.over_hang_max_1[1];
                overhead2 = self.over_hang_max_2[1];
                individualTest = self.individual[1];
                totalTest = self.total[1];
            }
            else
            {
                overhead1 = self.over_hang_max_1[2];
                overhead2 = self.over_hang_max_2[2];
                individualTest = self.individual[2];
                totalTest = self.total[2];
            }

            if (total_power > kMinEnergy)
            {
                // The signal power of current frame is large enough for processing. The
                // processing consists of two parts:
                // 1) Calculating the likelihood of speech and thereby a VAD decision.
                // 2) Updating the underlying model, w.r.t., the decision made.

                // The detection scheme is an LRT with hypothesis
                // H0: Noise
                // H1: Speech
                //
                // We combine a global LRT with local tests, for each frequency sub-band,
                // here defined as |channel|.
                for (channel = 0; channel < kNumChannels; channel++)
                {
                    // For each channel we model the probability with a GMM consisting of
                    // |kNumGaussians|, with different means and standard deviations depending
                    // on H0 or H1.
                    h0_test = 0;
                    h1_test = 0;
                    for (k = 0; k < kNumGaussians; k++)
                    {
                        gaussian = channel + k * kNumChannels;
                        // Probability under H0, that is, probability of frame being noise.
                        // Value given in Q27 = Q7 * Q20.
                        tmp1_s32 = VadGmm.WebRtcVad_GaussianProbability(features[channel],
                                                                 self.noise_means[gaussian],
                                                                 self.noise_stds[gaussian],
                                                                 ref deltaN[gaussian]);
                        noise_probability[k] = kNoiseDataWeights[gaussian] * tmp1_s32;
                        h0_test += noise_probability[k];  // Q27

                        // Probability under H1, that is, probability of frame being speech.
                        // Value given in Q27 = Q7 * Q20.
                        tmp1_s32 = VadGmm.WebRtcVad_GaussianProbability(features[channel],
                                                                 self.speech_means[gaussian],
                                                                 self.speech_stds[gaussian],
                                                                 ref deltaS[gaussian]);
                        speech_probability[k] = kSpeechDataWeights[gaussian] * tmp1_s32;
                        h1_test += speech_probability[k];  // Q27
                    }

                    // Calculate the log likelihood ratio: log2(Pr{X|H1} / Pr{X|H1}).
                    // Approximation:
                    // log2(Pr{X|H1} / Pr{X|H1}) = log2(Pr{X|H1}*2^Q) - log2(Pr{X|H1}*2^Q)
                    //                           = log2(h1_test) - log2(h0_test)
                    //                           = log2(2^(31-shifts_h1)*(1+b1))
                    //                             - log2(2^(31-shifts_h0)*(1+b0))
                    //                           = shifts_h0 - shifts_h1
                    //                             + log2(1+b1) - log2(1+b0)
                    //                          ~= shifts_h0 - shifts_h1
                    //
                    // Note that b0 and b1 are values less than 1, hence, 0 <= log2(1+b0) < 1.
                    // Further, b0 and b1 are independent and on the average the two terms
                    // cancel.
                    shifts_h0 = SplInline.WebRtcSpl_NormW32(h0_test);
                    shifts_h1 = SplInline.WebRtcSpl_NormW32(h1_test);
                    if (h0_test == 0)
                    {
                        shifts_h0 = 31;
                    }
                    if (h1_test == 0)
                    {
                        shifts_h1 = 31;
                    }
                    log_likelihood_ratio = (Int16)(shifts_h0 - shifts_h1);

                    // Update |sum_log_likelihood_ratios| with spectrum weighting. This is
                    // used for the global VAD decision.
                    sum_log_likelihood_ratios +=
                        (Int32)(log_likelihood_ratio * kSpectrumWeight[channel]);

                    // Local VAD decision.
                    if ((log_likelihood_ratio << 2) > individualTest)
                    {
                        vadflag = 1;
                    }

                    // TODO(bjornv): The conditional probabilities below are applied on the
                    // hard coded number of Gaussians set to two. Find a way to generalize.
                    // Calculate local noise probabilities used later when updating the GMM.
                    h0 = (Int16)(h0_test >> 12);  // Q15
                    if (h0 > 0)
                    {
                        // High probability of noise. Assign conditional probabilities for each
                        // Gaussian in the GMM.
                        tmp1_s32 = (int)(noise_probability[0] & 0xFFFFF000) << 2;  // Q29
                        ngprvec[channel] = (Int16)DivisionOperations.WebRtcSpl_DivW32W16(tmp1_s32, h0);  // Q14
                        ngprvec[channel + kNumChannels] = (Int16)(16384 - ngprvec[channel]);
                    }
                    else
                    {
                        // Low noise probability. Assign conditional probability 1 to the first
                        // Gaussian and 0 to the rest (which is already set at initialization).
                        ngprvec[channel] = 16384;
                    }

                    // Calculate local speech probabilities used later when updating the GMM.
                    h1 = (Int16)(h1_test >> 12);  // Q15
                    if (h1 > 0)
                    {
                        // High probability of speech. Assign conditional probabilities for each
                        // Gaussian in the GMM. Otherwise use the initialized values, i.e., 0.
                        tmp1_s32 = (int)(speech_probability[0] & 0xFFFFF000) << 2;  // Q29
                        sgprvec[channel] = (Int16)DivisionOperations.WebRtcSpl_DivW32W16(tmp1_s32, h1);  // Q14
                        sgprvec[channel + kNumChannels] = (Int16)(16384 - sgprvec[channel]);
                    }
                }

                // Make a global VAD decision.
                vadflag |= (short)(sum_log_likelihood_ratios >= totalTest ? 1 : 0);

                // Update the model parameters.
                maxspe = 12800;
                for (channel = 0; channel < kNumChannels; channel++)
                {

                    // Get minimum value in past which is used for long term correction in Q4.
                    feature_minimum = VadSp.WebRtcVad_FindMinimum(self, features[channel], channel);

                    // Compute the "global" mean, that is the sum of the two means weighted.
                    noise_global_mean = WeightedAverage(self.noise_means, 0,
                                                        kNoiseDataWeights, channel);
                    tmp1_s16 = (Int16)(noise_global_mean >> 6);  // Q8

                    for (k = 0; k < kNumGaussians; k++)
                    {
                        gaussian = channel + k * kNumChannels;

                        nmk = self.noise_means[gaussian];
                        smk = self.speech_means[gaussian];
                        nsk = self.noise_stds[gaussian];
                        ssk = self.speech_stds[gaussian];

                        // Update noise mean vector if the frame consists of noise only.
                        nmk2 = nmk;
                        if (vadflag == 0)
                        {
                            // deltaN = (x-mu)/sigma^2
                            // ngprvec[k] = |noise_probability[k]| /
                            //   (|noise_probability[0]| + |noise_probability[1]|)

                            // (Q14 * Q11 >> 11) = Q14.
                            delt = (Int16)Macros.WEBRTC_SPL_MUL_16_16_RSFT(ngprvec[gaussian],
                                                                       deltaN[gaussian],
                                                                       11);
                            // Q7 + (Q14 * Q15 >> 22) = Q7.
                            nmk2 = (Int16)(nmk + Macros.WEBRTC_SPL_MUL_16_16_RSFT(delt,
                                                                             kNoiseUpdateConst,
                                                                             22));
                        }

                        // Long term correction of the noise mean.
                        // Q8 - Q8 = Q8.
                        ndelt = (Int16)((feature_minimum << 4) - tmp1_s16);
                        // Q7 + (Q8 * Q8) >> 9 = Q7.
                        nmk3 = (Int16)(nmk2 + Macros.WEBRTC_SPL_MUL_16_16_RSFT(ndelt, kBackEta, 9));

                        // Control that the noise mean does not drift to much.
                        tmp_s16 = (Int16)((k + 5) << 7);
                        if (nmk3 < tmp_s16)
                        {
                            nmk3 = tmp_s16;
                        }
                        tmp_s16 = (Int16)((72 + k - channel) << 7);
                        if (nmk3 > tmp_s16)
                        {
                            nmk3 = tmp_s16;
                        }
                        self.noise_means[gaussian] = nmk3;

                        if (vadflag != 0)
                        {
                            // Update speech mean vector:
                            // |deltaS| = (x-mu)/sigma^2
                            // sgprvec[k] = |speech_probability[k]| /
                            //   (|speech_probability[0]| + |speech_probability[1]|)

                            // (Q14 * Q11) >> 11 = Q14.
                            delt = (Int16)Macros.WEBRTC_SPL_MUL_16_16_RSFT(sgprvec[gaussian],
                                                                       deltaS[gaussian],
                                                                       11);
                            // Q14 * Q15 >> 21 = Q8.
                            tmp_s16 = (Int16)Macros.WEBRTC_SPL_MUL_16_16_RSFT(delt,
                                                                          kSpeechUpdateConst,
                                                                          21);
                            // Q7 + (Q8 >> 1) = Q7. With rounding.
                            smk2 = (Int16)(smk + ((tmp_s16 + 1) >> 1));

                            // Control that the speech mean does not drift to much.
                            maxmu = (Int16)(maxspe + 640);
                            if (smk2 < kMinimumMean[k])
                            {
                                smk2 = kMinimumMean[k];
                            }
                            if (smk2 > maxmu)
                            {
                                smk2 = maxmu;
                            }
                            self.speech_means[gaussian] = smk2;  // Q7.

                            // (Q7 >> 3) = Q4. With rounding.
                            tmp_s16 = (Int16)((smk + 4) >> 3);

                            tmp_s16 = (Int16)(features[channel] - tmp_s16);  // Q4
                                                                             // (Q11 * Q4 >> 3) = Q12.
                            tmp1_s32 = Macros.WEBRTC_SPL_MUL_16_16_RSFT(deltaS[gaussian], tmp_s16, 3);
                            tmp2_s32 = tmp1_s32 - 4096;
                            tmp_s16 = (Int16)(sgprvec[gaussian] >> 2);
                            // (Q14 >> 2) * Q12 = Q24.
                            tmp1_s32 = tmp_s16 * tmp2_s32;

                            tmp2_s32 = tmp1_s32 >> 4;  // Q20

                            // 0.1 * Q20 / Q7 = Q13.
                            if (tmp2_s32 > 0)
                            {
                                tmp_s16 = (Int16)DivisionOperations.WebRtcSpl_DivW32W16(tmp2_s32, (Int16)(ssk * 10));
                            }
                            else
                            {
                                tmp_s16 = (Int16)DivisionOperations.WebRtcSpl_DivW32W16(-tmp2_s32, (Int16)(ssk * 10));
                                tmp_s16 = (Int16)(-tmp_s16);
                            }
                            // Divide by 4 giving an update factor of 0.025 (= 0.1 / 4).
                            // Note that division by 4 equals shift by 2, hence,
                            // (Q13 >> 8) = (Q13 >> 6) / 4 = Q7.
                            tmp_s16 += 128;  // Rounding.
                            ssk += (Int16)(tmp_s16 >> 8);
                            if (ssk < kMinStd)
                            {
                                ssk = kMinStd;
                            }
                            self.speech_stds[gaussian] = ssk;
                        }
                        else
                        {
                            // Update GMM variance vectors.
                            // deltaN * (features[channel] - nmk) - 1
                            // Q4 - (Q7 >> 3) = Q4.
                            tmp_s16 = (Int16)(features[channel] - (nmk >> 3));
                            // (Q11 * Q4 >> 3) = Q12.
                            tmp1_s32 = Macros.WEBRTC_SPL_MUL_16_16_RSFT(deltaN[gaussian], tmp_s16, 3);
                            tmp1_s32 -= 4096;

                            // (Q14 >> 2) * Q12 = Q24.
                            tmp_s16 = (Int16)((ngprvec[gaussian] + 2) >> 2);
                            tmp2_s32 = tmp_s16 * tmp1_s32;
                            // Q20  * approx 0.001 (2^-10=0.0009766), hence,
                            // (Q24 >> 14) = (Q24 >> 4) / 2^10 = Q20.
                            tmp1_s32 = tmp2_s32 >> 14;

                            // Q20 / Q7 = Q13.
                            if (tmp1_s32 > 0)
                            {
                                tmp_s16 = (Int16)DivisionOperations.WebRtcSpl_DivW32W16(tmp1_s32, nsk);
                            }
                            else
                            {
                                tmp_s16 = (Int16)DivisionOperations.WebRtcSpl_DivW32W16(-tmp1_s32, nsk);
                                tmp_s16 = (Int16)(-tmp_s16);
                            }
                            tmp_s16 += 32;  // Rounding
                            nsk += (Int16)(tmp_s16 >> 6);  // Q13 >> 6 = Q7.
                            if (nsk < kMinStd)
                            {
                                nsk = kMinStd;
                            }
                            self.noise_stds[gaussian] = nsk;
                        }
                    }

                    // Separate models if they are too close.
                    // |noise_global_mean| in Q14 (= Q7 * Q7).
                    noise_global_mean = WeightedAverage(self.noise_means, 0,
                                                        kNoiseDataWeights, channel);

                    // |speech_global_mean| in Q14 (= Q7 * Q7).
                    speech_global_mean = WeightedAverage(self.speech_means, 0,
                                                         kSpeechDataWeights, channel);

                    // |diff| = "global" speech mean - "global" noise mean.
                    // (Q14 >> 9) - (Q14 >> 9) = Q5.
                    diff = (Int16)((speech_global_mean >> 9) - (noise_global_mean >> 9));
                    if (diff < kMinimumDifference[channel])
                    {
                        tmp_s16 = (Int16)(kMinimumDifference[channel] - diff);

                        // |tmp1_s16| = ~0.8 * (kMinimumDifference - diff) in Q7.
                        // |tmp2_s16| = ~0.2 * (kMinimumDifference - diff) in Q7.
                        tmp1_s16 = (Int16)Macros.WEBRTC_SPL_MUL_16_16_RSFT(13, tmp_s16, 2);
                        tmp2_s16 = (Int16)Macros.WEBRTC_SPL_MUL_16_16_RSFT(3, tmp_s16, 2);

                        // Move Gaussian means for speech model by |tmp1_s16| and update
                        // |speech_global_mean|. Note that |self.speech_means[channel]| is
                        // changed after the call.
                        speech_global_mean = WeightedAverage(self.speech_means,
                                                             tmp1_s16,
                                                             kSpeechDataWeights, channel);

                        // Move Gaussian means for noise model by -|tmp2_s16| and update
                        // |noise_global_mean|. Note that |self.noise_means[channel]| is
                        // changed after the call.
                        noise_global_mean = WeightedAverage(self.noise_means,
                                                            (Int16)(-tmp2_s16),
                                                            kNoiseDataWeights, channel);
                    }

                    // Control that the speech & noise means do not drift to much.
                    maxspe = kMaximumSpeech[channel];
                    tmp2_s16 = (Int16)(speech_global_mean >> 7);
                    if (tmp2_s16 > maxspe)
                    {
                        // Upper limit of speech model.
                        tmp2_s16 -= maxspe;

                        for (k = 0; k < kNumGaussians; k++)
                        {
                            self.speech_means[channel + k * kNumChannels] -= tmp2_s16;
                        }
                    }

                    tmp2_s16 = (Int16)(noise_global_mean >> 7);
                    if (tmp2_s16 > kMaximumNoise[channel])
                    {
                        tmp2_s16 -= kMaximumNoise[channel];

                        for (k = 0; k < kNumGaussians; k++)
                        {
                            self.noise_means[channel + k * kNumChannels] -= tmp2_s16;
                        }
                    }
                }
                self.frame_counter++;
            }

            // Smooth with respect to transition hysteresis.
            if (vadflag == 0)
            {
                if (self.over_hang > 0)
                {
                    vadflag = (Int16)(2 + self.over_hang);
                    self.over_hang--;
                }
                self.num_of_speech = 0;
            }
            else
            {
                self.num_of_speech++;
                if (self.num_of_speech > kMaxSpeechFrames)
                {
                    self.num_of_speech = kMaxSpeechFrames;
                    self.over_hang = overhead2;
                }
                else
                {
                    self.over_hang = overhead1;
                }
            }
            return vadflag;
        }



        // Initialize the VAD. Set aggressiveness mode to default value.

        // Initializes the core VAD component. The default aggressiveness mode is
        // controlled by |kDefaultMode| in vad_core.c.
        //
        // - self [i/o] : Instance that should be initialized
        //
        // returns      : 0 (OK), -1 (NULL pointer in or if the default mode can't be
        //                set)
        static public int WebRtcVad_InitCore(VadInstT self)
        {
            int i;

            if (self == null)
            {
                return -1;
            }

            // Initialization of general struct variables.
            self.vad = 1;  // Speech active (=1).
            self.frame_counter = 0;
            self.over_hang = 0;
            self.num_of_speech = 0;

            // Initialization of downsampling filter state.
            //memset(self.downsampling_filter_states, 0, sizeof(self.downsampling_filter_states));

            // Initialization of 48 to 8 kHz downsampling.
            Resample.WebRtcSpl_ResetResample48khzTo8khz(self.state_48_to_8);

            // Read initial PDF parameters.
            for (i = 0; i < kTableSize; i++)
            {
                self.noise_means[i] = kNoiseDataMeans[i];
                self.speech_means[i] = kSpeechDataMeans[i];
                self.noise_stds[i] = kNoiseDataStds[i];
                self.speech_stds[i] = kSpeechDataStds[i];
            }

            // Initialize Index and Minimum value vectors.
            for (i = 0; i < 16 * kNumChannels; i++)
            {
                self.low_value_vector[i] = 10000;
                self.index_vector[i] = 0;
            }

            // Initialize splitting filter states.
            //memset(self.upper_state, 0, sizeof(self.upper_state));
            //memset(self.lower_state, 0, sizeof(self.lower_state));

            // Initialize high pass filter states.
            //memset(self.hp_filter_state, 0, sizeof(self.hp_filter_state));

            // Initialize mean value memory, for WebRtcVad_FindMinimum().
            for (i = 0; i < kNumChannels; i++)
            {
                self.mean_value[i] = 1600;
            }

            // Set aggressiveness mode to default (=|kDefaultMode|).
            if (WebRtcVad_set_mode_core(self, kDefaultMode) != 0)
            {
                return -1;
            }

            self.init_flag = kInitCheck;

            return 0;
        }


        // Set aggressiveness mode

        /****************************************************************************
         * WebRtcVad_set_mode_core(...)
         *
         * This function changes the VAD settings
         *
         * Input:
         *      - inst      : VAD instance
         *      - mode      : Aggressiveness degree
         *                    0 (High quality) - 3 (Highly aggressive)
         *
         * Output:
         *      - inst      : Changed  instance
         *
         * Return value     :  0 - Ok
         *                    -1 - Error
         */
        public static int WebRtcVad_set_mode_core(VadInstT self, int mode)
        {
            int return_value = 0;

            switch (mode)
            {
                case 0:
                    // Quality mode.
                    Array.Copy(kOverHangMax1Q, self.over_hang_max_1,
                           self.over_hang_max_1.Length);
                    Array.Copy(kOverHangMax2Q, self.over_hang_max_2,
                           self.over_hang_max_2.Length);
                    Array.Copy(kLocalThresholdQ, self.individual,
                           self.individual.Length);
                    Array.Copy(kGlobalThresholdQ, self.total,
                           self.total.Length);
                    break;
                case 1:
                    // Low bitrate mode.
                    Array.Copy(kOverHangMax1LBR, self.over_hang_max_1,
                           self.over_hang_max_1.Length);
                    Array.Copy(kOverHangMax2LBR, self.over_hang_max_2,
                           self.over_hang_max_2.Length);
                    Array.Copy(kLocalThresholdLBR, self.individual,
                           self.individual.Length);
                    Array.Copy(kGlobalThresholdLBR, self.total,
                           self.total.Length);
                    break;
                case 2:
                    // Aggressive mode.
                    Array.Copy(kOverHangMax1AGG, self.over_hang_max_1,
                           self.over_hang_max_1.Length);
                    Array.Copy(kOverHangMax2AGG, self.over_hang_max_2,
                           self.over_hang_max_2.Length);
                    Array.Copy(kLocalThresholdAGG, self.individual,
                           self.individual.Length);
                    Array.Copy(kGlobalThresholdAGG, self.total,
                           self.total.Length);
                    break;
                case 3:
                    // Very aggressive mode.
                    Array.Copy(kOverHangMax1VAG, self.over_hang_max_1,
                           self.over_hang_max_1.Length);
                    Array.Copy(kOverHangMax2VAG, self.over_hang_max_2,
                           self.over_hang_max_2.Length);
                    Array.Copy(kLocalThresholdVAG, self.individual,
                           self.individual.Length);
                    Array.Copy(kGlobalThresholdVAG, self.total,
                           self.total.Length);
                    break;
                default:
                    return_value = -1;
                    break;
            }

            return return_value;
        }


        // Calculate VAD decision by first extracting feature values and then calculate
        // probability for both speech and background noise.

        /****************************************************************************
        * WebRtcVad_CalcVad48khz(...)
        * WebRtcVad_CalcVad32khz(...)
        * WebRtcVad_CalcVad16khz(...)
        * WebRtcVad_CalcVad8khz(...)
        *
        * Calculate probability for active speech and make VAD decision.
        *
        * Input:
        *      - inst          : Instance that should be initialized
        *      - speech_frame  : Input speech frame
        *      - frame_length  : Number of input samples
        *
        * Output:
        *      - inst          : Updated filter states etc.
        *
        * Return value         : VAD decision
        *                        0 - No active speech
        *                        1-6 - Active speech
        */
        public static int WebRtcVad_CalcVad48khz(VadInstT inst, Int16[] speech_frame, int frame_length)
        {
            int vad;
            int i;
            Int16[] speech_nb = new Int16[240];  // 30 ms in 8 kHz.
                                                 // |tmp_mem| is a temporary memory used by resample function, length is
                                                 // frame length in 10 ms (480 samples) + 256 extra.
            Int32[] tmp_mem = new Int32[480 + 256];
            const int kFrameLen10ms48khz = 480;
            const int kFrameLen10ms8khz = 80;
            int num_10ms_frames = frame_length / kFrameLen10ms48khz;

            for (i = 0; i < num_10ms_frames; i++)
            {
                Resample.WebRtcSpl_Resample48khzTo8khz(speech_frame,
                                              speech_nb, i * kFrameLen10ms8khz,
                                              inst.state_48_to_8,
                                              tmp_mem);
            }

            // Do VAD on an 8 kHz signal
            vad = WebRtcVad_CalcVad8khz(inst, speech_nb, frame_length / 6);

            return vad;
        }

        public static int WebRtcVad_CalcVad32khz(VadInstT inst, Int16[] speech_frame, int frame_length)
        {
            int len, vad;
            Int16[] speechWB = new Int16[480]; // Downsampled speech frame: 960 samples (30ms in SWB)
            Int16[] speechNB = new Int16[240]; // Downsampled speech frame: 480 samples (30ms in WB)


            // Downsample signal 32.16.8 before doing VAD
            VadSp.WebRtcVad_Downsampling(speech_frame, speechWB, inst.downsampling_filter_states,
                                   frame_length, 2);
            len = frame_length / 2;

            VadSp.WebRtcVad_Downsampling(speechWB, speechNB, inst.downsampling_filter_states, len, 0);
            len /= 2;

            // Do VAD on an 8 kHz signal
            vad = WebRtcVad_CalcVad8khz(inst, speechNB, len);

            return vad;
        }

        public static int WebRtcVad_CalcVad16khz(VadInstT inst, Int16[] speech_frame, int frame_length)
        {
            int len, vad;
            Int16[] speechNB = new Int16[240]; // Downsampled speech frame: 480 samples (30ms in WB)

            // Wideband: Downsample signal before doing VAD
            VadSp.WebRtcVad_Downsampling(speech_frame, speechNB, inst.downsampling_filter_states,
                                   frame_length, 0);

            len = frame_length / 2;
            vad = WebRtcVad_CalcVad8khz(inst, speechNB, len);

            return vad;
        }

        public static int WebRtcVad_CalcVad8khz(VadInstT inst, Int16[] speech_frame, int frame_length)
        {
            Int16[] feature_vector = new Int16[kNumChannels];
            Int16 total_power;

            // Get power in the bands
            total_power = VadFilterBank.WebRtcVad_CalculateFeatures(inst, speech_frame, frame_length,
                                                      feature_vector);

            // Make a VAD
            inst.vad = GmmProbability(inst, feature_vector, total_power, frame_length);

            return inst.vad;
        }


    }
}
