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
    /// Interaction logic for BorderStyleDialog.xaml
    /// </summary>
    public partial class BorderStyleDialog : Window
    {
        public DoubleCollection selectedLineStyle;
        public int selectedWidthValue;
        public ShapeInformation.BorderType selectedBorderType = ShapeInformation.BorderType.Solid;

        public BorderStyleDialog()
        {
            InitializeComponent();
            selectedLineStyle = new DoubleCollection() { 1, 0 };
            selectedBorderType = ShapeInformation.BorderType.Solid;
            selectedWidthValue = 1;
            sliderValue.Content = 1;
            this.Topmost = true;
            this.ShowInTaskbar = false;
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            if (selectedLineStyle != null)
            {
                selectedLineStyle.Clear();
                selectedLineStyle.Add(1);
                selectedLineStyle.Add(0);
                selectedBorderType = ShapeInformation.BorderType.Solid;
            }
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            if (selectedLineStyle != null)
            {
                selectedLineStyle.Clear();
                selectedLineStyle.Add(1);
                selectedLineStyle.Add(1);
                selectedLineStyle.Add(3);
                selectedLineStyle.Add(1);
                selectedBorderType = ShapeInformation.BorderType.DashDot;
            }
        }

        private void RadioButton_Checked_3(object sender, RoutedEventArgs e)
        {
            if (selectedLineStyle != null)
            {
                selectedLineStyle.Clear();
                selectedLineStyle.Add(1);
                selectedBorderType = ShapeInformation.BorderType.Dot;
            }
        }

        private void widthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sliderValue != null)
            {
                sliderValue.Content = widthSlider.Value;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            selectedWidthValue = (int)widthSlider.Value;
            this.Hide();
        }
    }
}
