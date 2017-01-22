using System;

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Windows.Media.Render;
using Windows.Media.Capture;
using Windows.Media.Audio;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Media;
using Windows.Storage;

using Windows.Devices.Enumeration;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.Runtime.InteropServices;
using Windows.UI.Popups;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CareBeer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SpeechRecordingPage : Page
    {
        private AudioGraph graph;
        private AudioFileOutputNode fileOutputNode;
        private StorageFile tempAudioFile;
		private AudioFrameOutputNode frameOutputNode;
        private AudioDeviceInputNode deviceInputNode;

		private SpeechActivityAnalyzer vadAnalyzer;

		private byte[] audio;
		private int audioLen; // in number of bytes

        public SpeechRecordingPage()
        {
            this.InitializeComponent();
			
		}

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            beginMessage();

        }

        private async void beginMessage()
        {
            MessageDialog m = new MessageDialog("Begin recording when ready.");
            m.Commands.Add(new UICommand("OK"));
            await m.ShowAsync();
           


        }

        //protected override void OnNavigatedFrom(NavigationEventArgs e)
        //      {

        //      }

        private void RecordStopBtn_Click(object sender, RoutedEventArgs e)
        {
			ToggleRecordStop();
        }

        private void _RecordStopBtn_Click(object sender, RoutedEventArgs e)
        {

            
            ButtonAutomationPeer peer = new ButtonAutomationPeer(startStopButton);

            IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            invokeProv.Invoke();
            
        }


        /*
         -------------------
          Recording Logic
         -------------------
         */

        private async Task ToggleRecordStop()
		{
			if (recordStopBtn.Label.Equals("Record"))
			{
				await InitAudioGraph();
				audio = new byte[0];
				//vadAnalyzer = new SpeechActivityAnalyzer();
				ShowToastNotification("audio graph initialized");

				StartRecordAsync();
				recordStopBtn.Icon = new SymbolIcon(Symbol.Stop);
				recordStopBtn.Label = "Stop";
			}
			else if (recordStopBtn.Label.Equals("Stop"))
			{
				StopRecordAsync();

				recordStopBtn.Icon = new SymbolIcon(Symbol.Accept);
				recordStopBtn.Label = "Done";
				recordStopBtn.IsEnabled = false;
			}
		}


		private async Task InitAudioGraph()
        {
            AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.Speech);
			settings.DesiredSamplesPerQuantum = 32 * 30; // 32 KHz * 30 ms
			settings.QuantumSizeSelectionMode = QuantumSizeSelectionMode.ClosestToDesired;

            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);
            if (result.Status != AudioGraphCreationStatus.Success)
            {
                ShowToastNotification("AudioGraph creation error: " + result.Status.ToString());
                return;
            }

            graph = result.Graph;
			ShowToastNotification(graph.SamplesPerQuantum.ToString());
			// TODO: check samples per quantum
        }


        private async Task StartRecordAsync()
        {
			String fileName = "temp.wav";
			//tempAudioFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
			//		fileName, CreationCollisionOption.GenerateUniqueName);

			// create file in current location
			StorageFolder storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
			tempAudioFile = await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

			var microphone = await DeviceInformation.CreateFromIdAsync(
                MediaDevice.GetDefaultAudioCaptureId(AudioDeviceRole.Default));


            var outProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Low);
            outProfile.Audio = AudioEncodingProperties.CreatePcm(32000, 1, 16); // 32 KHz sample rate, 1 channel, 16 bits per sample

            var inProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);

            var outputResult = await this.graph.CreateFileOutputNodeAsync(tempAudioFile, outProfile);
            if (outputResult.Status != AudioFileNodeCreationStatus.Success)
            {
				// show error
				ShowToastNotification(outputResult.Status.ToString());
                return;
            }

            fileOutputNode = outputResult.FileOutputNode;

            var inputResult = await this.graph.CreateDeviceInputNodeAsync(MediaCategory.Speech, inProfile.Audio, microphone);
			if (inputResult.Status != AudioDeviceNodeCreationStatus.Success)
			{
				// show error
				ShowToastNotification(inputResult.Status.ToString());
				return;
			}

			deviceInputNode = inputResult.DeviceInputNode;
			deviceInputNode.AddOutgoingConnection(this.fileOutputNode);


			// create frame output node
			frameOutputNode = graph.CreateFrameOutputNode();
			graph.QuantumStarted += (sender, args) =>
			{
				AudioFrame frame = frameOutputNode.GetFrame();
				ProcessFrameOutput(frame);
				//vadAnalyzer.AnalyzeFrame(frame, 48000);
			};

			deviceInputNode.AddOutgoingConnection(frameOutputNode);


			graph.Start();
		}


		unsafe private void ProcessFrameOutput(AudioFrame frame)
		{
			using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
			using (IMemoryBufferReference reference = buffer.CreateReference())
			{
				byte* dataInBytes;
				uint capacityInBytes;

				// Get the buffer from the AudioFrame
				((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacityInBytes);

				Array.Resize<byte>(ref audio, audioLen + (int)capacityInBytes);
				Marshal.Copy((IntPtr)dataInBytes, audio, audioLen, (int)capacityInBytes);
				audioLen += (int)capacityInBytes;
			}
		}


		private async Task StopRecordAsync()
		{
			graph?.Stop();

			await fileOutputNode.FinalizeAsync();
			// TODO: check for errors

			//vadAnalyzer.AnalyzeAudio(audio, 32000, 30);

			double pauseVar = vadAnalyzer.PauseLengthVariance;
			double speechVar = vadAnalyzer.SpeechLengthVariance;

			pauseVarTxt.Text = string.Format("Pause length variance: {0}", pauseVar);
			speechVarTxt.Text = string.Format("Speech length variance: {0}", speechVar);

			graph?.Dispose();
		}


		static public void ShowToastNotification(string content, int timeout = 4)
		{
			ToastNotifier toastNotifier = ToastNotificationManager.CreateToastNotifier();

			XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
			XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
			toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(content));

			IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
			XmlElement audio = toastXml.CreateElement("audio");
			audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");

			ToastNotification toast = new ToastNotification(toastXml);
			toast.ExpirationTime = DateTime.Now.AddSeconds(timeout);
			toastNotifier.Show(toast);
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
