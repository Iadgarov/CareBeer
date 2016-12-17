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

#if OFFLINE_SYNC_ENABLED
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;  // offline sync
using Microsoft.WindowsAzure.MobileServices.Sync;         // offline sync
#endif

// for graphs: http://stackoverflow.com/questions/41190620/windows-universal-app-live-chart-of-accelerometer-data

namespace Knurd
{
    public sealed partial class AccelerometerPage : Page
    {



        MyViewModel vm;

        // Sensor and dispatcher variables
        private Accelerometer _accelerometer;

        // This event handler writes the current accelerometer reading to 
        // the three acceleration text blocks on the app' s main page.

        private async void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AccelerometerReading reading = e.Reading;
                txtXAxis.Text = String.Format("{0,5:0.00}", reading.AccelerationX);
                txtYAxis.Text = String.Format("{0,5:0.00}", reading.AccelerationY);
                txtZAxis.Text = String.Format("{0,5:0.00}", reading.AccelerationZ);

            });
        }



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

                // Assign an event handler for the reading-changed event
                _accelerometer.ReadingChanged += new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
            }
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

#if OFFLINE_SYNC_ENABLED
            await InitLocalStoreAsync(); // offline sync
#endif
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
            base.OnNavigatedTo(e);

            vm = new MyViewModel();
            DataContext = vm;

        }

        private void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (this.Frame.CanGoBack) this.Frame.GoBack();
        }



        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            vm.Start();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            vm.Stop();
        }



    }
}
