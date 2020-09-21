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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await CardAuth();
            //Task.Run(async () => await CardAuth());
        }

        public async Task CardAuth()
        {
            Guid cardGuid = Guid.Empty;
            tbOutput.Text += "Entered\n";
            try
            {
                string selector = SmartCardReader.GetDeviceSelector();
                DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);

                DeviceInformation device = devices[0];
                SmartCardReader reader = await SmartCardReader.FromIdAsync(device.Id);
                IReadOnlyList<SmartCard> cards = await reader.FindAllCardsAsync();
                SmartCard smartCard = cards.First();

                IAsyncOperation<SmartCardProvisioning> provisioning = SmartCardProvisioning.FromSmartCardAsync(smartCard);

                var task = await Task.Run(() => provisioning.GetResults().GetIdAsync());
                var results = task.GetResults();
                
                
                //cardGuid = t.res;

                tbOutput.Text += cardGuid + Environment.NewLine;
            }
            catch (Exception e)
            {
                tbOutput.Text += e.Message + Environment.NewLine;
                tbOutput.Text += e.InnerException + Environment.NewLine;
                tbOutput.Text += e.StackTrace + Environment.NewLine;
                tbOutput.Text += e.Source + Environment.NewLine;
                tbOutput.Text += e.Data + Environment.NewLine;
            }
        }

        private void Reader_CardAdded(SmartCardReader sender, CardAddedEventArgs args)
        {
            var a = 1;
        }
    }
}
