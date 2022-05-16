using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
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
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class About : Page
    {
        public About()
        {
            this.InitializeComponent();
            this.Loaded += AboutPage_Loaded;
        }
        private void AboutPage_Loaded(object sender, RoutedEventArgs e)
        {
            Removefocus_button.Focus(FocusState.Programmatic);
            var s = ApplicationView.GetForCurrentView();
            s.TryResizeView(new Windows.Foundation.Size { Width = 525.0, Height = 560.0 });
        }
    }
}
