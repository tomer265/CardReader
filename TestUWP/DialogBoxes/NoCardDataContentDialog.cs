using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace TestUWP.DialogBoxes
{
    public sealed class NoCardDataContentDialog : ContentDialog
    {
        public NoCardDataContentDialog()
        {
            Content = "No user was found for this given card.\nPlease try another card, or create a new card holder.";
            Title = "No user found.";
            PrimaryButtonText = "Try another card";
            SecondaryButtonText = "Create card holder";
        }
    }
}
