

using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Linq;

using Windows.UI.Xaml.Media;
using Windows.UI;
using System.Diagnostics;
using System.Collections.Generic;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.UI.Core;
//using System.Threading;

namespace Knurd
{
    public sealed partial class BubblePage : Page
    {

        List<AccelerometerReading> acc_data;
        List<GyrometerReading> gyr_data;
        List<double> acc_energy;
        List<double> gyr_energy;

        Gyrometer _gyrometer;
        Accelerometer _accelerometer;

        private int time = 0;
        private const int done = 30;
        DispatcherTimer t;
   

        public BubblePage()
        {

            this.InitializeComponent();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            

            

            beginMessage();

            
            
        }

        private void begin()
        {

            acc_energy = new List<double>();
            gyr_energy = new List<double>();
            time = 0;

            _gyrometer = Gyrometer.GetDefault();
            if (_gyrometer != null)
            {
                _gyrometer.ReportInterval = (uint)(_gyrometer.MinimumReportInterval * 2); // times two b/c we can't handle too much data on azure
                _gyrometer.ReadingChanged += new TypedEventHandler<Gyrometer, GyrometerReadingChangedEventArgs>(ReadingChanged);
            }

            _accelerometer = Accelerometer.GetDefault();
            if (_accelerometer != null)
            {
                _accelerometer.ReportInterval = _accelerometer.MinimumReportInterval;
                _accelerometer.ReadingChanged += new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
            }
        }


        
        async private void ReadingChanged(object sender, GyrometerReadingChangedEventArgs e)
        {
            
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var r = e.Reading;
                var ar = _accelerometer.GetCurrentReading();
                double gy = Math.Pow(Math.Pow(r.AngularVelocityX, 2) + Math.Pow(r.AngularVelocityY, 2) + Math.Pow(r.AngularVelocityZ, 2), 0.5);

                double gyroWeight = 10;
                moveTransform.X = ar.AccelerationX * (400 + gy * gyroWeight);
                moveTransform.Y = -1 * ar.AccelerationY * (400 + gy * gyroWeight);

                //gyr_data.Add(r);
                gyr_energy.Add(gy);
            });
        }


        async private void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var r = e.Reading;
                var gr = _gyrometer.GetCurrentReading();
                double gy = Math.Pow(Math.Pow(gr.AngularVelocityX, 2) + Math.Pow(gr.AngularVelocityY, 2) + Math.Pow(gr.AngularVelocityZ, 2), 0.5);

                double gyroWeight = 10;
                moveTransform.X = r.AccelerationX * (400 + gy * gyroWeight);
                moveTransform.Y = -1 * r.AccelerationY * (400 + gy * gyroWeight);

                //acc_data.Add(r);
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
                this.Frame.Navigate(typeof(MainPage));
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
            t = new DispatcherTimer();
            t.Tick += tickTock;
            t.Interval = new TimeSpan(0, 0, 1);
            t.Start();
            begin();


        }

        private async void tickTock(object sender, object e)
        {
            time++;
            timer.Text = ((int) (time / 60)).ToString("00") + ":" + (time % 60).ToString("00");

            if (time == done)
            {
                t.Stop();
                _gyrometer.ReadingChanged += null;
                _accelerometer.ReadingChanged += null;

                Debug.WriteLine("gyr len: " + gyr_energy.Count);
                Debug.WriteLine("acc len: " + acc_energy.Count);
                updateUser();
                await CloudServices.replaceIneEntity(EntryPage.user);
                summaryMessage();
            }
        }



        private void toggleButtonEnable()
        {
            button1.IsEnabled = !button1.IsEnabled;
            
        }

       

     

        private void updateUser()
        {
            User u = EntryPage.user;
            if (u.bubble_baslineExists)
            {
                u.acc_bubble_energy = User.listToString(acc_energy);
                u.gyr_bubble_energy = User.listToString(gyr_energy);
                
            }
            else
            {
                u.B_acc_bubble_energy = User.listToString(acc_energy);
                u.B_gyr_bubble_energy = User.listToString(gyr_energy);
                
                u.bubble_baslineExists = true;
            }
        }

    }
}
