using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UwpCardReader.Common;
using UwpCardReader.DialogBoxes;
using UwpCardReader.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UwpCardReader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateCardHolder : Page
    {
        private static CardHolder CardHolderToAdd = new CardHolder();
        private static string CardIdentifier = string.Empty;
        private static MainPage MainPageInstance = new MainPage();
        public CreateCardHolder()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Tuple<string, MainPage> paramItem  = (Tuple<string, MainPage>)e.Parameter;
            CardIdentifier = paramItem.Item1;
            MainPageInstance = paramItem.Item2;
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
                ViewMode = PickerViewMode.Thumbnail,
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
            bool isAllFieldsFilled = true;
            List<string> fieldsNamesToFill = new List<string>();
            try
            {
                CardHolderToAdd.CardIdentifier = CardIdentifier;

                if (string.IsNullOrEmpty(HolderFirstNameTb.Text))
                {
                    fieldsNamesToFill.Add("First name");
                    isAllFieldsFilled = false;
                }
                else
                {
                    CardHolderToAdd.FirstName = HolderFirstNameTb.Text;
                }

                if (string.IsNullOrEmpty(HolderLastNameTb.Text))
                {
                    fieldsNamesToFill.Add("Last name");
                    isAllFieldsFilled = false;
                }
                else
                {
                    CardHolderToAdd.LastName = HolderLastNameTb.Text;
                }


                if (string.IsNullOrEmpty(HolderIDTb.Text))
                {
                    fieldsNamesToFill.Add("ID");
                    isAllFieldsFilled = false;
                }
                else
                {
                    CardHolderToAdd.GovernmentId = HolderIDTb.Text;
                }


                if (HolderDOBTb.SelectedDate == null)
                {
                    fieldsNamesToFill.Add("Date of birth");
                    isAllFieldsFilled = false;
                }
                else
                {
                    CardHolderToAdd.DateOfBirth = HolderDOBTb.Date.Date.ToLocalTime();
                }

                if (string.IsNullOrEmpty(CardHolderToAdd.PicUrl))
                {
                    fieldsNamesToFill.Add("Holder's picture");
                    isAllFieldsFilled = false;
                }

                if (string.IsNullOrEmpty(CardHolderToAdd.VocalFileUrl))
                {
                    fieldsNamesToFill.Add("Holder's welcome recording");
                    isAllFieldsFilled = false;
                }

                if (isAllFieldsFilled)
                {
                    MongoClient client = new MongoClient(@connectionString);
                    IMongoDatabase dataBase = client.GetDatabase("CardReader");
                    IMongoCollection<CardHolder> collection = dataBase.GetCollection<CardHolder>("CardHolders");
                    collection.InsertOne(CardHolderToAdd);
                    createNewCardHolderContentDialog = new CreateNewCardHolderContentDialog($"Card holder {CardHolderToAdd.FirstName} has been Created Succussfuly.", true);
                    var result = await createNewCardHolderContentDialog.ShowAsync();
                    Window.Current.Close();
                    MainPageInstance.SetCardHolderInAppWindowInUIThread(CardHolderToAdd);
                }
                else
                {
                    string message = string.Join(",\n", fieldsNamesToFill);
                    string errorMessage = string.Concat("Please fill in the following missing fields: \n\n", message);
                    createNewCardHolderContentDialog = new CreateNewCardHolderContentDialog(errorMessage, false);
                    var result = await createNewCardHolderContentDialog.ShowAsync();
                }

            }
            catch (Exception ex)
            {
                createNewCardHolderContentDialog = new CreateNewCardHolderContentDialog(ex.Message, false);
                var result = await createNewCardHolderContentDialog.ShowAsync();
            }
        }
    }
}
