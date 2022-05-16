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

namespace BUDDY
{
    public sealed partial class SaveProjectLoad : UserControl
    {
        Popup popup;
        public SaveProjectLoad()
        {
            this.InitializeComponent();
        }

        public SaveProjectLoad(string message)
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += DatabaseLoad_BackRequested;
            Window.Current.CoreWindow.SizeChanged += CoreWindow_SizeChanged;
            //msg_Txt.Text = message;
        }

        private void CoreWindow_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            UpdateUI();
        }

        private void DatabaseLoad_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Close();

        }
        private void UpdateUI()
        {

            var bounds = Window.Current.Bounds;
            this.Width = bounds.Width;
            this.Height = bounds.Height;
        }

        public void Show()
        {
            popup = new Popup();
            popup.Child = this;
            popup.IsOpen = true;
            UpdateUI();
        }

        public void Close()
        {
            if (popup.IsOpen)
            {
                popup.IsOpen = false;
                SystemNavigationManager.GetForCurrentView().BackRequested -= DatabaseLoad_BackRequested;
                Window.Current.CoreWindow.SizeChanged -= CoreWindow_SizeChanged;
            }
        }
    }
}
