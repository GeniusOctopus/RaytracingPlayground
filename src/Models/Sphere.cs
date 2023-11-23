
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace RaytracingPlayground.Models
{
    public class Sphere(Point3D center, double radius, Color color)
    {
        public Point3D Center { get; set; } = center;
        public double Radius { get; set; } = radius;
        public Color Color { get; set; } = color;
    }
}
