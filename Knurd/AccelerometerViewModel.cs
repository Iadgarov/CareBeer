using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;

namespace Knurd
{
    public class AccelerometerViewModel
    {
        private Timer accelerometer;


        private ObservableCollection<AccelerometerReading> data;
       
        public ObservableCollection<AccelerometerReading> stepMoments;

        private List<double> energy; // energy values at every accelerometer sample point
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
        private const double peakThreshold = 1.2; //value was chosen for walking test with phone in hand. TODO - calibrate for pocket. 

        private double mean = -1;


        // Sensor and dispatcher variables
        private Accelerometer _accelerometer;

        public AccelerometerViewModel()
        {
            data = new ObservableCollection<AccelerometerReading>();
            energy = new List<double>();
            smoothData = new List<double>();


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

            accelerometer = new Timer(AccelDataCallback, null, 100, samplePeriod); // last parameter is how often to take measurment, in mSec
        }

        public void Stop()
        {

            accelerometer.Dispose();

            dataToEnergy();
            smooth(energy);
            toChuncks(smoothData);
            isolatePeaks();
            getStrideLength();
            getStepEnergy();
            //writeDataToFile();
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

        public async void dummyValueSmooth()
        {

            string s = "";
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///AccelerometerEnergyData.txt"));
            using (var inputStream = await file.OpenReadAsync())
            using (var classicStream = inputStream.AsStreamForRead())
            using (var streamReader = new StreamReader(classicStream))
            {
                while (streamReader.Peek() >= 0)
                {
                   s += streamReader.ReadLine();
                }
            }
               
            energy = s.Split(';')[0].Split(',').Select(double.Parse).ToList();
            //smoothData = s.Split(';')[1].Split(',').Select(double.Parse).ToList();

            dataToEnergy();
            smooth(energy);
            toChuncks(smoothData);
            isolatePeaks();
            getStrideLength();
            getStepEnergy();


            strideLenghts.Add(1);
            foreach (double d in strideLenghts)
                Debug.Write(d + ", ");

            Debug.WriteLine("stride length mean in sample count. Sample every 10 mSec: " + strideLenghts.Mean());
            Debug.WriteLine("stride length variance: " + strideLenghts.Variance());
            
            Debug.WriteLine("step energy mean: " + stepEnergy.Mean());
            Debug.WriteLine("step energy variance: " + stepEnergy.Variance());

            writeDataToFile();
            
        }

        private void dataToEnergy()
        {
            foreach (AccelerometerReading r in data)
            {

                energy.Add(Math.Pow(Math.Pow(r.AccelerationX, 2) + Math.Pow(r.AccelerationY, 2) + Math.Pow(r.AccelerationZ, 2), 0.5));

            }
        }

        private async void writeDataToFile()
        {
            

            string writeMe = "";
            bool temp = false;
            foreach (double x in energy)
            {

                writeMe += (!temp ? "" : ", ") + x.ToString();
                temp = true;
            }

            writeMe += ";";
            temp = false;
            foreach (double x in smoothData)
            {

                writeMe += (!temp ? "" : ", ") + x.ToString();
                temp = true;
            }

            /*
            writeMe += ";";
            temp = false;
            for (int i = 0; i < windowSize-2; i++)
            {
                writeMe += (!temp ? "" : ", ") + "0";
                temp = true;
            }
            foreach (var nw in low.Zip(high, Tuple.Create))
            {
                foreach (double x in nw.Item1)
                {
                    writeMe += (!temp ? "" : ", ") + "0";
                }
                
                foreach (double x in nw.Item2)
                {
                    writeMe += (!temp ? "" : ", ") + x.ToString();
                }


                
            }
            */
            writeMe += ";";
            temp = false;
            foreach (double x in max_points)
            {

                writeMe += (!temp ? "" : ", ") + x.ToString();
                temp = true;
            }

            writeMe += ";";
            temp = false;
            foreach (double x in min_points)
            {

                writeMe += (!temp ? "" : ", ") + x.ToString();
                temp = true;
            }

            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;

            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
            savePicker.SuggestedFileName = "AccelerometerEnergyData";

            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                // write to file
                await Windows.Storage.FileIO.WriteTextAsync(file, writeMe);
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);

            }




        }

        private async void AccelDataCallback(object state)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    if (_accelerometer != null)
                    {
                        data.Add(_accelerometer.GetCurrentReading());
                    }
                });
        }


        // takes accelerometer data at this moment (x,y,z) and returns the energy (root of sum of a^2 for a= x,y,z)
        private double getAccelerationEnergy()
        {
            AccelerometerReading reading = _accelerometer.GetCurrentReading();
            double temp = Math.Pow(Math.Pow(reading.AccelerationX, 2) + Math.Pow(reading.AccelerationY, 2) + Math.Pow(reading.AccelerationZ, 2), 0.5);
            return temp;

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

                _l.Add(energy.ElementAt(i));
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
                if (l.Max() >= peakThreshold)
                    findThese_max.Add(l.Max());              
            }


            foreach (List<double> l in low)
            {
                if (l.Min() <= mean - 0.1 )
                    findThese_min.Add(l.Min());
            }

            Debug.WriteLine("max points amount: " + findThese_max.Count);
            foreach(double d in energy)
            {
                if (findThese_max.Contains(d))
                {
                    max_points.Add(d);
                }
                else
                {
                    max_points.Add(0);
                }


                if (findThese_min.Contains(d))
                {
                    min_points.Add(d);
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





    }
}
