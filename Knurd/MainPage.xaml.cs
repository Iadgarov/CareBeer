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



namespace Knurd
{
    public sealed partial class MainPage : Page
    {


        // Define a member variable for storing the signed-in user. 
        public static MobileServiceUser user = null;

        public static MainPage Current;

        public MainPage()
        {
            this.InitializeComponent();

            Current = this;

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

   
        private void reactionGameButton_Click(object sender, RoutedEventArgs e)
        {
           
                this.Frame.Navigate(typeof(ReactionPage));
           
        }

        private void accGraphButton_Click(object sender, RoutedEventArgs e)
        {

          
                this.Frame.Navigate(typeof(AccelerometerPage));
         
        }


    }
}
