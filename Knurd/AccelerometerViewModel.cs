using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.Devices.Sensors;

using Windows.UI.Core;

namespace CareBeer
{
    public class AccelerometerViewModel
    {
        private Timer accelerometer;


        private ObservableCollection<AccelerometerReading> acc_data;
        private ObservableCollection<GyrometerReading> gyr_data;

        //public ObservableCollection<AccelerometerReading> stepMoments;

        private List<double> acc_energy; // energy values at every accelerometer sample point
        private List<double> gyr_energy; // energy values at every accelerometer sample point
        private List<double> smoothData; // energy data after smoothing
        private List<List<double>> high; // smooth data in chuncks with values above avg
        private List<List<double>> low; // smooth data in chuncks with values bellow avg
        private List<double> max_points; // peaks in energy, one per step, rest of values are 0
        private List<double> min_points; // peaks in energy, one per step, rest of values are 0
        private List<double> strideLenghts; // length in sample counts
        private List<Tuple<double, double>> stepPkPk; // peak to pleak for steps (min, max). 
        private List<double> stepEnergy; // the peak values alone, none of the other samples.  

        private int samplePeriod = 10; // mSec

        private const int topSamples = 20; //choosing the number of top samples in accelerometrt
        private const int windowSize = 12; //determine window's size for smoothing data calculation
        private const double peakThreshold = 1.1; //value was chosen for walking test with phone in hand. TODO - calibrate for pocket. 
        private const double noiseThreshold = 0.1; //changes below this will be dismissed. 

        private double mean = -1;

        public bool running = false; 

        private static Mutex mutx = new Mutex();

        // Sensor and dispatcher variables
        private Accelerometer _accelerometer;
        private Gyrometer _gyrometer;

        public AccelerometerViewModel()
        {
            
            acc_data = new ObservableCollection<AccelerometerReading>();
            acc_energy = new List<double>();
            smoothData = new List<double>();

            gyr_data = new ObservableCollection<GyrometerReading>();
            gyr_energy = new List<double>();


            _accelerometer = Accelerometer.GetDefault();

            if (_accelerometer != null)
            {
                // Establish the report interval
                uint minReportInterval = _accelerometer.MinimumReportInterval;
                uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _accelerometer.ReportInterval = reportInterval;
            }

            _gyrometer = Gyrometer.GetDefault();

            if (_gyrometer != null)
            {
                // Establish the report interval
                uint minReportInterval = _gyrometer.MinimumReportInterval;
                uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _gyrometer.ReportInterval = reportInterval;
            }

        }

        public void Start()
        {
            mutx.WaitOne();
            if (running)
            {
                mutx.ReleaseMutex();
                return;
            }
            
            running = true;
            mutx.ReleaseMutex();

            accelerometer = new Timer(collectData, null, 100, samplePeriod); // last parameter is how often to take measurment, in mSec
           
        }

        // premature is true if leaving page before test completion. Uses for debug. Back nvigation should not be possible from test in final product
        public bool Stop(bool premature)
        {
            mutx.WaitOne();
            if (!running)
            {
                mutx.ReleaseMutex();
                return false; // failed stop, already stopped
            }

            running = false;
            mutx.ReleaseMutex();

            accelerometer.Dispose();

            if (premature)
                return true;

            dataToEnergy();
            smooth(acc_energy);
            toChuncks(smoothData);
            isolatePeaks();
            getStrideLength();
            getStepEnergy();

            Debug.WriteLine(acc_energy.Count);

            updateUser();
            return true;
            
            
        }

        public int getStepCount()
        {
            return stepEnergy.Count;
        }


        public double getStrideLengthVariance()
        {
            return strideLenghts.Variance();
        }


        public double getStepEnergyVariance()
        {
            return stepEnergy.Variance();
        }

      

