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
using Windows.Storage.Streams;
using System.Text;
using TestUWP.Models;
using System.Xml.Linq;
using System.Xml;
using MongoDB.Driver;

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
            await CardAuth();
        }

        public async Task CardAuth()
        {
            try
            {
                IReadOnlyList<SmartCard> cards = await reader.FindAllCardsAsync();
                if (cards.Any())
                {
                    SmartCard smartCard = cards.First();

                    string buffer64BaseString = await GetCardBase64String(smartCard);
                    CardHolder cardHolderFromDb = GetCardHolderFromDB(@buffer64BaseString);

                    if (cardHolderFromDb != null)
                    {
                        tbOutput.Text += "Name: " + cardHolderFromDb.FirstName + cardHolderFromDb.LastName + Environment.NewLine;
                        tbOutput.Text += "Date Of Birth" + cardHolderFromDb.DateOfBirth.ToString("dd/MM/yyyy") + Environment.NewLine;
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
                tbOutput.Text += "Error reading the card." + Environment.NewLine + Environment.NewLine;
                tbOutput.Text += e.Message + Environment.NewLine;
                tbOutput.Text += e.InnerException + Environment.NewLine;
                tbOutput.Text += e.StackTrace + Environment.NewLine;
                tbOutput.Text += e.Source + Environment.NewLine;
                tbOutput.Text += e.Data + Environment.NewLine + Environment.NewLine + Environment.NewLine;
            }
        }

        private async void Reader_CardAdded(object sender, CardAddedEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                await CardAuth();
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

        private CardHolder GetCardHolderFromDB(string CardIdentifier)
        {
            string connectionString = GetConnectionStringFromConfigFile().Trim();
            MongoClient client = new MongoClient(@connectionString);
            IMongoDatabase dataBase = client.GetDatabase("CardReaderUWP");
            IMongoCollection<CardHolder> collection = dataBase.GetCollection<CardHolder>("CardHolders");

            CardHolder result = collection.Find(ch => ch.CardIdentifier == CardIdentifier).FirstOrDefault();

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
