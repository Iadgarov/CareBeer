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
using System.Diagnostics;
using CareBeer.Tests;
using Windows.Media.Transcoding;
using System.Threading;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CareBeer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SpeechRecordingPage : Page
    {
        private const int SAMPLE_RATE_KHZ = 32; 
        private const int FRAME_LEN_MILLISEC = 30;

        // audio graph + input and output nodes
        private AudioGraph graph;
        private AudioFileOutputNode fileOutputNode;
		private AudioFrameOutputNode frameOutputNode;
        private AudioDeviceInputNode deviceInputNode;
        private AudioFileInputNode fileInputNode;
        private MediaEncodingProfile profile;

        private StorageFile tempAudioFile;
        private String fileName;

        private Int16[] audio;
		private int audioLen; // in number of bytes

        SpeechTest tester;

        public SpeechRecordingPage()
        {
            this.InitializeComponent();
			
		}

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame.BackStack.Clear();
            tester = e.Parameter as SpeechTest;
            tester.vadAnalyzer = new SpeechActivityAnalyzer();

            beginMessage();

        }

        private async void beginMessage()
        {
            MessageDialog m = new MessageDialog("This is the speech analysis test.\nWhen you're ready, press the button and read the displayed text aloud. When finished, stop the recording.");
            m.Commands.Add(new UICommand("OK"));
            await m.ShowAsync();
           
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Frame.BackStack.Clear();
        }

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
				audio = new Int16[0];
                //vadAnalyzer = new SpeechActivityAnalyzer();

                textScroll.Visibility = Visibility.Visible;

				recordStopBtn.Icon = new SymbolIcon(Symbol.Stop);
				recordStopBtn.Label = "Stop";
                startStopButton.Width = 100;
                startStopButton.Height = 100;

                await StartRecordAsync();
            }
			else if (recordStopBtn.Label.Equals("Stop"))
			{
				

				recordStopBtn.Icon = new SymbolIcon(Symbol.Accept);
				recordStopBtn.Label = "Done";
				recordStopBtn.IsEnabled = false;
                await StopRecordAsync();
            }
		}


		private async Task InitAudioGraph()
        {
            AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.Speech);
			settings.DesiredSamplesPerQuantum = SAMPLE_RATE_KHZ * FRAME_LEN_MILLISEC;
			settings.QuantumSizeSelectionMode = QuantumSizeSelectionMode.ClosestToDesired;

            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);
            if (result.Status != AudioGraphCreationStatus.Success)
            {
                Debug.WriteLine("AudioGraph creation error: " + result.Status.ToString());
                return;
            }

            graph = result.Graph;
			// TODO: check samples per quantum
        }


        private async Task addFileOutputNode()
        {
            fileName = "temp.wav";
            tempAudioFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
                    fileName, CreationCollisionOption.GenerateUniqueName);

            // create file in current location
            //StorageFolder storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            //tempAudioFile = await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            fileName = tempAudioFile.Name;

            var outProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Low);
            outProfile.Audio = AudioEncodingProperties.CreatePcm(SAMPLE_RATE_KHZ * 1000, 1, 16); // 32 KHz sample rate, 1 channel, 16 bits per sample 

            var outputResult = await this.graph.CreateFileOutputNodeAsync(tempAudioFile, outProfile);
            if (outputResult.Status != AudioFileNodeCreationStatus.Success)
            {
                // show error
                Debug.WriteLine(outputResult.Status.ToString());
                return;
            }

            fileOutputNode = outputResult.FileOutputNode;

            deviceInputNode.AddOutgoingConnection(fileOutputNode);
        }


        private async Task addDeviceInputNode()
        {
            var microphone = await DeviceInformation.CreateFromIdAsync(
                MediaDevice.GetDefaultAudioCaptureId(AudioDeviceRole.Default));

            var inProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);
            var inputResult = await this.graph.CreateDeviceInputNodeAsync(MediaCategory.Speech, inProfile.Audio, microphone);
            if (inputResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                // show error
                Debug.WriteLine(inputResult.Status.ToString());
                return;
            }

            deviceInputNode = inputResult.DeviceInputNode;
        }


        private void addFrameOutputNode()
        {
            frameOutputNode = graph.CreateFrameOutputNode(profile.Audio);
            graph.QuantumStarted += (sender, args) =>
            {
                AudioFrame frame = frameOutputNode.GetFrame();
                ProcessFrameOutput(frame);
            };

            //deviceInputNode.AddOutgoingConnection(frameOutputNode);
            fileInputNode.AddOutgoingConnection(frameOutputNode);
        }


        private async Task addFileInputNode()
        {
            tempAudioFile = await ApplicationData.Current.TemporaryFolder.GetFileAsync(fileName);

            var inputResult = await graph.CreateFileInputNodeAsync(tempAudioFile);
            if (inputResult.Status != AudioFileNodeCreationStatus.Success)
            {
                Debug.WriteLine(inputResult.Status.ToString());
                return;
            }

            fileInputNode = inputResult.FileInputNode;
            fileInputNode.FileCompleted += OnFileCompleted;
        }


        private async void OnFileCompleted(AudioFileInputNode sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                graph.Stop();
                graph.Dispose();

                tempAudioFile.DeleteAsync();

                tester.vadAnalyzer.AnalyzeAudio(audio, SAMPLE_RATE_KHZ * 1000, FRAME_LEN_MILLISEC);

                summaryMessage();
            });
        }

        // Phase 1: record user and save to file
        private async Task recordAudioToFile()
        {
            await addDeviceInputNode();
            await addFileOutputNode();

            graph.Start();
        }


        // Phase 2: read from file into audio frames to be analyzed
        private async Task readAudioFileToFrames()
        {
            // delete old graph and start again
            graph.Dispose();
            await InitAudioGraph();

            profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Low);
            profile.Audio.SampleRate = SAMPLE_RATE_KHZ * 1000;
            profile.Audio.BitsPerSample = 16;
            profile.Audio.ChannelCount = 1;

            await addFileInputNode();
            addFrameOutputNode();
            graph.Start();
        }


        private async Task StartRecordAsync()
        {
            await recordAudioToFile();

        }


		unsafe private void ProcessFrameOutput(AudioFrame frame)
		{
			using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Read))
			using (IMemoryBufferReference reference = buffer.CreateReference())
			{
				byte* dataInBytes;
				uint capacityInBytes;

				// Get the buffer from the AudioFrame
				((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacityInBytes);

                int capacityInShorts = (int)capacityInBytes / 2;
                short* dataInShort = (short*)dataInBytes;

				Array.Resize(ref audio, audioLen + capacityInShorts);
				Marshal.Copy((IntPtr)dataInShort, audio, audioLen, capacityInShorts);
                //for (int i = 0; i < capacityInShorts; i++)
                //{
                //    audio[audioLen + i] = dataInShort[i];
                //}
				audioLen += capacityInShorts;
			}
		}


		private async Task StopRecordAsync()
		{
			graph.Stop();

            TranscodeFailureReason fail = await fileOutputNode.FinalizeAsync();
            if (fail != TranscodeFailureReason.None)
            {
                Debug.WriteLine(fail.ToString());
            }

            await readAudioFileToFrames();

        }


        private async Task summaryMessage()
        {
            //string s = "";
            //s += "pause length mean: " + tester.vadAnalyzer.PauseLengthMean + "\n";
            //s += "pause length variance: " + tester.vadAnalyzer.PauseLengthVariance + "\n\n";
            //s += "speech length mean: " + tester.vadAnalyzer.SpeechLengthMean + "\n";
            //s += "speech length variance: " + tester.vadAnalyzer.SpeechLengthVariance + "\n";

            MessageDialog m = new MessageDialog("Very nice!");
            m.Commands.Add(new UICommand("Next"));
            m.Commands.Add(new UICommand("Redo"));


            var r = await m.ShowAsync();
            if (r == null)
            {
                return;
            }

            if (r.Label == "Next")
            {
                tester.Finished();
            }
            else if (r.Label == "Redo")
            {
                reset();
            }

        }


        private void reset()
        {
            tester.vadAnalyzer = new SpeechActivityAnalyzer();

            recordStopBtn.Icon = new SymbolIcon(Symbol.Microphone);
            recordStopBtn.Label = "Record";
            recordStopBtn.IsEnabled = true;

            startStopButton.Width = 300;
            startStopButton.Height = 300;

            textScroll.Visibility = Visibility.Collapsed;

            beginMessage();
        }


        //static public void ShowToastNotification(string content, int timeout = 4)
        //{
        //	ToastNotifier toastNotifier = ToastNotificationManager.CreateToastNotifier();

        //	XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
        //	XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
        //	toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(content));

        //	IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
        //	XmlElement audio = toastXml.CreateElement("audio");
        //	audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");

        //	ToastNotification toast = new ToastNotification(toastXml);
        //	toast.ExpirationTime = DateTime.Now.AddSeconds(timeout);
        //	toastNotifier.Show(toast);
        //}


        [ComImport]
		[Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		unsafe interface IMemoryBufferByteAccess
		{
			void GetBuffer(out byte* buffer, out uint capacity);
		}

	}
}
