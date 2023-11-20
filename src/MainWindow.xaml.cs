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

namespace RaytracingPlayground
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap _writeableBitmap;
        private Image _canvas;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeCanvas()
        {
            _canvas = new Image();
            RenderOptions.SetBitmapScalingMode(_canvas, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(_canvas, EdgeMode.Aliased);

            Content = _canvas;

            _writeableBitmap = new WriteableBitmap(
                (int)ActualWidth,
                (int)ActualHeight,
                96,
                96,
                PixelFormats.Bgr32,
                null);

            _canvas.Source = _writeableBitmap;

            _canvas.Stretch = Stretch.None;
            _canvas.HorizontalAlignment = HorizontalAlignment.Left;
            _canvas.VerticalAlignment = VerticalAlignment.Top;

            RenderScene();
        }

        private void RenderScene()
        {
            int column = 0;
            int row = 0;

            try
            {
                _writeableBitmap.Lock();

                unsafe
                {
                    for (int x = 0; x < 30; x++)
                    {
                        for (int y = 0; y < 30; y++)
                        {
                            IntPtr pBackBuffer = _writeableBitmap.BackBuffer;

                            pBackBuffer += (row + x) * _writeableBitmap.BackBufferStride;
                            pBackBuffer += (column + y) * 4;

                            int colorData = 255 << 16;
                            colorData |= 255 << 8;
                            colorData |= 255 << 0;

                            *((int*)pBackBuffer) = colorData;
                        }
                    }
                }

                _writeableBitmap.AddDirtyRect(new Int32Rect(column, row, 30, 30));
            }
            finally
            {
                _writeableBitmap.Unlock();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCanvas();
        }
    }
}