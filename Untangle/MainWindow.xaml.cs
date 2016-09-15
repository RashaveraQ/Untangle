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

namespace Untangle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            double cx = 400;
            double cy = 400;
            for (float th = 0; th < 2 * Math.PI; th += 0.5f) {
                Ellipse node = new Ellipse();
                node.Width = node.Height = 20;
                node.Fill = Brushes.Blue;
                Canvas.SetLeft(node, cx + cx * Math.Cos(th));
                Canvas.SetTop(node, cy + cy * Math.Sin(th));
                node.MouseDown += node_MouseDown;
                canvas.Children.Add(node);        
            }
        }

        Shape shape = null;

        void node_MouseDown(object sender, MouseButtonEventArgs e)
        {
            shape = (Shape)sender;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (shape == null)
                return;
            Point p = e.GetPosition(canvas);
            Canvas .SetLeft(shape, p.X);
            Canvas.SetTop(shape, p.Y);
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            shape = null;
        }

    }
}
