using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace UwpCardReader.DialogBoxes
{
    public class CreateNewCardHolderContentDialog : ContentDialog
    {
        public CreateNewCardHolderContentDialog(string message, bool isSuccess)
        {
            Content = message;
            PrimaryButtonText = "OK";
            if (!isSuccess)
            {
                Title = "Error Creating New Card Holder";
            }
            else
            {
                Title = "Creating New Card Holder Success";
            }
        }
    }
}
