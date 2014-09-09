/* Name: Pranish Dutt
 * Date: 11/10/20113 
 * Purpose: Assignment 4 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace program_4
{
    /// <summary>
    /// Interaction logic for ColorDialogWindow.xaml
    /// </summary>
    public partial class ColorDialogWindow : Window
    {
        //testing
        public Brush selectedColor { get; private set; }
        private Color transparencyChecked;

        public ColorDialogWindow(string label_Info, string title)
        {
            InitializeComponent();
            label_Dialog.Content = label_Info;
            this.Title = title;
            this.Topmost = true;
            this.ShowInTaskbar = false;
        }

        private void removeTransparency(ref Color c)
        {
            if ((c.R == 255) && (c.G == 255) && (c.B == 255))
            {
                c.R--;
                c.G--;
                c.B--;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            transparencyChecked = colorDialog.SelectedColor;
            removeTransparency(ref transparencyChecked);
            this.selectedColor = new SolidColorBrush(transparencyChecked);
            this.endGradientColor.Color = transparencyChecked;
            this.Hide();
        }

        private void colorDialog_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            transparencyChecked = colorDialog.SelectedColor;
            removeTransparency(ref transparencyChecked);
            this.selectedColor = new SolidColorBrush(transparencyChecked);
            this.endGradientColor.Color = transparencyChecked;
        }
    }
}
