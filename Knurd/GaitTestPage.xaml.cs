/*
 * To add Offline Sync Support:
 *  1) Add the NuGet package Microsoft.Azure.Mobile.Client.SQLiteStore (and dependencies) to all client projects
 *  2) Uncomment the #define OFFLINE_SYNC_ENABLED
 *
 * For more information, see: http://go.microsoft.com/fwlink/?LinkId=717898
 */
//#define OFFLINE_SYNC_ENABLED

using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Windows.UI.Xaml.Navigation;

using Windows.Devices.Sensors;

using Windows.UI.Popups;
using System.Diagnostics;
using System.Threading.Tasks;
using CareBeer.Tests;
using Windows.UI.Core;
using System.Text;

namespace CareBeer
{
    public sealed partial class AccelerometerPage : Page
    {

        //AccelerometerViewModel vm;
		GaitTest tester;

        // Sensor and dispatcher variables
        private Accelerometer _accelerometer;

        private bool start = true;

        public static AccelerometerPage current;


        public AccelerometerPage()
        {
            this.InitializeComponent();
            _accelerometer = Accelerometer.GetDefault();

            if (_accelerometer != null)
            {
                // Establish the report interval
                uint minReportInterval = _accelerometer.MinimumReportInterval;
                uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _accelerometer.ReportInterval = reportInterval;
            }
            current = this;
        }


        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            Frame.BackStack.Clear();

            tester = args.Parameter as GaitTest;

            if (Accelerometer.GetDefault() == null)
            {
                Debug.WriteLine("no accleromerter!");
                //tester.AccVm = new AccelerometerViewModel();
                noAccMessage();
                return;
            }


            tester.AccVm = new AccelerometerViewModel();
            DataContext = tester.AccVm;

            beginMessage();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            tester.AccVm?.Stop(true); // stop prematurly

            Frame.BackStack.Clear();
        }


        private async void beginMessage()
        {
            MessageDialog m = new MessageDialog("This is the gait analysis test.\nWhen you're ready, press the button and start walking as usual. After approximately 40 steps of normal walking, stop and press the button again.");
            m.Commands.Add(new UICommand("Got it!"));
            await m.ShowAsync();

        }


        private void startMode()
        {
            start = true;
            startStopButton.Content = "Go!";
			startStopButton.IsEnabled = true;
            instructions.Visibility = Visibility.Visible;
        }

        private void stopMode()
        {
            start = false;
            startStopButton.Content = "Stop";
            instructions.Visibility = Visibility.Collapsed;
        }

        private async void noAccMessage()
        {
            MessageDialog m = new MessageDialog("This device does not have an accelerometer!");
            await m.ShowAsync();
            tester.Finished(true);

        }




        private void btn_Click(object sender, RoutedEventArgs e)
        {

            if (start)
            {
                btnStart_Click();
            }
            else
            {
                btnStop_Click();
            }
        }


        private void btnStart_Click()
        {

            stopMode();
            tester.AccVm.Start();
        }

        private async void btnStop_Click()
        {
            

            Debug.WriteLine("stop clicked");
			startStopButton.IsEnabled = false;
			tester.AccVm.Stop(false);
			// if (!tester.AccVm.Stop(false))
			//   return; // stop failed, not actually running .can't stop what's not running. stop asshole double clickers 

			summaryMessage();

        }


        public async void enoughDataMessage()
        {
            
            string s = "";
            s += "You have walked enough. Thank you!";

            MessageDialog m = new MessageDialog(s);
            try
            {
                await m.ShowAsync();
                summaryMessage();
            }
            catch (Exception e)
            {
                //Debug.WriteLine(e.StackTrace);
            }

			summaryMessage();

        }


        private async void summaryMessage()
        {

            string s = "";
            s += "Step Count: " + tester.AccVm.getStepCount();

            MessageDialog m = new MessageDialog(s);
            m.Commands.Add(new UICommand("Next"));
            m.Commands.Add(new UICommand("Redo"));

            IUICommand r = null;
            try
            {
                r = await m.ShowAsync();
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }

            if (r == null)
            {
                return;
            }

            if (r.Label == "Next")
            {
				tester.Finished(false);
            }
            else if (r.Label == "Redo")
            {
				tester.AccVm = new AccelerometerViewModel();
				startMode(); // reset
            }

        }



    }
}
