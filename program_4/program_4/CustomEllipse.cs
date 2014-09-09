using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace program_4
{
    class CustomEllipse : Shape, CustomShape
    {
        public int shapeID;
        public int ShapeID
        {
            get
            {
                return this.shapeID;
            }
            set
            {
                this.shapeID = value;
            }
        }
        public Point startPoint;
        public Point StartPoint
        {
            get
            {
                return this.startPoint;
            }
            set
            {
                this.startPoint = value;
            }
        }

        public Point endPoint;
        public Point EndPoint
        {
            get
            {
                return this.endPoint;
            }
            set
            {
                this.endPoint = value;
            }
        }
        public ShapeInformation.ShapeType shapeType = ShapeInformation.ShapeType.Ellipse;
        public ShapeInformation.ShapeType ShapeType
        {
            get { return this.shapeType; }
        }
        public ShapeInformation.BorderType borderType;
        public ShapeInformation.BorderType BorderType
        {
            get
            {
                return this.borderType;
            }
            set
            {
                this.borderType = value;
            }
        }

        public EllipseGeometry geometry;

        public CustomEllipse(int shapeID, Point startPoint, Point endPoint, ShapeInformation.BorderType borderType) : base()
        {
            geometry = new EllipseGeometry();
            this.Stretch = Stretch.Fill;
            this.shapeID = shapeID;
            this.startPoint = startPoint;
            this.endPoint = endPoint;
            this.borderType = borderType;
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                geometry.RadiusX = this.Width / 2;
                geometry.RadiusY = this.Width / 2;

                return geometry;
            }
        }

        public Brush GetBaseFillColor()
        {
            return base.Fill;
        }

        public Brush GetBaseBorderColor()
        {
            return base.Stroke;
        }

        public int GetStrokeThickness
        {
            get
            {
                return (int)base.StrokeThickness;
            }
            set
            {
                base.StrokeThickness = value;
            }
        }
    }
}