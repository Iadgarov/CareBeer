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
using Microsoft.WindowsAzure.Storage.Table;


namespace CareBeer
{
    public sealed partial class EntryPage : Page
    {


        // Define a member variable for storing the signed-in user. 
        public static User user = null; // holds the current user


        public EntryPage()
        {
            this.InitializeComponent();
            createTable();


        }

        private async Task<bool> createTable()
        {

            ContentDialog d = new ContentDialog();
            ProgressRing p = new ProgressRing();

            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = "\n\n\nConnecting to cloud. Please wait." });
            p.IsActive = true;
            panel.Children.Add(p);
            d.Content = panel;
            d.ShowAsync();
            

            bool b = await CloudServices.createTableStorage();
            d.Hide();
            if (!b)
            {
                cloudErrorMessage();
            }
            return b;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs args)
        {
            
            User tmp = args.Parameter as User;
            if (tmp != null)
            {
                TableResult result = await CloudServices.retrieveEntity(tmp);

                user = (User)(result.Result);
                if (user != null)
                {
                    Frame.Navigate(typeof(MainPage));

                }
                
                
            }
             
        }


        private async void  go_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameIn.Text;
            string pass = passwordIn.Password;

            TableResult result = await CloudServices.retrieveEntity(new User(username, pass));
            
            if (result.Result == null)
            {
                badLoginMessage();
                return;

            }
            user = (User)(result.Result);
            this.Frame.Navigate(typeof(MainPage));

        }

        private async void badLoginMessage()
        {
            MessageDialog m = new MessageDialog("Login failed.\n Incorrect password.");
            m.Commands.Add(new UICommand("OK"));
            
            await m.ShowAsync();


        }

        private async void register_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameIn.Text;
            string pass = passwordIn.Password;

            TableResult result = await CloudServices.retrieveEntity(new User(username, pass));

            if (result.Result != null)
            {
                badRegisterMessage();
                return;

            }

            await CloudServices.insertEntity(new User(username, pass));
            result = await CloudServices.retrieveEntity(new User(username, pass));
            
            if (result.Result == null)
            {
                Debug.WriteLine("error in cloud serivce, new user not inserted into table");
            }

            user = (User)(result.Result);
            this.Frame.Navigate(typeof(MainPage));
        }

        private async void badRegisterMessage()
        {
            MessageDialog m = new MessageDialog("Registration failed.\n Username unavailable.");
            m.Commands.Add(new UICommand("OK"));
            await m.ShowAsync();


        }

        private async void cloudErrorMessage()
        {
            MessageDialog m = new MessageDialog("Could not connect to cloud services. Please check your internet connection");
            m.Commands.Add(new UICommand("Try Again"));
            var r = await m.ShowAsync();
            if (r.Label == "Try Again")
            {
                createTable();
            }

        }


        private async void toFile_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameIn.Text;
            string pass = passwordIn.Password;

            await CloudServices.userParametersIntoFile(new CareBeer.User(username, pass), "bubble");
        }



    }
}
