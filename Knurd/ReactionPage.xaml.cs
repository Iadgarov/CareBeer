﻿

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
using CareBeer.Tests.ReactionTime;
using Windows.UI.Core;

namespace CareBeer
{
    public sealed partial class ReactionPage : Page
    {

        Stopwatch sw;
        Random rand;
        int counter;
        //List<ReactionData> data; // This will contain data and be saved to cloud for sober case. 
        private Button expected;

		ReactionTimeTest tester;

        const int FLASH_AMOUNT = 8;

        public ReactionPage()
        {

            this.InitializeComponent();

        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            Frame.BackStack.Clear();

            tester = args.Parameter as ReactionTimeTest;
			reset();

        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Frame.BackStack.Clear();
        }


        private void reset()
        {
            beginMessage();
            button1.IsEnabled = true;   button1.Content = "";
            button2.IsEnabled = true;   button2.Content = "";
            counter = 0;    // countes thenumber of flashes done
            sw = new Stopwatch();
            rand = new Random();
			tester.Data.Clear();
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
            else
            {
                button1.Content = "Done!";
                button2.Content = "Done!";
                button1.IsEnabled = false;
                button2.IsEnabled = false;

				tester.CalculateResult();
                //updateUser();
                //await CloudServices.replaceIneEntity(EntryPage.user);
                summaryMessage();

                return;

            }
            counter++;
            toggleButtonEnable();

        }

        private async void summaryMessage()
        {
			string s = "";
            s += "Your average reaction time was: " + (tester.ReactionTimeMean / 1000) + " seconds\n";
            s += "You made " + tester.Mistakes + " mistake(s)";


            MessageDialog m = new MessageDialog(s);
			m.Title = "Results";
            m.Commands.Add(new UICommand("Next"));
            m.Commands.Add(new UICommand("Redo"));


            var r = await m.ShowAsync();
            if (r == null)
            {
                return;
            }

            if (r.Label == "Next")
            {
				tester.Finished();
				//this.Frame.Navigate(typeof(AccelerometerPage));
            }
            else if (r.Label == "Redo")
            {
                reset();
            }

          
            

        }


        private async void beginMessage()
        {
            MessageDialog m = new MessageDialog("This is a reaction time test which includes a correctness factor.\n" +
                "When the screen flashes a certain color, you must choose the correct button quickly. Ready?");
            m.Commands.Add(new UICommand("Yes sir!"));
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

            sw.Stop();

            Button b = sender as Button;
            string name = b.Name;

            
            ReactionData d = new ReactionData();
            d.rTime = sw.ElapsedMilliseconds;
            d.isRight = b.Equals(expected);
            tester.Data.Add(d);
           
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



        
    }
}
