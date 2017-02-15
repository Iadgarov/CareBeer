using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CareBeer.Tests;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CareBeer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ResultsPage : Page
    {
        public ResultsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            Frame.BackStack.Clear();
            setResultsText((Results)args.Parameter);
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Frame.BackStack.Clear();
        }


        private void setResultsText(Results res)
        {
            gaitTestResult.Text = "Gait Test - " + res.GaitResult.ToString();
            bubbleTestResult.Text = "Bubble Test - " + res.BubbleResult.ToString();
            singleReactionTestResult.Text = "Single-Button Reaction Test - " + res.SingleReactionResult.ToString();
            reactionTestResult.Text = "Multiple-Button Reaction Test - " + res.ReactionResult.ToString();
            speechTestResult.Text = "Speech Test - " + res.SpeechResult.ToString();
        }

        private void returnBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
