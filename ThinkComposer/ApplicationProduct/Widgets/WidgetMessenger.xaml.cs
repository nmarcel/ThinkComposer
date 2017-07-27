using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Instrumind.Common;

namespace Instrumind.ThinkComposer.ApplicationProduct.Widgets
{
    /// <summary>
    /// Interaction logic for WidgetMessenger.xaml
    /// </summary>
    public partial class WidgetMessenger : UserControl
    {
        public WidgetMessenger()
        {
            InitializeComponent();
        }

        private void BtnSelectall_Click(object sender, RoutedEventArgs e)
        {
            this.MessagePublisher.UnselectAll();    // Needed to preserve order
            this.MessagePublisher.SelectAll();
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            var TextList = new StringBuilder(this.MessagePublisher.Items.Count * 80);

            var Items = this.MessagePublisher.SelectedItems;
            if (Items.Count < 1)
                Items = this.MessagePublisher.Items;

            foreach (var Item in Items)
                TextList.Append(Item.ToString() + Environment.NewLine);

            General.Execute(() => Clipboard.SetText(TextList.ToString()), "Cannot access Windows Clipboard!");
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ProductDirector.MessengerConsolePublisher.Clear();
        }
    }
}
