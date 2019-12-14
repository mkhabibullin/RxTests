using System.Windows;

namespace RxTest.Models
{
    struct ShapePoint
    {
        public Point Point { get; }

        public int ShapeId { get; }

        public ShapePoint(Point point, int shapeId)
        {
            Point = point;
            ShapeId = shapeId;
        }
    }
}
