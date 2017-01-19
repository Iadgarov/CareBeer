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



namespace Knurd
{
    public sealed partial class AccelerometerPage : Page
    {



        AccelerometerViewModel vm;


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


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            if (Accelerometer.GetDefault() == null)
            {
                Debug.WriteLine("no accleromerter!");
                vm = new AccelerometerViewModel();
                noAccMessage();

            }


            vm = new AccelerometerViewModel();
            DataContext = vm;

        }

        private void startMode()
        {
            start = true;
            startStopButton.Content = "Start";
        }

        private void stopMode()
        {
            start = false;
            startStopButton.Content = "Stop";
        }

        private async void noAccMessage()
        {
            MessageDialog m = new MessageDialog("This device does not have an accelerometer!");
            await m.ShowAsync();
            this.Frame.GoBack();

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
            vm.Start();
        }

        private async void btnStop_Click()
        {
            

            Debug.WriteLine("stop clicked");
            if (!vm.Stop())
                return; // stop failed, not actually running .can't stop what's not running. stop asshole double clickers 

          
            summaryMessage();
    

            await CloudServices.replaceIneEntity(EntryPage.user);

            vm = new AccelerometerViewModel(); // fresh dataset for future test. by now things should have been saved. 

            
            
            
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

            await CloudServices.replaceIneEntity(EntryPage.user);

            vm = new AccelerometerViewModel(); // fresh dataset for future test. by now things should have been saved. 

            startMode();

        }


        private async void summaryMessage()
        {

            string s = "";
            s += "Step Count: " + vm.getStepCount();

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
                this.Frame.Navigate(typeof(BubblePage)); 
            }
            else if (r.Label == "Redo")
            {
                startMode(); // reset
            }

        }



    }
}
