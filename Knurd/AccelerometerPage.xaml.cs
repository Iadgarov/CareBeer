/*
 * To add Offline Sync Support:
 *  1) Add the NuGet package Microsoft.Azure.Mobile.Client.SQLiteStore (and dependencies) to all client projects
 *  2) Uncomment the #define OFFLINE_SYNC_ENABLED
 *
 * For more information, see: http://go.microsoft.com/fwlink/?LinkId=717898
 */
//#define OFFLINE_SYNC_ENABLED

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.UI.Core;
using Windows.Devices.Sensors;
using Windows.UI;
using Windows.UI.Popups;
using System.Diagnostics;

#if OFFLINE_SYNC_ENABLED
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;  // offline sync
using Microsoft.WindowsAzure.MobileServices.Sync;         // offline sync
#endif

// for graphs: http://stackoverflow.com/questions/41190620/windows-universal-app-live-chart-of-accelerometer-data

namespace Knurd
{
    public sealed partial class AccelerometerPage : Page
    {



        AccelerometerViewModel vm;


        // Sensor and dispatcher variables
        private Accelerometer _accelerometer;

        private bool start = true;

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
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

#if OFFLINE_SYNC_ENABLED
            await InitLocalStoreAsync(); // offline sync
#endif
     

            if (Accelerometer.GetDefault() == null)
            {
                Debug.WriteLine("no accleromerter!");
                vm = new AccelerometerViewModel();
                //vm.dummyValueSmooth(); // for debug only
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
            
            vm.Stop();
            summaryMessage();
            await CloudServices.replaceIneEntity(EntryPage.user);

            vm = new AccelerometerViewModel(); // fresh dataset for future test. by now things should have been saved. 

            startMode();
        }


        private async void summaryMessage()
        {

            string s = "";
            s += "Step Count: " + vm.getStepCount();

            MessageDialog m = new MessageDialog(s);
            await m.ShowAsync();


        }



    }
}
