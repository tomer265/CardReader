using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace TestUWP.DialogBoxes
{
    public class CreateNewCardHolderContentDialog : ContentDialog
    {
        TextBlock UserName = new TextBlock();

        public CreateNewCardHolderContentDialog()
        {
            UserName.Text = "Hi tomer";
        }
    }
}
