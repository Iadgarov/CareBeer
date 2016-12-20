/*
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

#if OFFLINE_SYNC_ENABLED
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;  // offline sync
using Microsoft.WindowsAzure.MobileServices.Sync;         // offline sync
#endif

namespace Knurd
{
    public sealed partial class ReactionPage : Page
    {

        Stopwatch sw;
        Random rn;
        int counter;
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
            rn = new Random();

            




        }

        private async void flash()
        {
            toggleButtonEnable();
            if (counter < FLASH_AMOUNT)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(rn.Next(500, 2000))); // delay 0.5 sec to 2
                switch (rn.Next(1, 4))
                {
                    case 1: flashButton(topCircle); break;
                    case 2: flashButton(centerCircle); break;
                    case 3: flashButton(bottomCircle); break;
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
            topCircle.IsEnabled = !topCircle.IsEnabled;
            centerCircle.IsEnabled = !centerCircle.IsEnabled;
            bottomCircle.IsEnabled = !bottomCircle.IsEnabled;
        }

        private void buttonClick(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            string name = b.Name;
            
            switch (name)
            {
                case "topCircle":
                    sw.Stop();
                    Debug.WriteLine("Red Chosen: " + sw.Elapsed);
                    
                    break;

                case "centerCircle":
                    sw.Stop();
                    Debug.WriteLine("Green Chosen: " + sw.Elapsed);
                    break;

                case "bottomCircle":
                    sw.Stop();
                    Debug.WriteLine("Blue Chosen: " + sw.Elapsed);
                    break;

            }
            sw.Reset();
            flash();

        }

        private async void screenFlash(byte a, byte r, byte g, byte b)
        {
            sw.Start();
            screen.Background = new SolidColorBrush(Color.FromArgb(a, r, g, b));
            await Task.Delay(TimeSpan.FromMilliseconds(50));
            screen.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
        }

        private void flashButton(Button b)
        {
            var temp = b.Background as SolidColorBrush;
            Debug.WriteLine("flashing " + temp.Color.ToString());
            screenFlash(temp.Color.A, temp.Color.R, temp.Color.G, temp.Color.B);
            
        }

    }
}
