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
using System.Diagnostics;

namespace Knurd
{
    public sealed partial class MainPage : Page
    {


        //public static MainPage Current;

        public MainPage()
        {
            this.InitializeComponent();
            //Current = this;

            


        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Window.Current.Content = Current.Frame;
        }

        private void setWelcomeText()
        {
            if (EntryPage.user.isSetUp())
            {
                marker.Text = "Welcome";
            }
            else
            {
                marker.Text = "Please create a sober basline by running the app when sober.";
            }
        }

        private async void firstRunMessage()
        {
            MessageDialog m = new MessageDialog("This is the first use of the app by this user. Please complete the following tests when sober to create your basline.");
            m.Commands.Add(new UICommand("OK"));
            m.Commands.Add(new UICommand("No Thanks"));
            var r = await m.ShowAsync();
            if (r.Label == "OK")
            {
                // begin tests
            }

        }


        private void reactionGameButton_Click(object sender, RoutedEventArgs e)
        {
           
                this.Frame.Navigate(typeof(ReactionPage));
           
        }

        private void accGraphButton_Click(object sender, RoutedEventArgs e)
        {

          
                this.Frame.Navigate(typeof(AccelerometerPage));
         
        }

        private void changeUser_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(EntryPage));

        }

        private void begin_Click(object sender, RoutedEventArgs e)
        {
            if (!EntryPage.user.isSetUp())
            {
                firstRunMessage();
            }
            else
            {
                // begin the tests
            }
        }
    }
}
