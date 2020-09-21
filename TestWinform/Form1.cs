using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace TestWinform
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            Task.Run(async () => await CardAuth());
        }

        private async Task CardAuth()
        {
            try
            {
                string selector = SmartCardReader.GetDeviceSelector();
                DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);

                DeviceInformation device = devices[0];
                SmartCardReader reader = await SmartCardReader.FromIdAsync(device.Id);
                IReadOnlyList<SmartCard> cards = reader.FindAllCardsAsync().GetResults();

                foreach (SmartCard card in cards)
                {
                    SmartCardProvisioning provisioning = await SmartCardProvisioning.FromSmartCardAsync(card);

                    Guid cardName = await provisioning.GetIdAsync();

                    tbOutput.Text += cardName + Environment.NewLine;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void Reader_CardAdded(SmartCardReader sender, CardAddedEventArgs args)
        {
            var a = 1;
        }
    }
}
