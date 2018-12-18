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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Storage;

namespace WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random rnd = new Random(Environment.TickCount);
        public MainWindow()
        {
            this.Left = (double)ApplicationData.Current.LocalSettings.Values["X"];
            this.Top = (double)ApplicationData.Current.LocalSettings.Values["Y"];
            this.Width = (double)ApplicationData.Current.LocalSettings.Values["Width"];
            this.Height = (double)ApplicationData.Current.LocalSettings.Values["Height"];

            InitializeComponent();
            this.MouseMove += MainWindow_MouseMove;

            RadialGradientBrush brush = new RadialGradientBrush();
            brush.GradientStops.Add(new GradientStop(GetRandomColor(), 0.0d));
            brush.GradientStops.Add(new GradientStop(GetRandomColor(), 0.5d));
            brush.GradientStops.Add(new GradientStop(GetRandomColor(), 1.0d));
            root.Background = brush;

            double[] rawPoints = (double[])ApplicationData.Current.LocalSettings.Values["Points"];
            PathGeometry geo = new PathGeometry();
            geo.FillRule = FillRule.Nonzero;
            PathFigure figure = new PathFigure();
            figure.StartPoint = new Point(rawPoints[0], rawPoints[1]);
            PolyLineSegment segment = new PolyLineSegment();
            for (int i = 0; i < rawPoints.Length / 2; i+=2)
            {
                segment.Points.Add(new Point(rawPoints[2 * i], rawPoints[2 * i +1]));
            }
            figure.Segments.Add(segment);
            geo.Figures.Add(figure);
            this.Clip = geo;

            closeButton.SetValue(Canvas.LeftProperty, geo.Bounds.Width / 2 - geo.Bounds.Left - 16);
            closeButton.SetValue(Canvas.TopProperty, geo.Bounds.Height / 2 - geo.Bounds.Top - 16);
        }

        private Color GetRandomColor()
        {
            byte[] bytes = new byte[4];
            rnd.NextBytes(bytes);
            return Color.FromArgb((byte)(192 + bytes[0]/4), bytes[1], bytes[2], bytes[3]);
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                {
                    this.DragMove();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
