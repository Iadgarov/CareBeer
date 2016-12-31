﻿/*
 * To add Offline Sync Support:
 *  1) Add the NuGet package Microsoft.Azure.Mobile.Client.SQLiteStore (and dependencies) to all client projects
 *  2) Uncomment the #define OFFLINE_SYNC_ENABLED
 *
 * For more information, see: http://go.microsoft.com/fwlink/?LinkId=717898
 */
//#define OFFLINE_SYNC_ENABLED

using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Linq;
using Windows.Security.Credentials;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System.Diagnostics;
using System.Collections.Generic;

#if OFFLINE_SYNC_ENABLED
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;  // offline sync
using Microsoft.WindowsAzure.MobileServices.Sync;         // offline sync
#endif

namespace Knurd
{
    public sealed partial class ReactionPage : Page
    {

        Stopwatch sw;
        Random rand;
        int counter;
        List<ReactionData> data; // This will contain data and be saved to cloud for sober case. 
        private Button expected;

        const int FLASH_AMOUNT = 5;

        public ReactionPage()
        {

            this.InitializeComponent();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
#if OFFLINE_SYNC_ENABLED
            await InitLocalStoreAsync(); // offline sync
#endif

            beginMessage();

            counter = 0;    // countes thenumber of flashes done
            sw = new Stopwatch();
            rand = new Random();

        }

        private async void flash()
        {
            toggleButtonEnable();
            if (counter < FLASH_AMOUNT)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(rand.Next(500, 2000))); // delay 0.5 sec to 2
                switch (rand.Next(1, 3))
                {
                    case 1: flashButton(button1); expected = button1; break;
                    case 2: flashButton(button2); expected = button2; break;
                    
                }
            }
            counter++;
            toggleButtonEnable();

        }


        private async void beginMessage()
        {
            MessageDialog m = new MessageDialog("Ready?");
            m.Commands.Add(new UICommand("Yes!"));
            await m.ShowAsync();
            flash();


        }
        
        private void toggleButtonEnable()
        {
            button1.IsEnabled = !button1.IsEnabled;
            button2.IsEnabled = !button2.IsEnabled;
            
        }

        private void buttonClick(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            string name = b.Name;

            /**
            ReactionData d = new ReactionData();
            d.rTime = sw.Elapsed;
            d.isRight = b.Equals(expected);
            data.Add(d);
            */
            switch (name)
            {
                case "button1":
                    sw.Stop();
                    Debug.WriteLine("Button1 Chosen: " + sw.Elapsed);

                    break;

                case "button2":
                    sw.Stop();
                    Debug.WriteLine("Button2 Chosen: " + sw.Elapsed);
                    break;


            }
            sw.Reset();
            flash();

        }

        private const int FLASH_DURATION = 100; // in millieseconds 
        private async void screenFlash(byte a, byte r, byte g, byte b)
        {
            sw.Start();
            screen.Background = new SolidColorBrush(Color.FromArgb(a, r, g, b));
            await Task.Delay(TimeSpan.FromMilliseconds(FLASH_DURATION));
            screen.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
        }

        private void flashButton(Button b)
        {
            var temp = b.Background as SolidColorBrush;
            Debug.WriteLine("flashing " + b.Name + ". color: " + temp.Color.ToString());
            screenFlash(temp.Color.A, temp.Color.R, temp.Color.G, temp.Color.B);
            
        }

        private int averageReactionTime(List<ReactionData> d)
        {
            int temp = 0; 
            foreach (var t in d)
            {
                temp += t.rTime.Milliseconds;
            }
            temp /= data.Count();

            return temp;
        }

        private double varianceReactionTime(List<ReactionData> d)
        {
            double temp = 0;
            int mean = averageReactionTime(d);

            foreach (var t in d)
            {
                temp += Math.Pow(t.rTime.Milliseconds - mean, 2);
            }
            temp /= data.Count() - 1;

            return temp;
        }

        private int mistakes()
        {
            int temp = 0;
            foreach (var t in data)
            {
                if (!t.isRight)
                {
                    temp++;
                }
            }
            return temp;
        }


        private void getSoberData()
        {
            //TODO: method that gets list of data from cloud to be compared to. 
        }





        private class ReactionData
        {
            public bool isRight { get; set; }
            public TimeSpan rTime { get; set; }

        }

        
    }
}