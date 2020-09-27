using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TestUWP.Common;
using TestUWP.DialogBoxes;
using TestUWP.Models;
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
        private static CardHolder CardHolderToAdd = new CardHolder();
        private static string CardIdentifier = string.Empty;
        public CreateCardHolder()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            CardIdentifier = (string)e.Parameter;
        }

        private async void AddPictureToNewCardHolder(object sender, RoutedEventArgs e)
        {
            string picutreFilePath = await GetPictureFilePath();
            CardHolderToAdd.PicUrl = picutreFilePath;
        }

        private async void AddRecordingToNewCardHolder(object sender, RoutedEventArgs e)
        {
            string recordingFilePath = await GetRecordingFilePath();
            CardHolderToAdd.VocalFileUrl = recordingFilePath;
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
                string path = recordingFile.Path.ToLower();
                string[] pathDevided = path.Split("dropbox");
                btnAddRecording.Background = new SolidColorBrush(Colors.ForestGreen);
                return $@"dropbox{pathDevided[1]}";
            }
            return string.Empty;
        }

        private async Task<string> GetPictureFilePath()
        {
            FileOpenPicker picturePicker = new FileOpenPicker()
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                CommitButtonText = "Add Picture",
                ViewMode = PickerViewMode.List,
                FileTypeFilter = { ".jpg", ".jpeg", ".png", ".bmp" }
            };

            StorageFile pictureFile = await picturePicker.PickSingleFileAsync();

            if (pictureFile != null)
            {
                string path = pictureFile.Path.ToLower();
                string[] pathDevided = path.Split("dropbox");
                btnAddPicture.Background = new SolidColorBrush(Colors.ForestGreen);
                return $@"dropbox{pathDevided[1]}";
            }
            return string.Empty;
        }

        private async void BtnCreateHolder_Click(object sender, RoutedEventArgs e)
        {
            CreateNewCardHolderContentDialog createNewCardHolderContentDialog;
            string connectionString = CommonFunctions.GetConnectionStringFromConfigFile();
            try
            {
                CardHolderToAdd.FirstName = HolderFirstNameTb.Text;
                CardHolderToAdd.LastName = HolderLastNameTb.Text;
                CardHolderToAdd.GovernmentId = HolderIDTb.Text;
                CardHolderToAdd.DateOfBirth = HolderDOBTb.Date.Date;
                CardHolderToAdd.CardIdentifier = CardIdentifier;
                MongoClient client = new MongoClient(@connectionString);
                IMongoDatabase dataBase = client.GetDatabase("CardReader");
                IMongoCollection<CardHolder> collection = dataBase.GetCollection<CardHolder>("CardHolders");
                collection.InsertOne(CardHolderToAdd);
                createNewCardHolderContentDialog = new CreateNewCardHolderContentDialog($"Card holder {CardHolderToAdd.FirstName} has been Created Succussfuly.", true);
                var result = await createNewCardHolderContentDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                createNewCardHolderContentDialog = new CreateNewCardHolderContentDialog(ex.Message, false);
                var result = await createNewCardHolderContentDialog.ShowAsync();
            }
        }
    }
}
