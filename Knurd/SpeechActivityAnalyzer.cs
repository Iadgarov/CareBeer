using System;
using System.Runtime.InteropServices;
using Windows.Media;
using CareBeer;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using WebRtc.CommonAudio.Vad;

namespace CareBeer
{
    using VadInst = VadInstT;

    class SpeechActivityAnalyzer
	{
        
		//private static VadInst vad;

		//static SpeechActivityAnalyzer()
		//{
		//	// create and initialize the vad struct
  //          vad = WebRtcVad.WebRtcVad_Create();
  //          WebRtcVad.WebRtcVad_Init(vad);
  //          WebRtcVad.WebRtcVad_set_mode(vad, 3);
		//}

		private StringBuilder voiceStringBuilder;

		public string VoiceString { get; private set; }
		public List<byte> VoicedUnvoicedVector { get; private set; } // a list of 1s and 0s representing voiced/unvoiced sections respectively
		public List<int> VoicedUnvoicedLengthsVector { get; private set; } // a list of the lengths of voiced/unvoiced sections (odd/even indices respectively)

		public double PauseLengthVariance { get; private set; }
		public double PauseLengthMean { get; private set; }

		public double SpeechLengthVariance { get; private set; }
		public double SpeechLengthMean { get; private set; }


		public SpeechActivityAnalyzer()
		{
			voiceStringBuilder = new StringBuilder();
			VoicedUnvoicedVector = new List<byte>();
			VoicedUnvoicedLengthsVector = new List<int>();
		}

		private static IEnumerable<Int16[]> GetFrames(int frameLen, Int16[] audio, int sampleRate)
		{
			int n = (sampleRate * frameLen / 1000); // frameLen in msec
			int offset = 0;

			while (offset + n < audio.Length)
			{
				Int16[] frame = new Int16[n];
				Array.Copy(audio, offset, frame, 0, n);
				offset += n;
				yield return frame;
			}
		}

		public void AnalyzeAudio(Int16[] audio, int sampleRate, int frameLen)
		{
            VadInst vad = WebRtcVad.WebRtcVad_Create();

            foreach (Int16[] frame in GetFrames(frameLen, audio, sampleRate))
			{
                WebRtcVad.WebRtcVad_Init(vad);
                WebRtcVad.WebRtcVad_set_mode(vad, 3);

                int isVoiced = WebRtcVad.WebRtcVad_Process(vad, sampleRate, frame, frame.Length);

                //fixed (byte *raw = frame)
                //{
                //	Int16 *data = (Int16*)raw;
                //	int valid = WebRtcVad.WebRtcVad_ValidRateAndFrameLength(sampleRate, frame.Length / 2);
                //	isVoiced = VadWrapperProcess(vad, sampleRate, data, frame.Length / 2);
                //}

                voiceStringBuilder.Append(isVoiced);
				VoicedUnvoicedVector.Add((byte) isVoiced);
			}

			FinalizeAnalysis();
		}

		//unsafe public void AnalyzeFrame(AudioFrame frame, int sampleRate)
		//{
		//	int isVoiced;

		//	using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
		//	using (IMemoryBufferReference reference = buffer.CreateReference())
		//	{
		//		byte* dataInBytes;
		//		uint capacityInBytes;

		//		// Get the buffer from the AudioFrame
		//		((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacityInBytes);

		//		Int16 *frameData = (Int16*)dataInBytes;
		//		uint frameLength = capacityInBytes / 2;

		//		int valid = VadWrapperValidate(vad, sampleRate, (int)frameLength);
		//		isVoiced = VadWrapperProcess(vad, sampleRate, frameData, (int)frameLength);
		//	}
			
		//	voiceStringBuilder.Append(isVoiced);
		//	VoicedUnvoicedVector.Add((byte) isVoiced);
		//}

		public void FinalizeAnalysis()
		{
			VoiceString = voiceStringBuilder.ToString();

			// voice length vector
			int count = 0;
			int curr = 0;
			foreach (byte val in VoicedUnvoicedVector)
			{
				if (val == curr)
				{
					count++;
				}
				else
				{
					VoicedUnvoicedLengthsVector.Add(count);
					count = 1;
					curr = 1 - curr;
				}
			}
			VoicedUnvoicedLengthsVector.Add(count);

			List<int> pauseLengthVector = VoicedUnvoicedLengthsVector.SublistModulo(2, 0);
			List<int> speechLengthVector = VoicedUnvoicedLengthsVector.SublistModulo(2, 1);

			// means
			PauseLengthMean = pauseLengthVector.Select<int, double>(i => i).ToList().Mean();
			SpeechLengthMean = speechLengthVector.Select<int, double>(i => i).ToList().Mean();

			// variances
			PauseLengthVariance = pauseLengthVector.Select<int, double>(i => i).ToList().Variance(PauseLengthMean);
			SpeechLengthVariance = speechLengthVector.Select<int, double>(i => i).ToList().Variance(SpeechLengthMean);
		}



		[ComImport]
		[Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		unsafe interface IMemoryBufferByteAccess
		{
			void GetBuffer(out byte* buffer, out uint capacity);
		}
	}
}
