/* Name: Pranish Dutt
 * Date: 10/4/20113 
 * Purpose: Assignment 4 */

using System;
using System.Linq;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;
using System.Text;

namespace ShapeInformation
{
    public enum ShapeType
    {
        Rectangle,
        Ellipse,
        Right_Triangle,
        Diamond,
        Isoceles_Triangle,
        Hexagon,
        Pentagon
    }

    public enum FillType
    {
        Drawn,
        Filled
    }

    public enum BorderType
    {
        Solid,
        DashDot,
        Dot
    }



    public class ShapeInfo
    {
        public int shapeID;
        public ShapeType shapeType;
        public FillType fillType;
        public Point startPoint;
        public Point endPoint;
        public Color fillColor;
        public Color borderColor;
        public BorderType borderType;
        public int borderWidth;

        public ShapeInfo(int shapeID, ShapeType shapeType, FillType fillType, Point startPoint, Point endPoint,
                            Color fillColor, Color borderColor, BorderType borderType, int borderWidth)
        {
            this.shapeID = shapeID;         // CustomShape.shapeID
            this.shapeType = shapeType;     // CustomShape.shapeType
            this.fillType = fillType;       // Shape: Find fill type by checking if fillColor is transperant.
            this.startPoint = startPoint;   // CustomShape.startPoint
            this.endPoint = endPoint;       // CustomShape.endPoint
            this.fillColor = fillColor;     // Shape.Fill
            this.borderColor = borderColor; // Shape.Stroke
            this.borderType = borderType;   // Shape: Find which DoubleCollection coiencides with borderType.
            this.borderWidth = borderWidth; // Shape.StrokeThickness
        }

        public override string ToString()
        {
            string data = (int)shapeType + " "
                        + (int)fillType + " "
                        + startPoint.X + " " 
                        + startPoint.Y + " "
                        + endPoint.X + " "
                        + endPoint.Y + " "
                        + fillColor.R + " "
                        + fillColor.G + " "
                        + fillColor.B + " "
                        + borderColor.R + " "
                        + borderColor.G + " "
                        + borderColor.B + " "
                        + (int)borderType + " "
                        + borderWidth;
            return data;
        }
    }
}
