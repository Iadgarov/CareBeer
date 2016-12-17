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

#if OFFLINE_SYNC_ENABLED
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;  // offline sync
using Microsoft.WindowsAzure.MobileServices.Sync;         // offline sync
#endif

namespace Knurd
{
    public sealed partial class MainPage : Page
    {

        private MobileServiceCollection<User, User> users;
#if OFFLINE_SYNC_ENABLED
        private IMobileServiceSyncTable<TodoItem> todoTable = App.MobileService.GetSyncTable<TodoItem>(); // offline sync
#else
        private IMobileServiceTable<User> userTable = App.MobileService.GetTable<User>();
#endif

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
#if OFFLINE_SYNC_ENABLED
            await InitLocalStoreAsync(); // offline sync
#endif
           

            checkLogin();


        }

        private void checkLogin()
        {
            var provider = MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory;
            PasswordVault vault = new PasswordVault();
            PasswordCredential credential = null;
            bool loggedIn = false;
            try
            {
                // Try to get an existing credential from the vault.
                credential = vault.FindAllByResource(provider.ToString()).FirstOrDefault();
            }
            catch (Exception)
            {
                // When there is no matching resource an error occurs, which we ignore.
            }
            if (credential != null)
            {
                loggedIn = true;
                MainPage.user = new MobileServiceUser(credential.UserName);
            }
            updateButtons(loggedIn);

        }

        private void updateButtons(bool loggedIn)
        {
            if (loggedIn)
            {
                login.IsEnabled = false;
                logout.IsEnabled = true;

                marker.Content = "Logged in as " + MainPage.user.UserId;
            }
            else
            {
                login.IsEnabled = true; ;
                logout.IsEnabled = false;

                marker.Content = "Please log in";
            }
        }



        // Define a method that performs the authentication process
        private async System.Threading.Tasks.Task<bool> AuthenticateAsync()
        {
            string message;
            bool success = false;

            // This sample uses the Facebook provider.
            var provider = MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory;

            // Use the PasswordVault to securely store and access credentials.
            PasswordVault vault = new PasswordVault();
            PasswordCredential credential = null;


            if (MainPage.user != null)
            {
               
                credential.RetrievePassword();
                MainPage.user.MobileServiceAuthenticationToken = credential.Password;

                // Set the user from the stored credentials.
                App.MobileService.CurrentUser = MainPage.user;

                // Consider adding a check to determine if the token is 
                // expired, as shown in this post: http://aka.ms/jww5vp.

                success = true;
                message = string.Format("Cached credentials for user - {0}", MainPage.user.UserId);
            }
            else
            {
                try
                {
                    // Login with the identity provider.
                    MainPage.user = await App.MobileService
                        .LoginAsync(provider);

                    // Create and store the user credentials.
                    credential = new PasswordCredential(provider.ToString(),
                        MainPage.user.UserId, MainPage.user.MobileServiceAuthenticationToken);
                    vault.Add(credential);

                    success = true;
                    message = string.Format("You are now logged in - {0}", MainPage.user.UserId);
                }
                catch (MobileServiceInvalidOperationException)
                {
                    message = "You must log in. Login Required";
                }
            }

            var dialog = new MessageDialog(message);
            dialog.Commands.Add(new UICommand("OK"));
            await dialog.ShowAsync();

            return success;
        }

        #region Offline sync
#if OFFLINE_SYNC_ENABLED
        private async Task InitLocalStoreAsync()
        {
           if (!App.MobileService.SyncContext.IsInitialized)
           {
               var store = new MobileServiceSQLiteStore("localstore.db");
               store.DefineTable<TodoItem>();
               await App.MobileService.SyncContext.InitializeAsync(store);
           }

           await SyncAsync();
        }

        private async Task SyncAsync()
        {
           await App.MobileService.SyncContext.PushAsync();
           await todoTable.PullAsync("todoItems", todoTable.CreateQuery());
        }
#endif
        #endregion

        private async void DemandLogin(object sender, RoutedEventArgs e)
        {
            await(AuthenticateAsync());
            checkLogin();
        }

        private void logout_Click(object sender, RoutedEventArgs e)
        {
            MainPage.user = null;
            updateButtons(false);
        }

        private void reactionGameButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void accGraphButton_Click(object sender, RoutedEventArgs e)
        {
         
            this.Frame.Navigate(typeof(AccelerometerPage));
        }
    }
}
