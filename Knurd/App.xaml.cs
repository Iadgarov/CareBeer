using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CareBeer
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        // This MobileServiceClient has been configured to communicate with the Azure Mobile Service and
        // Azure Gateway using the application key. You're all set to start working with your Mobile Service!
        public static MobileServiceClient MobileService = new MobileServiceClient(
            "https://knurd.azurewebsites.net"
        );



        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = false;
            }
#endif

            //Frame rootFrame = Window.Current.Content as Frame;
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;


                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += App.App_BackRequested;

            
            if (args.PreviousExecutionState == ApplicationExecutionState.Terminated ||
                    args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser)
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["user"];

                User user = null;
                if (composite != null)
                {
                    user = new User((string)composite["username"], (string)composite["password"]);
                    localSettings.Values.Remove("user");
                }
                

                rootFrame.Navigate(typeof(EntryPage), user);
            }
            else if (args.PreviousExecutionState == ApplicationExecutionState.NotRunning)
            {
                IPropertySet localProperties = ApplicationData.Current.LocalSettings.Values;
                if (localProperties.ContainsKey("FirstUse"))
                {
                    // The normal case
                    rootFrame.Navigate(typeof(EntryPage), args.Arguments);
                }
                else
                {
                    // The first-time case
                    rootFrame.Navigate(typeof(AboutPage), true);
                    localProperties["FirstUse"] = bool.FalseString;
                }
            }
            else
            {
                rootFrame.Navigate(typeof(EntryPage), args.Arguments);
            }

            //if (rootFrame.Content == null)
            //{
            //    // When the navigation stack isn't restored navigate to the first page,
            //    // configuring the new page by passing required information as a navigation
            //    // parameter
            //    rootFrame.Navigate(typeof(EntryPage), args.Arguments);
            //}
            // Ensure the current window is active
            Window.Current.Activate();

            
        }


        public static void App_BackRequested(object sender, BackRequestedEventArgs e)
        {

            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;

            // Navigate back if possible, and if the event has not 
            // already been handled .
            if (rootFrame.CurrentSourcePageType == typeof(MainPage))
            {
                // ignore the event. We want the default system behavior
                e.Handled = false;
            }
            else
            {
                e.Handled = true;

                if (rootFrame.CanGoBack)
                    rootFrame.GoBack();
            }

            
        }






        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }


    }
}
