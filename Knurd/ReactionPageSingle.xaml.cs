

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



namespace CareBeer
{
    public sealed partial class ReactionPageSingle : Page
    {

        Stopwatch sw;
        Random rand;
        int counter;
        List<ReactionData> data; // This will contain data and be saved to cloud for sober case. 

        const int FLASH_AMOUNT = 3;

        public ReactionPageSingle()
        {

            this.InitializeComponent();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            reset();

        }

        private void reset()
        {
            beginMessage();
            button1.IsEnabled = true;
            button1.Content = "";
            counter = 0;    // countes thenumber of flashes done
            sw = new Stopwatch();
            rand = new Random();
            data = new List<ReactionData>();
        }

        private async void flash()
        {
            toggleButtonEnable();
            if (counter < FLASH_AMOUNT)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(rand.Next(500, 2000))); // delay 0.5 sec to 2
                flashButton(button1);
            }
            else
            {
                button1.Content = "Done!";
                button1.IsEnabled = false;
    
                
                updateUser();
                await CloudServices.replaceIneEntity(EntryPage.user);
                summaryMessage();
                return;

            }
            counter++;
            toggleButtonEnable();

        }

        private async void summaryMessage()
        {
            string s = "Results:\n";
            s += "reaction time mean: " + averageReactionTime(data) + "mSec \n";
            s += "reaction time variance: " + varianceReactionTime(data) + "\n";

            MessageDialog m = new MessageDialog(s);
            m.Commands.Add(new UICommand("Next"));
            m.Commands.Add(new UICommand("Redo"));


            var r = await m.ShowAsync();
            if (r == null)
            {
                return;
            }

            if (r.Label == "Next")
            {
                this.Frame.Navigate(typeof(ReactionPage));
            }
            else if (r.Label == "Redo")
            {
                reset();
            }
            

        }


        private async void beginMessage()
        {
            MessageDialog m = new MessageDialog("When the screen flashes you must press the button. Ready?");
            m.Commands.Add(new UICommand("Yes!"));
            await m.ShowAsync();
            flash();


        }
        
        private void toggleButtonEnable()
        {
            button1.IsEnabled = !button1.IsEnabled;
            
        }

        private void buttonClick(object sender, RoutedEventArgs e)
        {

            sw.Stop();

            Button b = sender as Button;
            string name = b.Name;

            
            ReactionData d = new ReactionData();
            d.rTime = sw.ElapsedMilliseconds;
            d.isRight = b.Equals(button1);
            data.Add(d);
           
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

        private long averageReactionTime(List<ReactionData> d)
        {
            long temp = 0; 
            foreach (var t in d)
            {
                temp += t.rTime;
            }
            temp /= data.Count();

            return temp;
        }

        private double varianceReactionTime(List<ReactionData> d)
        {
            double temp = 0;
            long mean = averageReactionTime(d);
            
            foreach (var t in d)
            {
                temp += Math.Pow(t.rTime - mean, 2);
            }
            temp /= data.Count() - 1;

            return temp;
        }

   
        private void getSoberData()
        {
            //TODO: method that gets list of data from cloud to be compared to. 
        }

        private void updateUser()
        {
            User u = EntryPage.user;
            if (u.reactionSingle_baslineExists)
            {
                u.reactionSingle_mean = averageReactionTime(data);
                u.reactionSingle_variance = varianceReactionTime(data);
            }
            else
            {
                u.B_reactionSingle_mean = averageReactionTime(data);
                u.B_reactionSingle_variance = varianceReactionTime(data);
                u.reactionSingle_baslineExists = true;
            }
        }

        private class ReactionData
        {
            public bool isRight { get; set; }
            public long rTime { get; set; }

        }

        
    }
}
