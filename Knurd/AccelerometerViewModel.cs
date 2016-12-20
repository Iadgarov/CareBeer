using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.UI.Core;

namespace Knurd
{
    public class AccelerometerViewModel
    {
        private Timer accelerometer;

        private Random r;

        private ObservableCollection<MyAccelModel> data;
        public ObservableCollection<MyAccelModel> Data { get { return data; }  set { } }


        // Sensor and dispatcher variables
        private Accelerometer _accelerometer;

        public AccelerometerViewModel()
        {
            data = new ObservableCollection<MyAccelModel>();

            r = new Random(DateTime.Now.Millisecond);

            _accelerometer = Accelerometer.GetDefault();

            if (_accelerometer != null)
            {
                // Establish the report interval
                uint minReportInterval = _accelerometer.MinimumReportInterval;
                uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _accelerometer.ReportInterval = reportInterval;

                // Assign an event handler for the reading-changed event
                //_accelerometer.ReadingChanged += new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
            }
        }

        public void Start()
        {
            accelerometer = new Timer(AccelDataCallback, null, 100, 50); // last parameter is how often to take measurment, in mSec
        }

        public void Stop()
        {
            accelerometer.Dispose();
        }

        private async void AccelDataCallback(object state)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    if (_accelerometer != null)
                    {
                        AccelerometerReading reading = _accelerometer.GetCurrentReading();

              


                        data.Add(new MyAccelModel {
                            TimeStamp = DateTime.Now,
                            Accel = getAccelerationEnergy()
                        });
                    }
                });
        }

        private double getAccelerationEnergy()
        {
            AccelerometerReading reading = _accelerometer.GetCurrentReading();
            double temp = Math.Pow(Math.Pow(reading.AccelerationX, 2) + Math.Pow(reading.AccelerationY, 2) + Math.Pow(reading.AccelerationZ, 2), 0.5);
            Debug.WriteLine(temp);
            return temp;

        }






    }
}
