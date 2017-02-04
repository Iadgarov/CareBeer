

using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using System.Diagnostics;
using System.Collections.Generic;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.UI.Core;
using CareBeer.Tests;
//using System.Threading;

namespace CareBeer
{
    public sealed partial class BubblePage : Page
    {

        //List<AccelerometerReading> acc_data;
        //List<GyrometerReading> gyr_data;
        List<double> acc_energy;
        List<double> gyr_energy;

        Gyrometer _gyrometer;
        Accelerometer _accelerometer;

        private int time = 0;
        private const int done = 30;
        DispatcherTimer dTimer;

		private BubbleTest tester;
   

        public BubblePage()
        {

            this.InitializeComponent();

        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            Frame.BackStack.Clear();

            tester = args.Parameter as BubbleTest;
            beginMessage();
			begin();        
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            dTimer.Stop();
            if (_gyrometer != null) _gyrometer.ReadingChanged -= ReadingChanged;
            if (_accelerometer != null) _accelerometer.ReadingChanged -= ReadingChanged;
            _accelerometer = null;
            _gyrometer = null;

            Frame.BackStack.Clear();
        }

        private void begin()
        {

            acc_energy = tester.accelEnergy;
			acc_energy.Clear();

            gyr_energy = tester.gyroEnergy;
			gyr_energy.Clear();

            time = 0;

			dTimer = new DispatcherTimer();
			dTimer.Tick += tickTock;
			dTimer.Interval = new TimeSpan(0, 0, 1);
			dTimer.Start();

			_gyrometer = Gyrometer.GetDefault();
            if (_gyrometer != null)
            {
                _gyrometer.ReportInterval = _gyrometer.MinimumReportInterval * 2; // times two b/c we can't handle too much data on azure
                _gyrometer.ReadingChanged += new TypedEventHandler<Gyrometer, GyrometerReadingChangedEventArgs>(ReadingChanged);
            }

            _accelerometer = Accelerometer.GetDefault();
            if (_accelerometer != null)
            {
                _accelerometer.ReportInterval = _accelerometer.MinimumReportInterval;
                _accelerometer.ReadingChanged += new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
            }

            if (_accelerometer == null || _gyrometer == null)
            {
                noGyroMessage();
            }
        }


        private async void noGyroMessage()
        {
            MessageDialog m = new MessageDialog("This device does not have a gyrometer and/or accelerometer!");
            await m.ShowAsync();
            tester.Finished(true);

        }


        async private void ReadingChanged(object sender, GyrometerReadingChangedEventArgs e)
        {
            
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {

                if (_gyrometer == null)
                {
                    return;
                }
                var r = e.Reading;
                var ar = _accelerometer.GetCurrentReading();
                double gy = Math.Pow(Math.Pow(r.AngularVelocityX, 2) + Math.Pow(r.AngularVelocityY, 2) + Math.Pow(r.AngularVelocityZ, 2), 0.5);

                double gyroWeight = 10;
                moveTransform.X = ar.AccelerationX * (400 + gy * gyroWeight);
                moveTransform.Y = -1 * ar.AccelerationY * (400 + gy * gyroWeight);

                //gyr_data.Add(r);
                // give the user time to stabalize
                if (time >= done/3)
                    gyr_energy.Add(gy);
            });
        }


        async private void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {

                if (_accelerometer == null)
                {
                    Debug.WriteLine("!!!!!");
                    return;
                }
                var r = e.Reading;
                var gr = _gyrometer.GetCurrentReading();
                double gy = Math.Pow(Math.Pow(gr.AngularVelocityX, 2) + Math.Pow(gr.AngularVelocityY, 2) + Math.Pow(gr.AngularVelocityZ, 2), 0.5);

                double gyroWeight = 10;
                moveTransform.X = r.AccelerationX * (400 + gy * gyroWeight);
                moveTransform.Y = -1 * r.AccelerationY * (400 + gy * gyroWeight);

                //acc_data.Add(r);
                if (time >= done / 3)
                    acc_energy.Add(Math.Pow(Math.Pow(r.AccelerationX, 2) + Math.Pow(r.AccelerationY, 2) + Math.Pow(r.AccelerationZ, 2), 0.5));

            });
        }


        private async void summaryMessage()
        {
            MessageDialog m = new MessageDialog("Well done.");
            m.Commands.Add(new UICommand("Finish"));
            m.Commands.Add(new UICommand("Redo"));

            IUICommand r = null;
            try
            {
                r = await m.ShowAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }

            if (r == null)
            {
                return;
            }

            if (r.Label == "Finish")
            {
                tester.Finished(false);
            }
            else if (r.Label == "Redo")
            {
                begin(); // reset
            }

        }


        private async void beginMessage()
        {
            MessageDialog m = new MessageDialog("Try to keep the wild dot in the center for " + done + " seconds.");
            m.Commands.Add(new UICommand("Ok"));
            await m.ShowAsync();

        }

        private void tickTock(object sender, object e)
        {
            time++;
            timer.Text = ((int) (time / 60)).ToString("00") + ":" + (time % 60).ToString("00");

            if (time == done)
            {
                dTimer.Stop();

                _gyrometer.ReadingChanged -= ReadingChanged;
                _accelerometer.ReadingChanged -= ReadingChanged;

                Debug.WriteLine("gyr len: " + gyr_energy.Count);
                Debug.WriteLine("acc len: " + acc_energy.Count);

                summaryMessage();
            }
        }



        private void toggleButtonEnable()
        {
            button1.IsEnabled = !button1.IsEnabled;
            
        }
 

    }
}
