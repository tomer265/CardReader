using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Text;
using TestUWP.Models;
using System.Xml.Linq;
using System.Xml;
using MongoDB.Driver;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using System.Net;
using TestUWP.DialogBoxes;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SmartCardReader reader;
        public MainPage()
        {
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Maximized;
            this.InitializeComponent();
            Task setReaderTask = SetReaderAndEvents();
            Task.Run(async () => await setReaderTask);
        }

        private async Task SetReaderAndEvents()
        {
            string selector = SmartCardReader.GetDeviceSelector();
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);

            DeviceInformation device = devices[0];
            reader = await SmartCardReader.FromIdAsync(device.Id);
            reader.CardAdded += Reader_CardAdded;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //var vd = new CreateNewCardHolderContentDialog();
            //await vd.ShowAsync();

            await CardAuth();
        }

        private void ClearFields()
        {
            ImageBox.Source = null;
            tbOutput.Text = string.Empty;
        }

        public async Task CardAuth()
        {
            ClearFields();
            try
            {
                IReadOnlyList<SmartCard> cards = await reader.FindAllCardsAsync();
                if (cards.Any())
                {
                    SmartCard smartCard = cards.First();

                    string buffer64BaseString = await GetCardBase64String(smartCard);
                    CardHolder cardHolderFromDb = await GetCardHolderFromDB(@buffer64BaseString);
                    

                    if (cardHolderFromDb != null)
                    {
                        string userDropFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                        tbOutput.Text += $"Name: {cardHolderFromDb.FirstName} {cardHolderFromDb.LastName}" + Environment.NewLine;
                        tbOutput.Text += $"Date Of Birth: {cardHolderFromDb.DateOfBirth.ToString("dd/MM/yyyy")}" + Environment.NewLine;
                        await SetImageIframe(userDropFolder, @cardHolderFromDb.PicUrl);
                        await PlayWelcomeSound(userDropFolder, cardHolderFromDb.VocalFileUrl);
                    }
                    else
                    {
                        tbOutput.Text += "No user detected for this card." + Environment.NewLine;
                    }
                }
                else
                {
                    tbOutput.Text += "No cards detected." + Environment.NewLine;
                }
            }
            catch (Exception e)
            {
                string errorName = e.GetType().Name;
                if (errorName == "TimeoutException")
                {
                    tbOutput.Text += "Error reading the card." + Environment.NewLine;
                    tbOutput.Text += "MongoDB server took to long to respond. Try again later." + Environment.NewLine + Environment.NewLine + Environment.NewLine;
                }
                else if (errorName == "DnsResponseException")
                {
                    tbOutput.Text += "Error reading the card." + Environment.NewLine;
                    tbOutput.Text += "There is a problem with the DNS address used. Try using another DNS address." + Environment.NewLine + Environment.NewLine + Environment.NewLine;
                }
                else
                {
                    tbOutput.Text += "Error reading the card." + Environment.NewLine + Environment.NewLine;
                    tbOutput.Text += e.Message + Environment.NewLine;
                    tbOutput.Text += e.InnerException + Environment.NewLine;
                    tbOutput.Text += e.StackTrace + Environment.NewLine;
                    tbOutput.Text += e.Source + Environment.NewLine;
                    tbOutput.Text += e.Data + Environment.NewLine + Environment.NewLine + Environment.NewLine;
                }
            }
        }

        private async Task PlayWelcomeSound(string userDropFolder, string vocalFileUrl)
        {
            MediaElement mysong = new MediaElement();
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(@userDropFolder);
            StorageFile file = await folder.GetFileAsync(@vocalFileUrl);
            var stream = await file.OpenAsync(FileAccessMode.Read);
            mysong.SetSource(stream, file.ContentType);
            mysong.Play();
        }

        private async Task SetImageIframe(string userDropFolder, string fileUrl)
        {
            BitmapImage bitmap = new BitmapImage();
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(@userDropFolder);
            StorageFile file = await folder.GetFileAsync(@fileUrl);
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                bitmap.SetSource(stream);
                ImageBox.Source = bitmap;
            }
        }

        private async void Reader_CardAdded(object sender, CardAddedEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                ScanBtn.ClickMode = ClickMode.Press;
                await CardAuth();
                ScanBtn.ClickMode = ClickMode.Release;
            });
        }

        private async Task<string> GetCardBase64String(SmartCard smartCard)
        {
            var buffer = await smartCard.GetAnswerToResetAsync();
            DataReader bufferReader = DataReader.FromBuffer(buffer);
            byte[] fileContent = new byte[bufferReader.UnconsumedBufferLength];
            bufferReader.ReadBytes(fileContent);
            string buffer64BaseString = Convert.ToBase64String(fileContent);

            return buffer64BaseString;
        }

        private async Task<CardHolder> GetCardHolderFromDB(string CardIdentifier)
        {
            ContentDialog cd = new ContentDialog()
            {
                Content = "Retrieving card holder from database",
                Title = "Please Wait.",
                CloseButtonText = "OK"
            };
            await cd.ShowAsync();

            //AppPage.Opacity = 0.5;
            

            string connectionString = GetConnectionStringFromConfigFile().Trim();
            //MongoClient client = new MongoClient(@connectionString);
            //IMongoDatabase dataBase = client.GetDatabase("CardReader");
            //IMongoCollection<CardHolder> collection = dataBase.GetCollection<CardHolder>("CardHolders");
            //CardHolder result = collection.Find(ch => ch.CardIdentifier == CardIdentifier).FirstOrDefault();
            CardHolder result = null;
            if (result == null)
            {
                cd = new NoCardDataContentDialog();

                var userSelection = await cd.ShowAsync();
                if (userSelection == ContentDialogResult.Primary || userSelection == ContentDialogResult.None)
                {
                    return null;
                }
                else if (userSelection == ContentDialogResult.Secondary)
                {
                    ApplicationView currentAV = ApplicationView.GetForCurrentView();
                    CoreApplicationView newAV = CoreApplication.CreateNewView();
                    await newAV.Dispatcher.RunAsync(
                                    CoreDispatcherPriority.Normal,
                                    async () =>
                                    {
                                        Window newWindow = Window.Current;
                                        ApplicationView newAppView = ApplicationView.GetForCurrentView();
                                        newAppView.Title = "New window";

                                        Frame frame = new Frame();
                                        frame.Navigate(typeof(CreateCardHolder), null);
                                        newWindow.Content = frame;
                                        newWindow.Activate();

                                        await ApplicationViewSwitcher.TryShowAsStandaloneAsync(
                                            newAppView.Id,
                                            ViewSizePreference.UseMinimum,
                                            currentAV.Id,
                                            ViewSizePreference.UseMinimum);
                                    });

                }
            }
            return result;
        }

        private string GetConnectionStringFromConfigFile()
        {
            XmlDocument configurationsFile = new XmlDocument();
            configurationsFile.Load("Configurations.xml");
            XmlNode node = configurationsFile.SelectSingleNode("/AppSettings/ConnectionString");
            return node.InnerText;
        }
    }
}
