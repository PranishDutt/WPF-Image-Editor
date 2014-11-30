using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
namespace program_4
{
    interface CustomShape
    {
        //Properties
        int ShapeID { get; set; }
        Point StartPoint { get; set; }
        Point EndPoint { get; set; }
        ShapeInformation.ShapeType ShapeType { get; }
        ShapeInformation.BorderType BorderType { get; set; }

        //Methods
        Brush GetBaseFillColor();
        Brush GetBaseBorderColor();
        int GetStrokeThickness { get; set; }
    }
}
