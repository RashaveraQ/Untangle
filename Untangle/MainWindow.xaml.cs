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
using System.Windows.Threading;

namespace Untangle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Shape[] nodes = null;
        List<Line> lines = new List<Line>();
        DispatcherTimer dispatcherTimer;
        Shape shape = null;
        Point pointMiddleButtonPressed;

        public MainWindow()
        {
            InitializeComponent();
            
            double r = Height / 2 - 200;

            int num = 6;
            nodes = new Shape[num];
            for (int i = 0; i < num; i++) {
                double th = 2.0f * Math.PI * (float)i / (float)num;
                nodes[i] = new Ellipse {
                    Width = 20,
                    Height = 20,
                    Fill = Brushes.Blue,
                    Margin = new Thickness(100 + r + r * Math.Cos(th), 100 + r + r * Math.Sin(th), 0, 0),
                    RenderTransform = new TranslateTransform(-10,-10),
                    Tag = new List<Shape>()
                };
                nodes[i].MouseDown += node_MouseDown;
            }

            Random random = new Random(Environment.TickCount);
            for (int i = 0; i < num; i++) {
                int j,k;
                do {
                    j = random.Next(num - 1); 
                    k = random.Next(num - 1);
                } while (i == j || i == k || j == k);
                link(i,j);
                link(i,k);
            }

            for (int i = 0; i < num; i++) {
                canvas.Children.Add(nodes[i]);        
            }

            updateLineColor();

            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal);
            dispatcherTimer.Interval = new TimeSpan(0,0,0,0,1);
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
        }

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            foreach (Shape node in nodes) {
                Point p1 = new Point(node.Margin.Left, node.Margin.Top);
                Vector v1 = new Vector(0,0);
                foreach (Shape other in nodes) {
                    if (other == node)
                        continue;
                    Point p2 = new Point(other.Margin.Left, other.Margin.Top);
                    Vector v = Point.Subtract(p2, p1);
                    if (((List<Shape>)node.Tag).Contains(other)) {
                        if (v.Length > 25) {
                            v *= 3.0E-4 * v.Length;
                            v1 += v;
                        }
                    }
                }
                foreach (Line line in lines) {
                    Point a = new Point(line.X1, line.Y1);
                    Point b = new Point(line.X2, line.Y2);
                    Vector AB = Point.Subtract(b,a);
                    Vector AC = Point.Subtract(p1,a);
                    double t = (AC.X * AB.X + AC.Y * AB.Y) / AB.LengthSquared;
                    if (0 <= t && t <= 1) {
                        Point p = a + AB * t;
                        Vector PC = Point.Subtract(p1, p);
                        if (PC.Length == 0)
                            continue;
                        double len = PC.Length;
                        PC *= 10 / len;
                        v1 += PC;
                    }
                }
                p1 += v1;
                node.Margin = new Thickness(p1.X, p1.Y, 0, 0);
            }
            updateLineColor();
        }

        void link(int i, int j) 
        {
            if (((List<Shape>)nodes[i].Tag).Contains(nodes[j]))
                return;
            
            ((List<Shape>)nodes[i].Tag).Add(nodes[j]);
            ((List<Shape>)nodes[j].Tag).Add(nodes[i]);
            Line line = new Line {Stroke = Brushes.Black};
            line.SetBinding(Line.X1Property, new Binding("Margin.Left") { Source = nodes[i] });
            line.SetBinding(Line.Y1Property, new Binding("Margin.Top") { Source = nodes[i] });
            line.SetBinding(Line.X2Property, new Binding("Margin.Left") { Source = nodes[j] });
            line.SetBinding(Line.Y2Property, new Binding("Margin.Top") { Source = nodes[j] });
            canvas.Children.Add(line);
            lines.Add(line);
        }


        void node_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(canvas);
            if (e.LeftButton == MouseButtonState.Pressed) {
                shape = (Shape)sender;
                shape.Fill = Brushes.Red;
                foreach(Shape s in (List<Shape>)shape.Tag)
                    s.Fill = Brushes.Yellow;
            } 
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(canvas);
            if (shape != null) {
                shape.Margin = new Thickness(p.X, p.Y, 0, 0);
                updateLineColor();
            } else if (e.MiddleButton == MouseButtonState.Pressed) {
                Vector v = Point.Subtract(p, pointMiddleButtonPressed);
                foreach (Shape s in nodes) {
                    Thickness t = s.Margin;
                    s.Margin = new Thickness(t.Left + v.X, t.Top + v.Y, 0, 0);
                }
                pointMiddleButtonPressed = p;
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (shape == null)
                return;

            shape.Fill = Brushes.Blue;
            foreach(Shape s in (List<Shape>)shape.Tag)
                s.Fill = Brushes.Blue;
            shape = null;

        }

        private void updateLineColor()
        {
            foreach (Line l1 in lines) {
                l1.StrokeThickness = -1;
                l1.Stroke = Brushes.Black;
                foreach(Line l2 in lines) {
                    if (l1 == l2)
                        continue;
                    double ax = l1.X1;
                    double ay = l1.Y1;
                    double bx = l1.X2;
                    double by = l1.Y2;
                    double cx = l2.X1;
                    double cy = l2.Y1;
                    double dx = l2.X2;
                    double dy = l2.Y2;
                    double ta = (cx - dx) * (ay - cy) + (cy - dy) * (cx - ax);
                    double tb = (cx - dx) * (by - cy) + (cy - dy) * (cx - bx);
                    double tc = (ax - bx) * (cy - ay) + (ay - by) * (ax - cx);
                    double td = (ax - bx) * (dy - ay) + (ay - by) * (ax - dx);
                    if (tc * td < 0 && ta * tb < 0) {
                        l1.StrokeThickness = 2;
                        l1.Stroke = Brushes.Tomato;
                    }
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(canvas);
            if (e.MiddleButton == MouseButtonState.Pressed) {
                pointMiddleButtonPressed = p;                
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point p = e.GetPosition(canvas);
            double f = Math.Pow(1.001, e.Delta);
            foreach (Shape s in nodes) {
                Thickness t = s.Margin;
                s.Margin = new Thickness(p.X + f * (t.Left - p.X), p.Y + f * (t.Top - p.Y), 0, 0);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key) {
                case Key.S:
                    if (dispatcherTimer.IsEnabled)
                        dispatcherTimer.Stop();
                    else
                        dispatcherTimer.Start();
                    break;
            }
        }
    }
}
