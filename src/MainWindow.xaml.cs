using RaytracingPlayground.Models;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RaytracingPlayground
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap _canvas;
        private Image _image;

        private readonly List<Sphere> _spheres = [
            new Sphere(new Point3D(0, -1, 3), 1, Color.FromRgb(255, 0, 0)),
            new Sphere(new Point3D(2, 0, 4), 1, Color.FromRgb(0, 0, 255)),
            new Sphere(new Point3D(-2, 0, 4), 1, Color.FromRgb(0, 255, 0))];
        private readonly double _viewportSize = 1;
        private readonly double _projectionPlaneD = 1;
        private readonly Point3D O = new(0, 0, 0);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCanvas();
        }

        private void InitializeCanvas()
        {
            _image = new Image();
            RenderOptions.SetBitmapScalingMode(_image, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(_image, EdgeMode.Aliased);

            Content = _image;

            _canvas = new WriteableBitmap(
                (int)ActualWidth,
                (int)ActualHeight,
                96,
                96,
                PixelFormats.Bgr32,
                null);

            _image.Source = _canvas;

            _image.Stretch = Stretch.None;
            _image.HorizontalAlignment = HorizontalAlignment.Left;
            _image.VerticalAlignment = VerticalAlignment.Top;

            RenderScene();
        }

        private void RenderScene()
        {
            for (int x = (int)_canvas.Width / 2 * (-1); x < (int)_canvas.Width / 2; x++)
            {
                for (int y = (int)_canvas.Height / 2 * (-1); y < (int)_canvas.Height; y++)
                {
                    Vector3D d = CanvasToViewport(x, y);
                    Color color = TraceRay(O, d, 1, double.PositiveInfinity);
                    PutPixel(x, y, color);
                }
            }
        }

        private Vector3D CanvasToViewport(int x, int y)
        {
            double Vx = x * (_viewportSize / _canvas.Width);
            double Vy = y * (_viewportSize / _canvas.Height);
            double Vz = _projectionPlaneD;
            return new Vector3D(Vx, Vy, Vz);
        }

        private Color TraceRay(Point3D o, Vector3D d, double t_min, double t_max)
        {
            double closest_t = double.PositiveInfinity;
            Sphere? closestSphere = null;
            Color color = new();

            foreach (var sphere in _spheres)
            {
                (double t1, double t2) = IntersectRaySphere(o, d, sphere);
                if (t1 > t_min && t1 < t_max && t1 < closest_t)
                {
                    closest_t = t1;
                    closestSphere = sphere;
                }
                if (t2 > t_min && t2 < t_max && t2 < closest_t)
                {
                    closest_t = t2;
                    closestSphere = sphere;
                }
                if (closestSphere == null)
                {
                    color = Color.FromRgb(255, 255, 255);
                }
                else
                {
                    color = closestSphere.Color;
                }
            }

            return color;
        }
        // -300 -153
        private static (double, double) IntersectRaySphere(Point3D o, Vector3D d, Sphere sphere)
        {
            double r = sphere.Radius;
            Vector3D co = o - sphere.Center;
            double a = Vector3D.DotProduct(d, d);
            double b = 2 * Vector3D.DotProduct(co, d);
            double c = Vector3D.DotProduct(co, co) - r * r;

            double discriminant = (b * b) - (4 * a * c);
            if (discriminant < 0)
            {
                return (double.PositiveInfinity, double.PositiveInfinity);
            }

            double t1 = ((-b) + Math.Sqrt(discriminant)) / (2 * a);
            double t2 = ((-b) - Math.Sqrt(discriminant)) / (2 * a);

            return (t1, t2);
        }

        private void PutPixel(int x, int y, Color color)
        {
            int row = (int)_canvas.Height / 2 - y;
            int column = (int)_canvas.Width / 2 + x;

            if (column >= _canvas.Width || row >= _canvas.Height || column < 0 || row < 0)
            {
                return;
            }

            try
            {
                _canvas.Lock();

                unsafe
                {
                    IntPtr pBackBuffer = _canvas.BackBuffer;

                    pBackBuffer += row * _canvas.BackBufferStride;
                    pBackBuffer += column * 4;

                    int colorData = color.R << 16;
                    colorData |= color.G << 8;
                    colorData |= color.B << 0;

                    *((int*)pBackBuffer) = colorData;
                }

                _canvas.AddDirtyRect(new Int32Rect(column, row, 1, 1));
            }
            finally
            {
                _canvas.Unlock();
            }
        }
    }
}