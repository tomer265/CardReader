using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TestUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateCardHolder : Page
    {
        public CreateCardHolder()
        {
            this.InitializeComponent();
        }

        private async void AddRecordingToNewCardHolder(object sender, RoutedEventArgs e)
        {
            string recordingFilePath = await GetRecordingFilePath();
        }

        private async Task<string> GetRecordingFilePath()
        {
            FileOpenPicker recordingPicker = new FileOpenPicker()
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                CommitButtonText = "Add Recording",
                ViewMode = PickerViewMode.List,
                FileTypeFilter = { ".m4a", ".wav", ".mp3", ".wma" }
            };

            StorageFile recordingFile = await recordingPicker.PickSingleFileAsync();

            if (recordingFile != null)
            {
                btnAddRecording.Background = new SolidColorBrush(Colors.ForestGreen);
                return recordingFile.Path;
            }
            return string.Empty;
        }
    }
}
