﻿using CareBeer.Tests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CareBeer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TestSelectorPage : Page
    {
        public TestSelectorPage()
        {
            this.InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }


        private void goBtn_Click(object sender, RoutedEventArgs e)
        {
            TestId test = TestId.None;

            if ((bool)gaitCheckbox.IsChecked)
            {
                test |= TestId.Gait;
            }
            if ((bool)bubbleCheckbox.IsChecked)
            {
                test |= TestId.Bubble;
            }
            if ((bool)singleReactionCheckbox.IsChecked)
            {
                test |= TestId.ReactionSingle;
            }
            if ((bool)reactionCheckbox.IsChecked)
            {
                test |= TestId.Reaction;
            }
            if ((bool)speechCheckbox.IsChecked)
            {
                test |= TestId.Speech;
            }

            TestManager.Instance.TestsToRun = test;
            TestManager.Instance.Start(false);
        }

    }
}
