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
using Windows.Graphics.Display;
using TestUWP.Common;
using Windows.Storage.BulkAccess;
using Windows.UI;

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
            this.InitializeComponent();
            MaximizeWindowOnLoad();
            Task setReaderTask = SetReaderAndEvents();
            Task.Run(async () => await setReaderTask);
        }

        private void MaximizeWindowOnLoad()
        {
            DisplayInformation view = DisplayInformation.GetForCurrentView();

            Size resolution = new Size(view.ScreenWidthInRawPixels, view.ScreenHeightInRawPixels);
            double scale = view.ResolutionScale == ResolutionScale.Invalid ? 1 : view.RawPixelsPerViewPixel;
            Size bounds = new Size(resolution.Width / scale, resolution.Height / scale);

            ApplicationView.PreferredLaunchViewSize = new Size(bounds.Width, bounds.Height);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
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
            ScanBtn.Visibility = Visibility.Collapsed;
            await CardAuth();
            ScanBtn.Visibility = Visibility.Visible;
        }

        private void ClearFields()
        {
            imageBox.Source = null;
            tbOutput.Text = string.Empty;
        }

        public async Task CardAuth()
        {
            ClearFields();
            //await GetCardHolderFromDB("");
            //return;
            try
            {
                if (reader == null)
                {
                    tbOutput.Text += "No card reader detected." + Environment.NewLine;
                    return;
                }
                IReadOnlyList<SmartCard> cards = await reader.FindAllCardsAsync();
                if (cards.Any())
                {
                    SmartCard smartCard = cards.First();

                    string buffer64BaseString = await GetCardBase64String(smartCard);
                    CardHolder cardHolderFromDb = await GetCardHolderFromDB(@buffer64BaseString);


                    if (cardHolderFromDb != null)
                    {
                        SetCardHolderInAppWindow(cardHolderFromDb);
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

        public async void SetCardHolderInAppWindowInUIThread(CardHolder cardHolderFromDb)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetCardHolderInAppWindow(cardHolderFromDb);
            });
        }

        public async void SetCardHolderInAppWindow(CardHolder cardHolderFromDb)
        {
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            tbOutput.Text += $"Name: {cardHolderFromDb.FirstName} {cardHolderFromDb.LastName}" + Environment.NewLine;
            tbOutput.Text += $"Date Of Birth: {cardHolderFromDb.DateOfBirth.ToString("dd/MM/yyyy")}" + Environment.NewLine;
            await SetImageIframe(userFolder, @cardHolderFromDb.PicUrl);
            await PlayWelcomeSound(userFolder, cardHolderFromDb.VocalFileUrl);
        }

        private async Task PlayWelcomeSound(string userDropFolder, string vocalFileUrl)
        {
            MediaElement mysong = new MediaElement();
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(@userDropFolder);
            StorageFile file = await folder.GetFileAsync(@vocalFileUrl);
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
            {
                mysong.SetSource(stream, file.ContentType);
                mysong.Play();
            }

        }

        private async Task SetImageIframe(string userDropFolder, string fileUrl)
        {
            BitmapImage bitmap = new BitmapImage();
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(@userDropFolder);
            StorageFile file = await folder.GetFileAsync(@fileUrl);
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
            {
                bitmap.SetSource(stream);
                imageBox.Source = bitmap;
            }
        }

        private async void Reader_CardAdded(object sender, CardAddedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Button_Click(sender, null);
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

            string connectionString = CommonFunctions.GetConnectionStringFromConfigFile();
            MongoClient client = new MongoClient(@connectionString);
            IMongoDatabase dataBase = client.GetDatabase("CardReader");
            IMongoCollection<CardHolder> collection = dataBase.GetCollection<CardHolder>("CardHolders");
            CardHolder result = collection.Find(ch => ch.CardIdentifier == CardIdentifier).FirstOrDefault();
            //CardHolder result = null;
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
                    Tuple<double, double> bounds = SetWindowSizeWindowOnLoad();
                    CoreApplicationView newAV = CoreApplication.CreateNewView();
                    await newAV.Dispatcher.RunAsync(
                                    CoreDispatcherPriority.Normal,
                                    async () =>
                                    {
                                        Window newWindow = Window.Current;
                                        ApplicationView newAppView = ApplicationView.GetForCurrentView();
                                        newAppView.Title = "Create New Card Holder";

                                        Frame frame = new Frame();
                                        frame.Navigate(typeof(CreateCardHolder), Tuple.Create(CardIdentifier, this));
                                        newWindow.Content = frame;
                                        newWindow.Activate();

                                        await ApplicationViewSwitcher.TryShowAsStandaloneAsync(
                                            newAppView.Id,
                                            ViewSizePreference.UseHalf);
                                        newAppView.TryResizeView(new Size { Width = bounds.Item1, Height = bounds.Item2 });
                                    });

                }
            }
            return result;
        }
        private Tuple<double, double> SetWindowSizeWindowOnLoad()
        {
            DisplayInformation view = DisplayInformation.GetForCurrentView();

            Size resolution = new Size(view.ScreenWidthInRawPixels, view.ScreenHeightInRawPixels);
            double scale = view.ResolutionScale == ResolutionScale.Invalid ? 1 : view.RawPixelsPerViewPixel;
            Size bounds = new Size(resolution.Width / scale, resolution.Height / scale);

            return Tuple.Create(bounds.Width * 0.4, (bounds.Height / 3) * 2);
        }
    }
}