        private void dataToEnergy()
        {
            foreach (AccelerometerReading r in acc_data)
            {
                acc_energy.Add(Math.Pow(Math.Pow(r.AccelerationX, 2) + Math.Pow(r.AccelerationY, 2) + Math.Pow(r.AccelerationZ, 2), 0.5));
            }

            foreach (GyrometerReading r in gyr_data)
            {
                gyr_energy.Add(Math.Pow(Math.Pow(r.AngularVelocityX, 2) + Math.Pow(r.AngularVelocityY, 2) + Math.Pow(r.AngularVelocityZ, 2), 0.5));
            }
            //Debug.WriteLine("byte count: " + System.Text.ASCIIEncoding.ASCII.GetByteCount(User.listToString(acc_energy)));
        }


        
        private async void collectData(object state)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    if (_accelerometer != null && _gyrometer != null)
                    {
                        acc_data.Add(_accelerometer.GetCurrentReading());
                        gyr_data.Add(_gyrometer.GetCurrentReading());
                        
                        if (acc_data.Count >= 4000)
                        {
                            this.Stop(false);
                           
                            AccelerometerPage.current.enoughDataMessage();
                        }
                    }
                });
        }



        //Using moving average to create smoother data(from the data collected by the accel.)
        public void smooth(List<double> from)
        {
                
                int i = 0;


            //for the first window- its first half remains the same
            for (i = 0; i < windowSize / 2; i++)
                addToMyAccelModelCollection(smoothData, 0);// from.ElementAt(i));

            for (i = windowSize / 2; i < (from.Count) - windowSize / 2; i++)
            {
                //don't touch extreme points (which propably represent the step)
                if (from.ElementAt(i) > peakThreshold || from.ElementAt(i) < peakThreshold - 0.5)
                {
                    addToMyAccelModelCollection(smoothData, from.ElementAt(i));
                }

                else
                {
                    movingAverage(i, from);
                }
            }

            //for the last window- its second half remains the same
            for (i = (from.Count) - windowSize / 2; i < from.Count; i++)
                addToMyAccelModelCollection(smoothData, 0);// from.ElementAt(i));
            
                
        }

        
        private void movingAverage(int index, List<double> from)
        {
            double mean = 0;
            for (int j = -windowSize / 2; j < windowSize / 2; j++)
                mean += from.ElementAt(index + j);

            addToMyAccelModelCollection(smoothData, mean / windowSize);

        }


        //adds new item to the given MyAccelModel collection
        private void addToMyAccelModelCollection(List<double> col, double accelEnergy)
        {
            col.Add(accelEnergy);
        }


        // takes the smoothed dataset, breaks it into chuncks, each chucnks is a step. 
        private void toChuncks(List<double> l)
        {
            double avg = l.Average();
            mean = avg;

            high = new List<List<double>>(); // sections that are above the avg
            low = new List<List<double>>(); // setions below the avg

            
            bool newChunk = true;
            bool above = false;
            List<double> _l = new List<double>();


            int i = windowSize / 2;

            
            // skip the inital section of points
            
            if (l.ElementAt(i) < avg)
            {
                /*
                while (l.ElementAt(i) <= avg)
                {
                    i++;
                }*/
                above = true;
            }
            else
            {
                /*while (l.ElementAt(i) > avg)
                {
                    i++;
                }*/
                above = false;
            }
            
            above = !above; // flip value for proper work of below loop (pretend we're coming from aprevious chunck). 



            for (; i < (l.Count) - windowSize / 2; i++)
            {
                if (l.ElementAt(i) <= avg)
                {
                    newChunk = (above == true); // if we were above before this is a new chunck (we are now bellow)
                    above = false;
                    
                }
                else
                {
                    newChunk = (above == false); // if we were below before this is a new chunck. 
                    above = true;
                    
                }

                if (newChunk)
                {
                    _l = new List<double>();
                    if (above)
                        high.Add(_l);
                    else
                        low.Add(_l);
                }

                _l.Add(acc_energy.ElementAt(i));
            }

            Debug.WriteLine("high len: "+ high.Count);
            Debug.WriteLine("low len: " + low.Count);

        }

        // find peaks from the high array, create a list with zeros for all other points in energy other than these peaks
        private void isolatePeaks()
        {

            max_points = new List<double>();
            min_points = new List<double>();

            List<double> findThese_max = new List<double>();
            List<double> findThese_min = new List<double>();


            foreach (List<double> l in high)
            {
                if (Math.Abs(l.Max() - mean) >= noiseThreshold)
                    findThese_max.Add(l.Max());              
            }


            foreach (List<double> l in low)
            {
                if (Math.Abs(l.Min() - mean) >= noiseThreshold)
                    findThese_min.Add(l.Min());
            }

            Debug.WriteLine("max points amount: " + findThese_max.Count);
            string last = ""; // helps us skip double min/max values 
            foreach(double d in acc_energy)
            {
                if (findThese_max.Contains(d) && last != "max")
                {
                    
                    max_points.Add(d);
                    last = "max";
                    
                }
                else
                {
                    max_points.Add(0);
                }


                if (findThese_min.Contains(d) && last != "min")
                {
                    
                    min_points.Add(d);
                    last = "min";
                }
                else
                {
                    min_points.Add(0);
                }

            }

        }

        // goes over peak array, count number of samples between each peak. 
        private void getStrideLength()
        {

            int gap = 0;
            bool start = false;
            strideLenghts = new List<double>();

            for (int i = 0; i < max_points.Count; i++)
            {
                if (max_points.ElementAt(i) > 0)
                {
                    if (start)
                    {
                        strideLenghts.Add(gap);
                    }
                    gap = 0;
                    start = true;
                }
                gap++;
            }

        }

        private void getStepEnergy()
        {
            stepEnergy = new List<double>();
            stepPkPk = new List<Tuple<double, double>>();

            List<double> pureMin = min_points.Where(i => i != 0).ToList();
            List<double> pureMax = max_points.Where(i => i != 0).ToList();

            // make sure we have at least as many mins as maxs 
            while (pureMin.Count < pureMax.Count)
            {
                pureMax.RemoveAt(pureMax.Count - 1);
            }

            var zipped = pureMin.Zip(pureMax, (first, second) => new Tuple<double, double>(first, second));
            foreach (Tuple<double, double> t in zipped)
            {
                stepPkPk.Add(t);
            }

            for (int i = 0; i < stepPkPk.Count; i++)
            {
                if (stepPkPk.ElementAt(i).Item2 > 0)
                {
                    stepEnergy.Add(stepPkPk.ElementAt(i).Item2 - stepPkPk.ElementAt(i).Item1);
                    Debug.Write(stepPkPk.ElementAt(i).ToString());
                }
                
            }

        }

        private void updateUser()
        {
            User u = EntryPage.user;
            if (u.step_baslineExists)
            {
                u.acc_energyList = User.listToString(acc_energy);
                u.gyr_energyList = User.listToString(gyr_energy);
                u.maxPoints = User.listToString(max_points);
                u.minPoints = User.listToString(min_points);
                u.stepAmplitude = User.listToString(stepEnergy);
                u.strideLength = User.listToString(strideLenghts);
                u.strideLengthVariance = getStrideLengthVariance();
            }
            else
            {
                u.B_acc_energyList = User.listToString(acc_energy);
                u.B_gyr_energyList = User.listToString(gyr_energy);
                u.B_maxPoints = User.listToString(max_points);
                u.B_minPoints = User.listToString(min_points);
                u.B_stepAmplitude = User.listToString(stepEnergy);
                u.B_strideLength = User.listToString(strideLenghts);
                u.B_strideLengthVariance = getStrideLengthVariance();

                u.step_baslineExists = true;
            }
        }



    }
}
