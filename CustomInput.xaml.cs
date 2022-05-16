using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class CustomInput : Page
    {
        List<StorageFile> customInputFile = new List<StorageFile>();
        StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

        public MainPage MainPage { get; private set; }

        public CustomInput()
        {
            this.InitializeComponent();
            //this.Loaded += CustomInputPage_Loaded;
        }
        //private void CustomInputPage_Loaded(object sender, RoutedEventArgs e)
        //{
        //    var s = ApplicationView.GetForCurrentView();
        //    s.TryResizeView(new Size { Width = 950.0, Height = 750.0 });
        //}

        private async void Upload_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".csv");
            picker.FileTypeFilter.Add(".CSV");

            var BUDDYfile = await picker.PickMultipleFilesAsync();

            if (BUDDYfile != null && BUDDYfile.Count > 0)
            {
                if(BUDDYfile.Count > 10)
                {
                    TooManyFilesSelected();
                    return;
                }
                string fileNameString = "";
                for (int i = 0; i < BUDDYfile.Count; i++)
                {
                    StorageFile newFile = await BUDDYfile[i].CopyAsync(storageFolder, BUDDYfile[i].Name, NameCollisionOption.ReplaceExisting);
                    customInputFile.Add(newFile);
                    fileNameString += "\n";
                    fileNameString += customInputFile[i].Name;
                }
                uploadedFileText.Text = fileNameString;
            }
            else
            {
                NoFileSelected();
            }

            //StorageFile BUDDYfile = await picker.PickSingleFileAsync();

            //if (BUDDYfile != null)
            //{
            //    customInputFile = await BUDDYfile.CopyAsync(storageFolder, BUDDYfile.Name, NameCollisionOption.ReplaceExisting);
            //    uploadedFileText.Text = customInputFile.Name;
            //}
            //else
            //{
            //    NoFileSelected();
            //}
        }
        private async void NoFileSelected()
        {
            ContentDialog noEXEDialog = new ContentDialog
            {
                Title = "Warning",
                Content = "Operation Cancelled.",
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await noEXEDialog.ShowAsync();
        }
        private async void TooManyFilesSelected()
        {
            ContentDialog noEXEDialog = new ContentDialog
            {
                Title = "Warning",
                Content = "Please input no more than 10 files. Operation Cancelled.",
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await noEXEDialog.ShowAsync();
        }

        private async void DownloadDemo_Click(object sender, RoutedEventArgs e)
        {
            StorageFile demoFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/FeatureTable_demo.csv"));

            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("CSV", new List<string>() { ".csv" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "FeatureTable_demo.csv";
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);
                // write to file
                await FileIO.WriteTextAsync(file, await FileIO.ReadTextAsync(demoFile));
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status =
                    await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    ExportSaved();
                }
                else
                {
                    ExportNotSaved();
                }
            }
            else
            {
                ExportCancelled();
            }
        }

        private async void ExportCancelled()
        {
            ContentDialog noEXEDialog = new ContentDialog
            {
                Title = "Warning",
                Content = "Operation cancelled.",
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await noEXEDialog.ShowAsync();
        }
        private async void ExportSaved()
        {
            ContentDialog noEXEDialog = new ContentDialog
            {
                Title = "Success",
                Content = "Demo feature Table was saved.",
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await noEXEDialog.ShowAsync();
        }
        private async void ExportNotSaved()
        {
            ContentDialog noEXEDialog = new ContentDialog
            {
                Title = "Warning",
                Content = "Demo feature table was not saved.",
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await noEXEDialog.ShowAsync();
        }

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (customInputFile != null && customInputFile.Count > 0)
            {
                await this.MainPage.UpdateCustomFilesAsync(customInputFile);
                Window.Current.Close();
            }
            else
            {
                NoFileUploaded();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.MainPage = (MainPage)e.Parameter;
        }

        private async void DuplicateFileSelected()
        {
            ContentDialog noEXEDialog = new ContentDialog
            {
                Title = "Warning",
                Content = "Duplicate File Selected",
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await noEXEDialog.ShowAsync();
        }
        private async void NoFileUploaded()
        {
            ContentDialog noEXEDialog = new ContentDialog
            {
                Title = "Error",
                Content = "No file(s) uploaded",
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await noEXEDialog.ShowAsync();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Window.Current.Close();
        }
    }
}
