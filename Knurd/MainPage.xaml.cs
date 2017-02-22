/*
 * To add Offline Sync Support:
 *  1) Add the NuGet package Microsoft.Azure.Mobile.Client.SQLiteStore (and dependencies) to all client projects
 *  2) Uncomment the #define OFFLINE_SYNC_ENABLED
 *
 * For more information, see: http://go.microsoft.com/fwlink/?LinkId=717898
 */
//#define OFFLINE_SYNC_ENABLED

using CareBeer.Tests;
using CareBeer.Tests.ReactionTime;
using System;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;

namespace CareBeer
{
    public sealed partial class MainPage : Page
    {


        //public static MainPage Current;

        public MainPage()
        {
            this.InitializeComponent();
            //Current = this;

            Application.Current.Suspending += new SuspendingEventHandler(App_Suspending);


        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Window.Current.Content = Current.Frame;
            if (!EntryPage.user.isSetUp())
            {
                testListBtn.IsEnabled = false;
            }
            else
            {
                testListBtn.IsEnabled = true;
            }
        }

        private void setWelcomeText()
        {
            if (EntryPage.user.isSetUp())
            {
                marker.Text = "Welcome";
            }
            else
            {
                marker.Text = "Please create a sober baseline by running the app when sober.";
            }
        }

        private async void firstRunMessage()
        {
            MessageDialog m = new MessageDialog("This is the first use of the app by this user. Please complete the following tests when sober to create your basline.");
            m.Commands.Add(new UICommand("OK"));
            m.Commands.Add(new UICommand("Not now"));
            var r = await m.ShowAsync();
            if (r != null && r.Label == "OK")
            {
				TestManager.Instance.TestsToRun = TestId.All;
				//TestManager.Instance.Tests.Add(new ReactionTimeTest());
				TestManager.Instance.Start(true);
				//this.Frame.Navigate(typeof(ReactionPageSingle));
            }

        }


        //private void reactionGameButton_Click(object sender, RoutedEventArgs e)
        //{
        //        this.Frame.Navigate(typeof(ReactionPageSingle));
        //}

        //private void accGraphButton_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Frame.Navigate(typeof(AccelerometerPage));
        //}

        //private void changeUser_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Frame.Navigate(typeof(EntryPage));

        //}

        private void begin_Click(object sender, RoutedEventArgs e)
        {
            if (!EntryPage.user.isSetUp())
            {
                firstRunMessage();
            }
            else
            {
                TestManager.Instance.TestsToRun = TestId.All;
                TestManager.Instance.Start(false);
            }
        }

        //private void settings_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Frame.Navigate(typeof(SettingsPage));
        //}

        private void testListBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(TestSelectorPage));
        }

        private void switchUser_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(EntryPage));
        }

        private void about_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AboutPage), false);
        }


        private void App_Suspending(Object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();
            composite["username"] = EntryPage.user.userName;
            composite["password"] = EntryPage.user.password;

            localSettings.Values["user"] = composite;
        }
    }
}
