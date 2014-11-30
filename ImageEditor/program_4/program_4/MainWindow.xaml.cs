/* Name: Pranish Dutt
 * Date: 11/10/20113 
 * Purpose: Assignment 4 */

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Text;
using System.Reflection;
using System.Windows.Markup;

namespace program_4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties
        //Shape Info
        private Brush fillColor, fillColorRetrieved;
        private Brush lineColor, lineColorRetrieved;
        private DoubleCollection lineStyle;
        private int lineWidth;
        private int NumShapesCreated;
        private int numShapesCreated
        {
            get
            {
                return this.NumShapesCreated;
            }
            set
            {
                this.NumShapesCreated = value;
                numShapeLabel.Content = "# of Shapes: " + NumShapesCreated;
            }
        }
        private ShapeInformation.BorderType borderStyle;
        int backgroundImgExists;
        string bgFileName, shpFilePath, imgFilePath, opnFilePath;
        Color bgColor;

        //Icons for lineStyle toggle
        private BitmapImage lineToggleOn;
        private BitmapImage lineToggleOff;

        //Points
        private Point originLoc;
        private Point mousePos;
        private Point drawLoc;
        private Point location;
        private Point startDrag;

        //Dialogs
        ColorDialogWindow fillColorOptions;
        ColorDialogWindow lineColorOptions;
        ColorDialogWindow bGColorOptions;

        //Comparison Objects
        readonly DoubleCollection solid = new DoubleCollection() { 1, 0 };
        readonly DoubleCollection dashDot = new DoubleCollection() { 1, 1, 3, 1 };
        readonly DoubleCollection dot = new DoubleCollection() { 1 };

        //Undo/Redo Temp
        CustomShape tempShape;
        Rectangle tempRect;
        #endregion

        #region Initialization
        public MainWindow()
        {
            InitializeComponent();
            canvas.Background = Brushes.Black;

            //Initialize shape comboBox
            shapeComboBox.Items.Add("Select a Shape:");
            shapeComboBox.Items.Add("Rectangle");
            shapeComboBox.Items.Add("Ellipse");
            shapeComboBox.Items.Add("Right Triangle");
            shapeComboBox.Items.Add("Isoseles Triangle");
            shapeComboBox.Items.Add("Diamond");
            shapeComboBox.Items.Add("Hexagon");
            shapeComboBox.Items.Add("Pentagon");
            shapeComboBox.SelectedIndex = 0;

            //Initialize toggle icons
            lineToggleOn = new BitmapImage(new Uri("AppResources/icons/GRAPH03.ICO", UriKind.Relative));
            lineToggleOff = new BitmapImage(new Uri("AppResources/icons/GRAPH03grey.png", UriKind.Relative));
            lineStyleImg.Source = lineToggleOff;

            //Initalize shape info
            fillColor = Brushes.Transparent;
            lineColor = Brushes.Transparent;
            fillColorRetrieved = Brushes.Transparent;
            lineColorRetrieved = Brushes.Transparent;
            lineStyle = new DoubleCollection() { 1, 0 };
            lineWidth = 1;
            numShapesCreated = 0;
            backgroundImgExists = 0;
            bgFileName = "";
            shpFilePath = "";
            imgFilePath = "";
            bgColor = Colors.Black;

            //Initialize points
            originLoc.X = 1;
            originLoc.Y = 1;

            //Initialize Dialogs
            fillColorOptions = new ColorDialogWindow("Please select a fill color:", "Fill Color");
            lineColorOptions = new ColorDialogWindow("Please select a line color:", "Line Color");
            bGColorOptions = new ColorDialogWindow("Please select a background color:", "Background Color");

            //Temp initilization
            tempShape = null;
            undoBtn.Visibility = Visibility.Hidden;
            redoBtn.Visibility = Visibility.Hidden;
        }
        #endregion

        #region MouseEvents: Display & Draw
        private void canvasWindow_MouseMove_1(object sender, MouseEventArgs e)
        {
            //Update the coordinatesBox to display the current mouse position
            coordinatesBox.Text = "(x,y): " + e.GetPosition(canvas).ToString();

            if (canvas.IsMouseCaptured)
            {
                Point currentPoint = e.GetPosition(canvas);
                double x = startDrag.X < currentPoint.X ? startDrag.X : currentPoint.X;
                double y = startDrag.Y < currentPoint.Y ? startDrag.Y : currentPoint.Y;
                if (rectangle.Visibility == Visibility.Hidden)
                {
                    rectangle.Visibility = Visibility.Visible;
                }
                rectangle.RenderTransform = new TranslateTransform(x, y);
                rectangle.Width = Math.Abs(e.GetPosition(canvas).X - startDrag.X);
                rectangle.Height = Math.Abs(e.GetPosition(canvas).Y - startDrag.Y);
            }
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Set originloc at mousedown location.
            originLoc = e.GetPosition(canvas);
            startDrag = e.GetPosition(canvas);
            Canvas.SetZIndex(rectangle, canvas.Children.Count);
            if (!canvas.IsMouseCaptured)
            {
                canvas.CaptureMouse();
            }
        }

        //Set to always draw from the top left of the "rubber band box"
        private void setTopLeft()
        {
            if ((originLoc.X < mousePos.X) && (originLoc.Y < mousePos.Y))
            {
                drawLoc = originLoc;
            }
            else if ((originLoc.X < mousePos.X) && (originLoc.Y > mousePos.Y))
            {
                drawLoc.X = originLoc.X;
                drawLoc.Y = mousePos.Y;
            }
            else if ((originLoc.X > mousePos.X) && (originLoc.Y < mousePos.Y))
            {
                drawLoc.X = mousePos.X;
                drawLoc.Y = originLoc.Y;
            }
            else if ((originLoc.X > mousePos.X) && (originLoc.Y > mousePos.Y))
            {
                drawLoc = mousePos;
            }
            else
            {
                MessageBox.Show("Click and drag the mouse to draw a shape.");
            }
        }

        //Draw shapes to the canvas
        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (canvas.IsMouseCaptured)
            {
                canvas.ReleaseMouseCapture();
            }
            rectangle.Visibility = Visibility.Hidden;

            if (shapeComboBox.SelectedIndex == 0)
            {
                MessageBox.Show("Please select a shape from the shape combobox.");
                return;
            }
            if (lineStyleImg.Source.Equals(lineToggleOn))
            {
                if (lineColor == Brushes.Transparent)
                {
                    MessageBox.Show("Please select a Line color.");
                    return;
                }
            }
            if (lineStyleImg.Source.Equals(lineToggleOff))
            {
                if (fillColor == Brushes.Transparent)
                {
                    MessageBox.Show("Please select a Fill color.");
                    return;
                }
            }

            mousePos = e.GetPosition(canvas);

            switch (shapeComboBox.SelectedIndex)
            {
                case 1:
                    //Draw Rectangle
                    CustomRectangle customRectangle = new CustomRectangle(numShapesCreated, originLoc, mousePos, borderStyle);
                    customRectangle.Width = Math.Abs(mousePos.X - originLoc.X);
                    customRectangle.Height = Math.Abs(mousePos.Y - originLoc.Y);
                    customRectangle.Stroke = lineColor;
                    customRectangle.Fill = fillColor;
                    customRectangle.StrokeThickness = lineWidth;
                    customRectangle.StrokeDashArray = lineStyle;
                    customRectangle.borderType = borderStyle;
                    customRectangle.HorizontalAlignment = HorizontalAlignment.Left;
                    customRectangle.VerticalAlignment = VerticalAlignment.Center;
                    setTopLeft();
                    Canvas.SetLeft(customRectangle, drawLoc.X);
                    Canvas.SetTop(customRectangle, drawLoc.Y);
                    canvas.Children.Add(customRectangle);
                    numShapesCreated++;
                    undoBtn.Visibility = Visibility.Visible;
                    redoBtn.Visibility = Visibility.Hidden;
                    break;
                case 2:
                    //Draw Ellipse
                    CustomEllipse customEllipse = new CustomEllipse(numShapesCreated, originLoc, mousePos, borderStyle);
                    customEllipse.Width = Math.Abs(mousePos.X - originLoc.X);
                    customEllipse.Height = Math.Abs(mousePos.Y - originLoc.Y);
                    customEllipse.Stroke = lineColor;
                    customEllipse.Fill = fillColor;
                    customEllipse.StrokeThickness = lineWidth;
                    customEllipse.StrokeDashArray = lineStyle;
                    customEllipse.borderType = borderStyle;
                    customEllipse.HorizontalAlignment = HorizontalAlignment.Left;
                    customEllipse.VerticalAlignment = VerticalAlignment.Center;
                    setTopLeft();
                    Canvas.SetLeft(customEllipse, drawLoc.X);
                    Canvas.SetTop(customEllipse, drawLoc.Y);
                    canvas.Children.Add(customEllipse);
                    numShapesCreated++;
                    undoBtn.Visibility = Visibility.Visible;
                    redoBtn.Visibility = Visibility.Hidden;
                    break;
                case 3:
                    //Draw Right Triangle
                    CustomRightTriangle customRightTriangle = new CustomRightTriangle(numShapesCreated, originLoc, mousePos, borderStyle);
                    customRightTriangle.Width = Math.Abs(mousePos.X - originLoc.X);
                    customRightTriangle.Height = Math.Abs(mousePos.Y - originLoc.Y);
                    customRightTriangle.Stroke = lineColor;
                    customRightTriangle.Fill = fillColor;
                    customRightTriangle.StrokeThickness = lineWidth;
                    customRightTriangle.StrokeDashArray = lineStyle;
                    customRightTriangle.borderType = borderStyle;
                    customRightTriangle.HorizontalAlignment = HorizontalAlignment.Left;
                    customRightTriangle.VerticalAlignment = VerticalAlignment.Center;
                    setTopLeft();
                    Canvas.SetLeft(customRightTriangle, drawLoc.X);
                    Canvas.SetTop(customRightTriangle, drawLoc.Y);
                    canvas.Children.Add(customRightTriangle);
                    numShapesCreated++;
                    undoBtn.Visibility = Visibility.Visible;
                    redoBtn.Visibility = Visibility.Hidden;
                    break;
                case 4:
                    //Draw Isoceles Triangle
                    CustomIsocelesTriangle customIsocelesTriangle = new CustomIsocelesTriangle(numShapesCreated, originLoc, mousePos, borderStyle);
                    customIsocelesTriangle.Width = Math.Abs(mousePos.X - originLoc.X);
                    customIsocelesTriangle.Height = Math.Abs(mousePos.Y - originLoc.Y);
                    customIsocelesTriangle.Stroke = lineColor;
                    customIsocelesTriangle.Fill = fillColor;
                    customIsocelesTriangle.StrokeThickness = lineWidth;
                    customIsocelesTriangle.StrokeDashArray = lineStyle;
                    customIsocelesTriangle.borderType = borderStyle;
                    customIsocelesTriangle.HorizontalAlignment = HorizontalAlignment.Left;
                    customIsocelesTriangle.VerticalAlignment = VerticalAlignment.Center;
                    setTopLeft();
                    Canvas.SetLeft(customIsocelesTriangle, drawLoc.X);
                    Canvas.SetTop(customIsocelesTriangle, drawLoc.Y);
                    canvas.Children.Add(customIsocelesTriangle);
                    numShapesCreated++;
                    undoBtn.Visibility = Visibility.Visible;
                    redoBtn.Visibility = Visibility.Hidden;
                    break;
                case 5:
                    //Draw Diamond
                    CustomDiamond customDiamond = new CustomDiamond(numShapesCreated, originLoc, mousePos, borderStyle);
                    customDiamond.Width = Math.Abs(mousePos.X - originLoc.X);
                    customDiamond.Height = Math.Abs(mousePos.Y - originLoc.Y);
                    customDiamond.Stroke = lineColor;
                    customDiamond.Fill = fillColor;
                    customDiamond.StrokeThickness = lineWidth;
                    customDiamond.StrokeDashArray = lineStyle;
                    customDiamond.borderType = borderStyle;
                    customDiamond.HorizontalAlignment = HorizontalAlignment.Left;
                    customDiamond.VerticalAlignment = VerticalAlignment.Center;
                    setTopLeft();
                    Canvas.SetLeft(customDiamond, drawLoc.X);
                    Canvas.SetTop(customDiamond, drawLoc.Y);
                    canvas.Children.Add(customDiamond);
                    numShapesCreated++;
                    undoBtn.Visibility = Visibility.Visible;
                    redoBtn.Visibility = Visibility.Hidden;
                    break;
                case 6:
                    //Draw Hexgon
                    CustomHexagon customHexagon = new CustomHexagon(numShapesCreated, originLoc, mousePos, borderStyle);
                    customHexagon.Width = Math.Abs(mousePos.X - originLoc.X);
                    customHexagon.Height = Math.Abs(mousePos.Y - originLoc.Y);
                    customHexagon.Stroke = lineColor;
                    customHexagon.Fill = fillColor;
                    customHexagon.StrokeThickness = lineWidth;
                    customHexagon.StrokeDashArray = lineStyle;
                    customHexagon.borderType = borderStyle;
                    customHexagon.HorizontalAlignment = HorizontalAlignment.Left;
                    customHexagon.VerticalAlignment = VerticalAlignment.Center;
                    setTopLeft();
                    Canvas.SetLeft(customHexagon, drawLoc.X);
                    Canvas.SetTop(customHexagon, drawLoc.Y);
                    canvas.Children.Add(customHexagon);
                    numShapesCreated++;
                    undoBtn.Visibility = Visibility.Visible;
                    redoBtn.Visibility = Visibility.Hidden;
                    break;
                case 7:
                    //Draw Pentagon
                    CustomPentagon customPentagon = new CustomPentagon(numShapesCreated, originLoc, mousePos, borderStyle);
                    customPentagon.Width = Math.Abs(mousePos.X - originLoc.X);
                    customPentagon.Height = Math.Abs(mousePos.Y - originLoc.Y);
                    customPentagon.Stroke = lineColor;
                    customPentagon.Fill = fillColor;
                    customPentagon.StrokeThickness = lineWidth;
                    customPentagon.StrokeDashArray = lineStyle;
                    customPentagon.borderType = borderStyle;
                    customPentagon.HorizontalAlignment = HorizontalAlignment.Left;
                    customPentagon.VerticalAlignment = VerticalAlignment.Center;
                    setTopLeft();
                    Canvas.SetLeft(customPentagon, drawLoc.X);
                    Canvas.SetTop(customPentagon, drawLoc.Y);
                    canvas.Children.Add(customPentagon);
                    numShapesCreated++;
                    undoBtn.Visibility = Visibility.Visible;
                    redoBtn.Visibility = Visibility.Hidden;
                    break;
            }
        }
        #endregion

        #region stripClick Events

            #region .shp Loading
        private void stripClick_Open(object sender, MouseButtonEventArgs e)
        {
            //Dialog: Open .shp file
            //Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            dlg.FileName = "shapeData"; // Default file name
            dlg.DefaultExt = ".shp"; // Default file extension
            dlg.Filter = "Shape Image Data (.shp)|*.shp|All files (*.*)|*.*"; // Filter files by extension
            //Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            //Process open file dialog box results 
            if (result == true)
            {
                try
                {
                    string lineOfData;
                    string[] data;

                    using (StreamReader shpReader = new StreamReader(dlg.FileName))
                    {
                        //Initialize background color
                        lineOfData = shpReader.ReadLine();
                        data = lineOfData.Split(' ');
                        canvas.Background = new SolidColorBrush(Color.FromArgb(255,
                                                                (byte)Convert.ToInt32(data[0]),
                                                                (byte)Convert.ToInt32(data[1]),
                                                                (byte)Convert.ToInt32(data[2])));
                        //Initialize background
                        lineOfData = shpReader.ReadLine();
                        data = lineOfData.Split(' ');
                        if (data[0] == "1")
                        {
                            if (File.Exists(data[1]))
                            {
                                ImageBrush background = new ImageBrush(new BitmapImage(new Uri(data[1], UriKind.Relative)));
                                background.Stretch = Stretch.UniformToFill;
                                canvas.Background = background;
                                bgFileName = data[1];
                                backgroundImgExists = 1;
                            }
                            else
                            {
                                MessageBox.Show("Cannot open file: background image not found or does not exist.");
                            }
                        }

                        //Initialize numOfShapesCreated
                        lineOfData = shpReader.ReadLine();
                        numShapesCreated = Convert.ToInt32(lineOfData);

                        //Initilize shapes
                        ShapeInformation.ShapeInfo shapeInfo;
                        for (int i = 1; i <= numShapesCreated; i++)
                        {
                            lineOfData = shpReader.ReadLine();
                            if (lineOfData != null)
                            {
                                data = lineOfData.Split(' ');

                                shapeInfo = new ShapeInformation.ShapeInfo(numShapesCreated,
                                                                           (ShapeInformation.ShapeType)Convert.ToInt32(data[0]),
                                                                           (ShapeInformation.FillType)Convert.ToInt32(data[1]),
                                                                           new Point(Convert.ToInt32(data[2]), Convert.ToInt32(data[3])),
                                                                           new Point(Convert.ToInt32(data[4]), Convert.ToInt32(data[5])),
                                                                           Color.FromArgb(255,
                                                                                         (byte)Convert.ToInt32(data[6]),
                                                                                         (byte)Convert.ToInt32(data[7]),
                                                                                         (byte)Convert.ToInt32(data[8])),
                                                                           Color.FromArgb(255,
                                                                                         (byte)Convert.ToInt32(data[9]),
                                                                                         (byte)Convert.ToInt32(data[10]),
                                                                                         (byte)Convert.ToInt32(data[11])),
                                                                           (ShapeInformation.BorderType)Convert.ToInt32(data[12]),
                                                                           Convert.ToInt32(data[13]));
                                SolidColorBrush fillCol;
                                SolidColorBrush borderCol;
                                if ((shapeInfo.fillColor.R == 255) && (shapeInfo.fillColor.G == 255) && (shapeInfo.fillColor.B == 255))
                                {
                                    fillCol = Brushes.Transparent;
                                }
                                else
                                {
                                    fillCol = new SolidColorBrush(shapeInfo.fillColor);
                                }
                                if ((shapeInfo.borderColor.R == 255) && (shapeInfo.borderColor.G == 255) && (shapeInfo.borderColor.B == 255))
                                {
                                    borderCol = Brushes.Transparent;
                                }
                                else
                                {
                                    borderCol = new SolidColorBrush(shapeInfo.borderColor);
                                }


                                switch (data[0])
                                {
                                    case "0":
                                        //Rectangle
                                        CustomRectangle customRectangle = new CustomRectangle(shapeInfo.shapeID, shapeInfo.startPoint,
                                                                                                shapeInfo.endPoint, shapeInfo.borderType);
                                        customRectangle.Width = Math.Abs(shapeInfo.endPoint.X - shapeInfo.startPoint.X);
                                        customRectangle.Height = Math.Abs(shapeInfo.endPoint.Y - shapeInfo.startPoint.Y);
                                        customRectangle.Stroke = borderCol;
                                        customRectangle.Fill = fillCol;
                                        customRectangle.StrokeThickness = (double)shapeInfo.borderWidth;
                                        switch ((int)shapeInfo.borderType)
                                        {
                                            case (int)ShapeInformation.BorderType.Solid:
                                                customRectangle.StrokeDashArray = solid;
                                                break;
                                            case (int)ShapeInformation.BorderType.DashDot:
                                                customRectangle.StrokeDashArray = dashDot;
                                                break;
                                            case (int)ShapeInformation.BorderType.Dot:
                                                customRectangle.StrokeDashArray = dot;
                                                break;
                                        }
                                        customRectangle.borderType = shapeInfo.borderType;
                                        customRectangle.HorizontalAlignment = HorizontalAlignment.Left;
                                        customRectangle.VerticalAlignment = VerticalAlignment.Center;
                                        if ((shapeInfo.startPoint.X < shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y < shapeInfo.endPoint.Y))
                                        {
                                            drawLoc = shapeInfo.startPoint;
                                        }
                                        else if ((shapeInfo.startPoint.X < shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y > shapeInfo.endPoint.Y))
                                        {
                                            drawLoc.X = shapeInfo.startPoint.X;
                                            drawLoc.Y = shapeInfo.endPoint.Y;
                                        }
                                        else if ((shapeInfo.startPoint.X > shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y < shapeInfo.endPoint.Y))
                                        {
                                            drawLoc.X = shapeInfo.endPoint.X;
                                            drawLoc.Y = shapeInfo.startPoint.Y;
                                        }
                                        else if ((shapeInfo.startPoint.X > shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y > shapeInfo.endPoint.Y))
                                        {
                                            drawLoc = shapeInfo.endPoint;
                                        }
                                        Canvas.SetLeft(customRectangle, drawLoc.X);
                                        Canvas.SetTop(customRectangle, drawLoc.Y);
                                        canvas.Children.Add(customRectangle);
                                        undoBtn.Visibility = Visibility.Visible;
                                        redoBtn.Visibility = Visibility.Hidden;
                                        break;
                                    case "1":
                                        //Ellipse
                                        CustomEllipse customEllipse = new CustomEllipse(shapeInfo.shapeID, shapeInfo.startPoint,
                                                                                                shapeInfo.endPoint, shapeInfo.borderType);
                                        customEllipse.Width = Math.Abs(shapeInfo.endPoint.X - shapeInfo.startPoint.X);
                                        customEllipse.Height = Math.Abs(shapeInfo.endPoint.Y - shapeInfo.startPoint.Y);
                                        customEllipse.Stroke = borderCol;
                                        customEllipse.Fill = fillCol;
                                        customEllipse.StrokeThickness = (double)shapeInfo.borderWidth;
                                        switch ((int)shapeInfo.borderType)
                                        {
                                            case (int)ShapeInformation.BorderType.Solid:
                                                customEllipse.StrokeDashArray = solid;
                                                break;
                                            case (int)ShapeInformation.BorderType.DashDot:
                                                customEllipse.StrokeDashArray = dashDot;
                                                break;
                                            case (int)ShapeInformation.BorderType.Dot:
                                                customEllipse.StrokeDashArray = dot;
                                                break;
                                        }
                                        customEllipse.borderType = shapeInfo.borderType;
                                        customEllipse.HorizontalAlignment = HorizontalAlignment.Left;
                                        customEllipse.VerticalAlignment = VerticalAlignment.Center;
                                        if ((shapeInfo.startPoint.X < shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y < shapeInfo.endPoint.Y))
                                        {
                                            drawLoc = shapeInfo.startPoint;
                                        }
                                        else if ((shapeInfo.startPoint.X < shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y > shapeInfo.endPoint.Y))
                                        {
                                            drawLoc.X = shapeInfo.startPoint.X;
                                            drawLoc.Y = shapeInfo.endPoint.Y;
                                        }
                                        else if ((shapeInfo.startPoint.X > shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y < shapeInfo.endPoint.Y))
                                        {
                                            drawLoc.X = shapeInfo.endPoint.X;
                                            drawLoc.Y = shapeInfo.startPoint.Y;
                                        }
                                        else if ((shapeInfo.startPoint.X > shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y > shapeInfo.endPoint.Y))
                                        {
                                            drawLoc = shapeInfo.endPoint;
                                        }
                                        Canvas.SetLeft(customEllipse, drawLoc.X);
                                        Canvas.SetTop(customEllipse, drawLoc.Y);
                                        canvas.Children.Add(customEllipse);
                                        undoBtn.Visibility = Visibility.Visible;
                                        redoBtn.Visibility = Visibility.Hidden;
                                        break;
                                    case "2":
                                        //Right Triangle
                                        CustomRightTriangle customRightTriangle = new CustomRightTriangle(shapeInfo.shapeID, shapeInfo.startPoint,
                                                                                                shapeInfo.endPoint, shapeInfo.borderType);
                                        customRightTriangle.Width = Math.Abs(shapeInfo.endPoint.X - shapeInfo.startPoint.X);
                                        customRightTriangle.Height = Math.Abs(shapeInfo.endPoint.Y - shapeInfo.startPoint.Y);
                                        customRightTriangle.Stroke = borderCol;
                                        customRightTriangle.Fill = fillCol;
                                        customRightTriangle.StrokeThickness = (double)shapeInfo.borderWidth;
                                        switch ((int)shapeInfo.borderType)
                                        {
                                            case (int)ShapeInformation.BorderType.Solid:
                                                customRightTriangle.StrokeDashArray = solid;
                                                break;
                                            case (int)ShapeInformation.BorderType.DashDot:
                                                customRightTriangle.StrokeDashArray = dashDot;
                                                break;
                                            case (int)ShapeInformation.BorderType.Dot:
                                                customRightTriangle.StrokeDashArray = dot;
                                                break;
                                        }
                                        customRightTriangle.borderType = shapeInfo.borderType;
                                        customRightTriangle.HorizontalAlignment = HorizontalAlignment.Left;
                                        customRightTriangle.VerticalAlignment = VerticalAlignment.Center;
                                        if ((shapeInfo.startPoint.X < shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y < shapeInfo.endPoint.Y))
                                        {
                                            drawLoc = shapeInfo.startPoint;
                                        }
                                        else if ((shapeInfo.startPoint.X < shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y > shapeInfo.endPoint.Y))
                                        {
                                            drawLoc.X = shapeInfo.startPoint.X;
                                            drawLoc.Y = shapeInfo.endPoint.Y;
                                        }
                                        else if ((shapeInfo.startPoint.X > shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y < shapeInfo.endPoint.Y))
                                        {
                                            drawLoc.X = shapeInfo.endPoint.X;
                                            drawLoc.Y = shapeInfo.startPoint.Y;
                                        }
                                        else if ((shapeInfo.startPoint.X > shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y > shapeInfo.endPoint.Y))
                                        {
                                            drawLoc = shapeInfo.endPoint;
                                        }
                                        Canvas.SetLeft(customRightTriangle, drawLoc.X);
                                        Canvas.SetTop(customRightTriangle, drawLoc.Y);
                                        canvas.Children.Add(customRightTriangle);
                                        undoBtn.Visibility = Visibility.Visible;
                                        redoBtn.Visibility = Visibility.Hidden;
                                        break;
                                    case "3":
                                        //Diamond
                                        CustomDiamond customDiamond = new CustomDiamond(shapeInfo.shapeID, shapeInfo.startPoint,
                                                                                                shapeInfo.endPoint, shapeInfo.borderType);
                                        customDiamond.Width = Math.Abs(shapeInfo.endPoint.X - shapeInfo.startPoint.X);
                                        customDiamond.Height = Math.Abs(shapeInfo.endPoint.Y - shapeInfo.startPoint.Y);
                                        customDiamond.Stroke = borderCol;
                                        customDiamond.Fill = fillCol;
                                        customDiamond.StrokeThickness = (double)shapeInfo.borderWidth;
                                        switch ((int)shapeInfo.borderType)
                                        {
                                            case (int)ShapeInformation.BorderType.Solid:
                                                customDiamond.StrokeDashArray = solid;
                                                break;
                                            case (int)ShapeInformation.BorderType.DashDot:
                                                customDiamond.StrokeDashArray = dashDot;
                                                break;
                                            case (int)ShapeInformation.BorderType.Dot:
                                                customDiamond.StrokeDashArray = dot;
                                                break;
                                        }
                                        customDiamond.borderType = shapeInfo.borderType;
                                        customDiamond.HorizontalAlignment = HorizontalAlignment.Left;
                                        customDiamond.VerticalAlignment = VerticalAlignment.Center;
                                        if ((shapeInfo.startPoint.X < shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y < shapeInfo.endPoint.Y))
                                        {
                                            drawLoc = shapeInfo.startPoint;
                                        }
                                        else if ((shapeInfo.startPoint.X < shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y > shapeInfo.endPoint.Y))
                                        {
                                            drawLoc.X = shapeInfo.startPoint.X;
                                            drawLoc.Y = shapeInfo.endPoint.Y;
                                        }
                                        else if ((shapeInfo.startPoint.X > shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y < shapeInfo.endPoint.Y))
                                        {
                                            drawLoc.X = shapeInfo.endPoint.X;
                                            drawLoc.Y = shapeInfo.startPoint.Y;
                                        }
                                        else if ((shapeInfo.startPoint.X > shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y > shapeInfo.endPoint.Y))
                                        {
                                            drawLoc = shapeInfo.endPoint;
                                        }
                                        Canvas.SetLeft(customDiamond, drawLoc.X);
                                        Canvas.SetTop(customDiamond, drawLoc.Y);
                                        canvas.Children.Add(customDiamond);
                                        undoBtn.Visibility = Visibility.Visible;
                                        redoBtn.Visibility = Visibility.Hidden;
                                        break;
                                    case "4":
                                        //Isoceles Triangle
                                        CustomIsocelesTriangle customIsocelesTriangle = new CustomIsocelesTriangle(shapeInfo.shapeID, shapeInfo.startPoint,
                                                                                                shapeInfo.endPoint, shapeInfo.borderType);
                                        customIsocelesTriangle.Width = Math.Abs(shapeInfo.endPoint.X - shapeInfo.startPoint.X);
                                        customIsocelesTriangle.Height = Math.Abs(shapeInfo.endPoint.Y - shapeInfo.startPoint.Y);
                                        customIsocelesTriangle.Stroke = borderCol;
                                        customIsocelesTriangle.Fill = fillCol;
                                        customIsocelesTriangle.StrokeThickness = (double)shapeInfo.borderWidth;
                                        switch ((int)shapeInfo.borderType)
                                        {
                                            case (int)ShapeInformation.BorderType.Solid:
                                                customIsocelesTriangle.StrokeDashArray = solid;
                                                break;
                                            case (int)ShapeInformation.BorderType.DashDot:
                                                customIsocelesTriangle.StrokeDashArray = dashDot;
                                                break;
                                            case (int)ShapeInformation.BorderType.Dot:
                                                customIsocelesTriangle.StrokeDashArray = dot;
                                                break;
                                        }
                                        customIsocelesTriangle.borderType = shapeInfo.borderType;
                                        customIsocelesTriangle.HorizontalAlignment = HorizontalAlignment.Left;
                                        customIsocelesTriangle.VerticalAlignment = VerticalAlignment.Center;
                                        if ((shapeInfo.startPoint.X < shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y < shapeInfo.endPoint.Y))
                                        {
                                            drawLoc = shapeInfo.startPoint;
                                        }
                                        else if ((shapeInfo.startPoint.X < shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y > shapeInfo.endPoint.Y))
                                        {
                                            drawLoc.X = shapeInfo.startPoint.X;
                                            drawLoc.Y = shapeInfo.endPoint.Y;
                                        }
                                        else if ((shapeInfo.startPoint.X > shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y < shapeInfo.endPoint.Y))
                                        {
                                            drawLoc.X = shapeInfo.endPoint.X;
                                            drawLoc.Y = shapeInfo.startPoint.Y;
                                        }
                                        else if ((shapeInfo.startPoint.X > shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y > shapeInfo.endPoint.Y))
                                        {
                                            drawLoc = shapeInfo.endPoint;
                                        }
                                        Canvas.SetLeft(customIsocelesTriangle, drawLoc.X);
                                        Canvas.SetTop(customIsocelesTriangle, drawLoc.Y);
                                        canvas.Children.Add(customIsocelesTriangle);
                                        undoBtn.Visibility = Visibility.Visible;
                                        redoBtn.Visibility = Visibility.Hidden;
                                        break;
                                    case "5":
                                        //Hexagon
                                        CustomHexagon customHexagon = new CustomHexagon(shapeInfo.shapeID, shapeInfo.startPoint,
                                                                                                shapeInfo.endPoint, shapeInfo.borderType);
                                        customHexagon.Width = Math.Abs(shapeInfo.endPoint.X - shapeInfo.startPoint.X);
                                        customHexagon.Height = Math.Abs(shapeInfo.endPoint.Y - shapeInfo.startPoint.Y);
                                        customHexagon.Stroke = borderCol;
                                        customHexagon.Fill = fillCol;
                                        customHexagon.StrokeThickness = (double)shapeInfo.borderWidth;
                                        switch ((int)shapeInfo.borderType)
                                        {
                                            case (int)ShapeInformation.BorderType.Solid:
                                                customHexagon.StrokeDashArray = solid;
                                                break;
                                            case (int)ShapeInformation.BorderType.DashDot:
                                                customHexagon.StrokeDashArray = dashDot;
                                                break;
                                            case (int)ShapeInformation.BorderType.Dot:
                                                customHexagon.StrokeDashArray = dot;
                                                break;
                                        }
                                        customHexagon.borderType = shapeInfo.borderType;
                                        customHexagon.HorizontalAlignment = HorizontalAlignment.Left;
                                        customHexagon.VerticalAlignment = VerticalAlignment.Center;
                                        if ((shapeInfo.startPoint.X < shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y < shapeInfo.endPoint.Y))
                                        {
                                            drawLoc = shapeInfo.startPoint;
                                        }
                                        else if ((shapeInfo.startPoint.X < shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y > shapeInfo.endPoint.Y))
                                        {
                                            drawLoc.X = shapeInfo.startPoint.X;
                                            drawLoc.Y = shapeInfo.endPoint.Y;
                                        }
                                        else if ((shapeInfo.startPoint.X > shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y < shapeInfo.endPoint.Y))
                                        {
                                            drawLoc.X = shapeInfo.endPoint.X;
                                            drawLoc.Y = shapeInfo.startPoint.Y;
                                        }
                                        else if ((shapeInfo.startPoint.X > shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y > shapeInfo.endPoint.Y))
                                        {
                                            drawLoc = shapeInfo.endPoint;
                                        }
                                        Canvas.SetLeft(customHexagon, drawLoc.X);
                                        Canvas.SetTop(customHexagon, drawLoc.Y);
                                        canvas.Children.Add(customHexagon);
                                        undoBtn.Visibility = Visibility.Visible;
                                        redoBtn.Visibility = Visibility.Hidden;
                                        break;
                                    case "6":
                                        //Pentagon
                                        CustomPentagon customPentagon = new CustomPentagon(shapeInfo.shapeID, shapeInfo.startPoint,
                                                                                                shapeInfo.endPoint, shapeInfo.borderType);
                                        customPentagon.Width = Math.Abs(shapeInfo.endPoint.X - shapeInfo.startPoint.X);
                                        customPentagon.Height = Math.Abs(shapeInfo.endPoint.Y - shapeInfo.startPoint.Y);
                                        customPentagon.Stroke = borderCol;
                                        customPentagon.Fill = fillCol;
                                        customPentagon.StrokeThickness = (double)shapeInfo.borderWidth;
                                        switch ((int)shapeInfo.borderType)
                                        {
                                            case (int)ShapeInformation.BorderType.Solid:
                                                customPentagon.StrokeDashArray = solid;
                                                break;
                                            case (int)ShapeInformation.BorderType.DashDot:
                                                customPentagon.StrokeDashArray = dashDot;
                                                break;
                                            case (int)ShapeInformation.BorderType.Dot:
                                                customPentagon.StrokeDashArray = dot;
                                                break;
                                        }
                                        customPentagon.borderType = shapeInfo.borderType;
                                        customPentagon.HorizontalAlignment = HorizontalAlignment.Left;
                                        customPentagon.VerticalAlignment = VerticalAlignment.Center;
                                        if ((shapeInfo.startPoint.X < shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y < shapeInfo.endPoint.Y))
                                        {
                                            drawLoc = shapeInfo.startPoint;
                                        }
                                        else if ((shapeInfo.startPoint.X < shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y > shapeInfo.endPoint.Y))
                                        {
                                            drawLoc.X = shapeInfo.startPoint.X;
                                            drawLoc.Y = shapeInfo.endPoint.Y;
                                        }
                                        else if ((shapeInfo.startPoint.X > shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y < shapeInfo.endPoint.Y))
                                        {
                                            drawLoc.X = shapeInfo.endPoint.X;
                                            drawLoc.Y = shapeInfo.startPoint.Y;
                                        }
                                        else if ((shapeInfo.startPoint.X > shapeInfo.endPoint.X) && (shapeInfo.startPoint.Y > shapeInfo.endPoint.Y))
                                        {
                                            drawLoc = shapeInfo.endPoint;
                                        }
                                        Canvas.SetLeft(customPentagon, drawLoc.X);
                                        Canvas.SetTop(customPentagon, drawLoc.Y);
                                        canvas.Children.Add(customPentagon);
                                        undoBtn.Visibility = Visibility.Visible;
                                        redoBtn.Visibility = Visibility.Hidden;
                                        break;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Cannot open file: the .shp file is corrupt.");
                }
                shpFilePath = dlg.FileName;
                canvas.UpdateLayout();
                MessageBox.Show(System.IO.Path.GetFileName(dlg.FileName + " has been loaded."));
            }
        }
        #endregion

            #region .shp Saving
        private string outputBG_RGB()
        {
            string bgData;
            if (backgroundImgExists == 0)
            {
                bgColor = ((SolidColorBrush)canvas.Background).Color;
                bgData = bgColor.R + " " + bgColor.G + " " + bgColor.B;
            }
            else
            {
                bgColor.R = 0;
                bgColor.G = 0;
                bgColor.B = 0;
                bgData = bgColor.R + " " + bgColor.G + " " + bgColor.B;
            }
            return bgData;
        }

        private string output_BG()
        {
            if (backgroundImgExists == 1)
            {
                return backgroundImgExists + " " + System.IO.Path.GetFileName(bgFileName);
            }
            else
            {
                return backgroundImgExists.ToString();
            }
        }

        private string output_Shapes()
        {
            ShapeInformation.ShapeInfo shapeInfo;
            ShapeInformation.FillType fillType;

            StringBuilder shapeString = new StringBuilder();

            tempRect = (Rectangle)canvas.Children[0];
            canvas.Children.RemoveAt(0);
            foreach (CustomShape customShape in canvas.Children)
            {
                //Set shapeType
                if (customShape.GetBaseFillColor() == Brushes.Transparent)
                {
                    fillType = ShapeInformation.FillType.Drawn;
                }
                else
                {
                    fillType = ShapeInformation.FillType.Filled;
                }

                shapeInfo = new ShapeInformation.ShapeInfo(customShape.ShapeID, customShape.ShapeType, fillType,
                                                        customShape.StartPoint, customShape.EndPoint,
                                                        ((SolidColorBrush)customShape.GetBaseFillColor()).Color,
                                                        ((SolidColorBrush)customShape.GetBaseBorderColor()).Color,
                                                        customShape.BorderType, (int)customShape.GetStrokeThickness);
                shapeString.Append(shapeInfo.ToString() + Environment.NewLine);

            }
            canvas.Children.Insert(0, tempRect);
            return shapeString.ToString();
        }

        private string saveCanvas()
        {
            return outputBG_RGB() + Environment.NewLine
                + output_BG() + Environment.NewLine
                + numShapesCreated + Environment.NewLine
                + output_Shapes();
        }

        private void stripClick_Save(object sender, MouseButtonEventArgs e)
        {
            if (!File.Exists(shpFilePath))
            {
                //Dialog: Save .shp file
                //Configure open file dialog box
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                dlg.FileName = "shapeData"; // Default file name
                dlg.DefaultExt = ".shp"; // Default file extension
                dlg.Filter = "Shape Image Data (.shp)|*.shp"; // Filter files by extension
                dlg.AddExtension = true;
                //Show open file dialog box
                Nullable<bool> result = dlg.ShowDialog();
                //Process open file dialog box results 
                if (result == true)
                {
                    using (StreamWriter shpWriter = new StreamWriter(dlg.FileName))
                    {
                        shpWriter.Write(saveCanvas());
                        shpFilePath = dlg.FileName;
                        MessageBox.Show(System.IO.Path.GetFileName(dlg.FileName + " has been saved."));
                    }
                }
            }
            else
            {
                using (StreamWriter shpWriter = new StreamWriter(shpFilePath))
                {
                    shpWriter.Write(saveCanvas());
                }
                MessageBox.Show(System.IO.Path.GetFileName(System.IO.Path.GetFileName(shpFilePath) + " has been saved."));
            }
        }
        #endregion

            #region Undo/Redo
        private void stripClick_Undo(object sender, MouseButtonEventArgs e)
        {
            tempRect = (Rectangle)canvas.Children[0];
            canvas.Children.RemoveAt(0);
            //Undo
            if (canvas.Children.Count != 0)
            {
                tempShape = ((CustomShape)canvas.Children[canvas.Children.Count - 1]);
                canvas.Children.RemoveAt(canvas.Children.Count - 1);
                numShapesCreated--;
                if (canvas.Children.Count == 0)
                {
                    undoBtn.Visibility = Visibility.Hidden;
                }
            }
            redoBtn.Visibility = Visibility.Visible;
            canvas.Children.Insert(0, tempRect);
        }

        private void stripClick_Redo(object sender, MouseButtonEventArgs e)
        {
            tempRect = (Rectangle)canvas.Children[0];
            canvas.Children.RemoveAt(0);
            //Redo
            if (tempShape != null)
            {
                switch (tempShape.ShapeType)
                {
                    case ShapeInformation.ShapeType.Rectangle:
                        canvas.Children.Add((CustomRectangle)tempShape);
                        break;
                    case ShapeInformation.ShapeType.Ellipse:
                        canvas.Children.Add((CustomEllipse)tempShape);
                        break;
                    case ShapeInformation.ShapeType.Right_Triangle:
                        canvas.Children.Add((CustomRightTriangle)tempShape);
                        break;
                    case ShapeInformation.ShapeType.Diamond:
                        canvas.Children.Add((CustomDiamond)tempShape);
                        break;
                    case ShapeInformation.ShapeType.Isoceles_Triangle:
                        canvas.Children.Add((CustomIsocelesTriangle)tempShape);
                        break;
                    case ShapeInformation.ShapeType.Hexagon:
                        canvas.Children.Add((CustomHexagon)tempShape);
                        break;
                    case ShapeInformation.ShapeType.Pentagon:
                        canvas.Children.Add((CustomPentagon)tempShape);
                        break;
                }
                numShapesCreated++;
                redoBtn.Visibility = Visibility.Hidden;
                tempShape = null;

                if (canvas.Children.Count != 0)
                {
                    undoBtn.Visibility = Visibility.Visible;
                }
            }
            canvas.Children.Insert(0, tempRect);
        }
        #endregion

            #region .shp Options
        private void stripClick_FillOptions(object sender, MouseButtonEventArgs e)
        {
            //Toggle: Fill/Drawn Options
            if (lineStyleImg.Source.Equals(lineToggleOff))
            {
                lineStyleImg.Source = lineToggleOn;
                this.fillColor = Brushes.Transparent;
                this.lineColor = lineColorRetrieved;
            }
            else
            {
                lineStyleImg.Source = lineToggleOff;
                this.lineColor = Brushes.Transparent;
                this.fillColor = fillColorRetrieved;
            }
        }

        private void stripClick_LineOptions(object sender, MouseButtonEventArgs e)
        {
            BorderStyleDialog lineOptions = new BorderStyleDialog();
            //Dialog: Line Solid/Dotted/Dashed Options
            location = canvasWindow.PointToScreen(new Point(0, 0));

            lineOptions.Left = location.X + 300;
            lineOptions.Top = location.Y + 136;
            lineOptions.ShowDialog();
            this.lineStyle = lineOptions.selectedLineStyle;
            this.lineWidth = lineOptions.selectedWidthValue;
            this.borderStyle = lineOptions.selectedBorderType;
            lineOptions.Close();
        }
        private void stripClick_BackgroundColorOptions(object sender, MouseButtonEventArgs e)
        {
            //Diaglog: Background Color Selection Dialog
            location = canvasWindow.PointToScreen(new Point(0, 0));

            try
            {
                bGColorOptions.Left = location.X + 300;
                bGColorOptions.Top = location.Y + 136;
                bGColorOptions.ShowDialog();
                canvas.Background = bGColorOptions.selectedColor;
                bGColorOptions.Hide();
                backgroundImgExists = 0;
            }
            catch (System.Windows.Markup.XamlParseException)
            {
                MessageBox.Show("Xceed.Wpf.Toolkit.dll is missing...\nPlease add the .dll to this folder.\n"
                                + "Force-closing the application to avoid bad program behavior.");
                Application.Current.Shutdown();
                return;
            }
        }

        private void stripClick_FillColorOptions(object sender, MouseButtonEventArgs e)
        {
            if (lineStyleImg.Source.Equals(lineToggleOn))
            {
                MessageBox.Show("Please enter Fill mode first before you change the Fill color.");
                return;
            }

            //Dialog: Fill Color Selection Dialog
            location = canvasWindow.PointToScreen(new Point(0, 0));
            try
            {
                fillColorOptions.Left = location.X + 300;
                fillColorOptions.Top = location.Y + 136;
                fillColorOptions.ShowDialog();
                this.fillColor = fillColorOptions.selectedColor;
                this.fillColorRetrieved = fillColorOptions.selectedColor;
                fillColorStatusBar.Background = fillColorOptions.selectedColor;
                fillColorOptions.Hide();
            }
            catch (System.Windows.Markup.XamlParseException)
            {
                MessageBox.Show("Xceed.Wpf.Toolkit.dll is missing...\nPlease add the .dll to this folder.\n"
                                + "Force-closing the application to avoid bad program behavior.");
                Application.Current.Shutdown();
                return;
            }
        }

        private void stripClick_LineColorOptions(object sender, MouseButtonEventArgs e)
        {
            if (lineStyleImg.Source.Equals(lineToggleOff))
            {
                MessageBox.Show("Please enter Draw mode first before you change the Line color.");
                return;
            }

            //Dialog: Line Color Selection Dialog
            location = canvasWindow.PointToScreen(new Point(0, 0));
            try
            {
                lineColorOptions.Left = location.X + 300;
                lineColorOptions.Top = location.Y + 136;
                lineColorOptions.ShowDialog();
                this.lineColor = lineColorOptions.selectedColor;
                this.lineColorRetrieved = lineColorOptions.selectedColor;
                lineColorOptions.Hide();
            }
            catch (System.Windows.Markup.XamlParseException)
            {
                MessageBox.Show("Xceed.Wpf.Toolkit.dll is missing...\nPlease add the .dll to this folder.\n"
                                + "Force-closing the application to avoid bad program behavior.");
                Application.Current.Shutdown();
                return;
            }
        }
        #endregion

            #region Open/Saving Images
        private void stripClick_OpenImage(object sender, MouseButtonEventArgs e)
        {
            //Dialog: Open .jpeg/.gif/.bmp Options as Canvas Background
            string filename = "";

            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            dlg.FileName = "canvas";
            dlg.DefaultExt =".jpg";
            dlg.Filter = "Image files (*.jpeg, *.gif, *.bmp)|*.jpeg;*.jpg;*.gif;*.bmp|All files (*.*)|*.*"; // Filter files by extension
            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            // Process open file dialog box results 
            if (result == true)
            {
                filename = dlg.FileName;

                ImageBrush background = new ImageBrush(new BitmapImage(new Uri(filename, UriKind.Absolute)));
                background.Stretch = Stretch.UniformToFill;
                canvas.Background = background;
                bgFileName = filename;
                backgroundImgExists = 1;
            }
            imgFilePath = dlg.FileName;
            opnFilePath = imgFilePath;
            MessageBox.Show(System.IO.Path.GetFileName(dlg.FileName + " has been loaded."));
        }

        private void stripClick_SaveImage(object sender, MouseButtonEventArgs e)
        {
            //Dialog: Save .jpeg/.gif/.bmp Options as Image

            string filename = "";
            string extension = "";

            if (!File.Exists(imgFilePath))
            {
                // Configure open file dialog box
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                dlg.DefaultExt = ".jpeg"; // Default file extension
                dlg.Filter = ".jpeg|*.jpg|.gif|*.gif|.bmp|*.bmp"; // Filter files by extension
                dlg.AddExtension = true;
                // Show open file dialog box
                Nullable<bool> result = dlg.ShowDialog();
                // Process open file dialog box results 
                if (result == true)
                {
                    filename = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                    extension = System.IO.Path.GetExtension(dlg.FileName);
                }
                else
                {
                    filename = "canvas";
                    extension = ".jpg";
                }
            }
            else
            {
                filename = System.IO.Path.GetFileNameWithoutExtension(imgFilePath);
                extension = System.IO.Path.GetExtension(imgFilePath);
            }

            string defaultFilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string temp = "temp.jpg";
            string output = filename + extension;
            int incermenter = 1; ;
            if (output == System.IO.Path.GetFileName(opnFilePath))
            {
                while (File.Exists(output))
                {
                    output = filename + "_" + incermenter + extension;
                    incermenter++;
                }
            }
            Stream x = File.Create(temp);
            Transform transform = canvas.LayoutTransform;
            System.Windows.Size sizeOfControl = new System.Windows.Size(canvas.ActualWidth, canvas.ActualHeight);
            canvas.Measure(sizeOfControl);
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((Int32)sizeOfControl.Width, (Int32)sizeOfControl.Height + 72,
                                                                                                    96d, 96d, PixelFormats.Pbgra32);
            renderBitmap.Render(canvas);
            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            pngEncoder.Save(x);
            x.Close();


            using (System.Drawing.Bitmap cropped = new System.Drawing.Bitmap(temp))
            {
                System.Drawing.Bitmap tempBitmap = bitmapCrop(cropped, new System.Drawing.Rectangle(0, 72, 884, 463));
                tempBitmap.Save(output);
                cropped.Dispose();
                tempBitmap.Dispose();
            }
            imgFilePath = defaultFilePath + "/" + output;
            File.Delete(temp);
            MessageBox.Show(System.IO.Path.GetFileName(output + " has been saved."));
        }

        static public System.Drawing.Bitmap bitmapCrop(System.Drawing.Bitmap srcBitmap, System.Drawing.Rectangle section)
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap((int)section.Width, (int)section.Height);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
            g.DrawImage(srcBitmap, 0, 0, section, System.Drawing.GraphicsUnit.Pixel);
            g.Dispose();
            return bmp;
        }
        #endregion

            #region About
        private void stripClick_About(object sender, MouseButtonEventArgs e)
        {
            AboutBox lineOptions = new AboutBox();
            //Dialog: Line Solid/Dotted/Dashed Options
            location = canvasWindow.PointToScreen(new Point(0, 0));

            lineOptions.Left = location.X + 200;
            lineOptions.Top = location.Y + 136;
            lineOptions.ShowDialog();
        }
        #endregion

        #endregion

        #region menuClick Events
        private void menuClick_Open(object sender, RoutedEventArgs e)
        {
            stripClick_Open(null, null);
        }

        private void menuClick_Save(object sender, RoutedEventArgs e)
        {
            stripClick_Save(null, null);
        }

        private void menuClick_SaveAs(object sender, RoutedEventArgs e)
        {
            //Dialog: Save .shp file
            //Configure open file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            dlg.FileName = "shapeData"; // Default file name
            dlg.DefaultExt = ".shp"; // Default file extension
            dlg.Filter = "Shape Image Data (.shp)|*.shp"; // Filter files by extension
            dlg.AddExtension = true;
            //Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            //Process open file dialog box results 
            if (result == true)
            {
                using (StreamWriter shpWriter = new StreamWriter(dlg.FileName))
                {
                    shpWriter.Write(saveCanvas());
                    shpFilePath = dlg.FileName;
                    MessageBox.Show(System.IO.Path.GetFileName(dlg.FileName + " has been saved."));
                }
            }
        }

        private void menuClick_Exit(object sender, RoutedEventArgs e)
        {
            fillColorOptions.Close();
            lineColorOptions.Close();
            bGColorOptions.Close();
            Application.Current.Shutdown();
            return;
        }

        private void menuClick_Undo(object sender, RoutedEventArgs e)
        {
            stripClick_Undo(null, null);
        }

        private void menuClick_Redo(object sender, RoutedEventArgs e)
        {
            stripClick_Redo(null, null);
        }

        private void menuClick_ClearScreen(object sender, RoutedEventArgs e)
        {
            //Clear screen of all shapes and reset background
            canvas.Children.Clear();
            canvas.Background = Brushes.Black;
            bgFileName = "";
            backgroundImgExists = 0;
            numShapesCreated = 0;
        }

        private void menuClick_OpenImage(object sender, RoutedEventArgs e)
        {
            stripClick_OpenImage(null, null);
        }

        private void menuClick_SaveImage(object sender, RoutedEventArgs e)
        {
            stripClick_SaveImage(null, null);
        }

        private void menuClick_SaveImageAs(object sender, RoutedEventArgs e)
        {
            //Dialog: Save .jpeg/.gif/.bmp Options as Image

            string filename = "";
            string extension = "";

            // Configure open file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            dlg.DefaultExt = ".jpeg"; // Default file extension
            dlg.Filter = ".jpeg|*.jpg|.gif|*.gif|.bmp|*.bmp"; // Filter files by extension
            dlg.AddExtension = true;
            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            // Process open file dialog box results 
            if (result == true)
            {
                filename = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                extension = System.IO.Path.GetExtension(dlg.FileName);
            }
            else
            {
                filename = "canvas";
                extension = ".jpg";
            }

            string defaultFilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string temp = "temp.jpg";
            string output = filename + extension;
            int incermenter = 1; ;
            if (output == System.IO.Path.GetFileName(opnFilePath))
            {
                while (File.Exists(output))
                {
                    output = filename + "_" + incermenter + extension;
                    incermenter++;
                }
            }
            Stream x = File.Create(temp);
            Transform transform = canvas.LayoutTransform;
            System.Windows.Size sizeOfControl = new System.Windows.Size(canvas.ActualWidth, canvas.ActualHeight);
            canvas.Measure(sizeOfControl);
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((Int32)sizeOfControl.Width, (Int32)sizeOfControl.Height + 72,
                                                                                                    96d, 96d, PixelFormats.Pbgra32);
            renderBitmap.Render(canvas);
            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            pngEncoder.Save(x);
            x.Close();


            using (System.Drawing.Bitmap cropped = new System.Drawing.Bitmap(temp))
            {
                System.Drawing.Bitmap tempBitmap = bitmapCrop(cropped, new System.Drawing.Rectangle(0, 72, 884, 463));
                tempBitmap.Save(output);
                cropped.Dispose();
                tempBitmap.Dispose();
            }
            imgFilePath = defaultFilePath + "/" + output;
            File.Delete(temp);
            MessageBox.Show(System.IO.Path.GetFileName(output + " has been saved."));
        }

        private void menuClick_Line(object sender, RoutedEventArgs e)
        {
            stripClick_LineOptions(null, null);
        }

        private void menuClick_FillColor(object sender, RoutedEventArgs e)
        {
            stripClick_FillColorOptions(null, null);
        }

        private void menuClick_LineColor(object sender, RoutedEventArgs e)
        {
            stripClick_LineColorOptions(null, null);
        }

        private void menuClick_BackgroundColor(object sender, RoutedEventArgs e)
        {
            stripClick_BackgroundColorOptions(null, null);
        }

        private void menuClick_About(object sender, RoutedEventArgs e)
        {
            stripClick_About(null, null);
        }
        #endregion

        #region Hotkeys
        private void canvasWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Declared handled. Don't climb furthur up the chain.
            e.Handled = true;

            //Special case for alt key (unused).
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            //Return without doing anything when only modifier keys are held.
            if (key == Key.LeftShift || key == Key.RightShift
                || key == Key.LeftCtrl || key == Key.RightCtrl
                || key == Key.LeftAlt || key == Key.RightAlt
                || key == Key.LWin || key == Key.RWin)
            {
                return;
            }

            // Build the shortcut key name.
            StringBuilder shortcutText = new StringBuilder();
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                shortcutText.Append("Ctrl+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                shortcutText.Append("Shift+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            {
                shortcutText.Append("Alt+");
            }
            shortcutText.Append(key);

            switch (shortcutText.ToString())
            {
                case "Ctrl+O":
                    menuClick_Open(null, null);
                    break;
                case "Ctrl+S":
                    menuClick_Save(null, null);
                    break;
                case "Ctrl+A":
                    menuClick_SaveAs(null, null);
                    break;
                case "Ctrl+X":
                    menuClick_Exit(null, null);
                    break;
                case "Ctrl+Z":
                    stripClick_Undo(null, null);
                    break;
                case "Ctrl+Y":
                    stripClick_Redo(null, null);
                    break;
                case "Ctrl+Shift+C":
                    menuClick_ClearScreen(null, null);
                    break;
                case "Ctrl+Shift+O":
                    stripClick_OpenImage(null, null);
                    break;
                case "Ctrl+Shift+S":
                    menuClick_SaveImage(null, null);
                    break;
                case "Ctrl+Shift+A":
                    menuClick_SaveImageAs(null, null);
                    break;
                case "Ctrl+L":
                    stripClick_LineOptions(null, null);
                    break;
                case "Ctrl+Shift+F":
                    stripClick_FillColorOptions(null, null);
                    break;
                case "Ctrl+Shift+L":
                    stripClick_LineColorOptions(null, null);
                    break;
                case "Ctrl+Shift+B":
                    stripClick_BackgroundColorOptions(null, null);
                    break;
                case "Ctrl+H":
                    stripClick_About(null, null);
                    break;
                default:
                    return;
            }
        }
        #endregion

        #region Status Bar
            #region Tooltip Opening
        private void Button_ToolTipOpening_1(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + ((Button)sender).ToolTip;
        }

        private void Button_ToolTipOpening_2(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + ((Button)sender).ToolTip;
        }

        private void undoBtn_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + ((Button)sender).ToolTip;
        }

        private void redoBtn_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + ((Button)sender).ToolTip;
        }

        private void Button_ToolTipOpening_3(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + ((Button)sender).ToolTip;
        }

        private void Button_ToolTipOpening_4(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + ((Button)sender).ToolTip;
        }

        private void Button_ToolTipOpening_5(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + ((Button)sender).ToolTip;
        }

        private void Button_ToolTipOpening_6(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + ((Button)sender).ToolTip;
        }

        private void Button_ToolTipOpening_7(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + ((Button)sender).ToolTip;
        }

        private void Button_ToolTipOpening_8(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + ((Button)sender).ToolTip;
        }

        private void Button_ToolTipOpening_9(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + ((Button)sender).ToolTip;
        }

        private void Button_ToolTipOpening_10(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + ((Button)sender).ToolTip;
        }
        #endregion

            #region Tooltip Closing
        private void Button_ToolTipClosing_1(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + "--Hover over a control!--";
        }

        private void Button_ToolTipClosing_2(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + "--Hover over a control!--";
        }

        private void undoBtn_ToolTipClosing(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + "--Hover over a control!--";
        }

        private void redoBtn_ToolTipClosing(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + "--Hover over a control!--";
        }

        private void Button_ToolTipClosing_3(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + "--Hover over a control!--";
        }

        private void Button_ToolTipClosing_4(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + "--Hover over a control!--";
        }

        private void Button_ToolTipClosing_5(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + "--Hover over a control!--";
        }

        private void Button_ToolTipClosing_6(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + "--Hover over a control!--";
        }

        private void Button_ToolTipClosing_7(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + "--Hover over a control!--";
        }

        private void Button_ToolTipClosing_8(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + "--Hover over a control!--";
        }

        private void Button_ToolTipClosing_9(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + "--Hover over a control!--";
        }

        private void Button_ToolTipClosing_10(object sender, ToolTipEventArgs e)
        {
            status.Content = "Status: " + "--Hover over a control!--";
        }
        #endregion

            #region Menu
                #region mouseOver Menu
        private void MenuItem_MouseEnter_1(object sender, MouseEventArgs e)
        {
            status.Content = "Status: Open a .shp file";
        }

        private void MenuItem_MouseEnter_2(object sender, MouseEventArgs e)
        {
            status.Content = "Status: Save a .shp file";
        }

        private void MenuItem_MouseEnter_3(object sender, MouseEventArgs e)
        {
            status.Content = "Status: Save a .shp file";
        }

        private void MenuItem_MouseEnter_4(object sender, MouseEventArgs e)
        {
            status.Content = "Status: Close the application";
        }

        private void MenuItem_MouseEnter_5(object sender, MouseEventArgs e)
        {
            status.Content = "Status: Undo last draw";
        }

        private void MenuItem_MouseEnter_6(object sender, MouseEventArgs e)
        {
            status.Content = "Status: Redo last draw";
        }

        private void MenuItem_MouseEnter_7(object sender, MouseEventArgs e)
        {
            status.Content = "Status: Line Options";
        }

        private void MenuItem_MouseEnter_8(object sender, MouseEventArgs e)
        {
            status.Content = "Status: Fill Color";
        }

        private void MenuItem_MouseEnter_9(object sender, MouseEventArgs e)
        {
            status.Content = "Status: Line Color";
        }

        private void MenuItem_MouseEnter_10(object sender, MouseEventArgs e)
        {
            status.Content = "Status: Background Color";
        }

        private void MenuItem_MouseEnter_11(object sender, MouseEventArgs e)
        {
            status.Content = "Status: About this program!";
        }

        private void MenuItem_MouseEnter_12(object sender, MouseEventArgs e)
        {
            status.Content = "Status: Clear the screen & reset the background";
        }

        private void MenuItem_MouseEnter_13(object sender, MouseEventArgs e)
        {
            status.Content = "Status: Open an image file as background";
        }

        private void MenuItem_MouseEnter_14(object sender, MouseEventArgs e)
        {
            status.Content = "Status: Save an image file from background";
        }

        private void MenuItem_MouseEnter_15(object sender, MouseEventArgs e)
        {
            status.Content = "Status: Save an image file from background";
        }
        #endregion

                #region mouseLeave Menu
        private void MenuItem_MouseLeave_1(object sender, MouseEventArgs e)
        {
            status.Content = "Status: --Hover over a control!--";
        }

        private void MenuItem_MouseLeave_2(object sender, MouseEventArgs e)
        {
            status.Content = "Status: --Hover over a control!--";
        }

        private void MenuItem_MouseLeave_3(object sender, MouseEventArgs e)
        {
            status.Content = "Status: --Hover over a control!--";
        }

        private void MenuItem_MouseLeave_4(object sender, MouseEventArgs e)
        {
            status.Content = "Status: --Hover over a control!--";
        }

        private void MenuItem_MouseLeave_5(object sender, MouseEventArgs e)
        {
            status.Content = "Status: --Hover over a control!--";
        }

        private void MenuItem_MouseLeave_6(object sender, MouseEventArgs e)
        {
            status.Content = "Status: --Hover over a control!--";
        }

        private void MenuItem_MouseLeave_7(object sender, MouseEventArgs e)
        {
            status.Content = "Status: --Hover over a control!--";
        }

        private void MenuItem_MouseLeave_8(object sender, MouseEventArgs e)
        {
            status.Content = "Status: --Hover over a control!--";
        }

        private void MenuItem_MouseLeave_9(object sender, MouseEventArgs e)
        {
            status.Content = "Status: --Hover over a control!--";
        }

        private void MenuItem_MouseLeave_10(object sender, MouseEventArgs e)
        {
            status.Content = "Status: --Hover over a control!--";
        }

        private void MenuItem_MouseLeave_11(object sender, MouseEventArgs e)
        {
            status.Content = "Status: --Hover over a control!--";
        }

        private void MenuItem_MouseLeave_12(object sender, MouseEventArgs e)
        {
            status.Content = "Status: --Hover over a control!--";
        }

        private void MenuItem_MouseLeave_13(object sender, MouseEventArgs e)
        {
            status.Content = "Status: --Hover over a control!--";
        }

        private void MenuItem_MouseLeave_14(object sender, MouseEventArgs e)
        {
            status.Content = "Status: --Hover over a control!--";
        }

        private void MenuItem_MouseLeave_15(object sender, MouseEventArgs e)
        {
            status.Content = "Status: --Hover over a control!--";
        }
        #endregion
        #endregion
        #endregion

        #region Finalization
        private void canvasWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            fillColorOptions.Close();
            lineColorOptions.Close();
            bGColorOptions.Close();
        }
        #endregion
    }  
}
