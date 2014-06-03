using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Flowchart_editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int i = 0;
        private Point _dropPoint;
        private double _totalHeight;
        private Arrow _dropArrow;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Czysc(object sender, RoutedEventArgs e)
        {
            //Wiadomość TAK/NIE z resetowaniem projektu
            MessageBoxResult result = MessageBox.Show("Czy chcesz uruchomić nowy projekt?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
           
        }
        



        private void DropCanvas_OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Polygon)))
            {
                
                e.Effects = DragDropEffects.Copy;
                
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            //zerowanie wysokości elemetów w canvas
           
            _totalHeight = 0;
            
        }

        private void DrawNewArrow(Arrow oldArrow)
        {
            var newArrow = new Arrow
            {
                X1 = oldArrow.X1,
                X2 = oldArrow.X2,
                Y1 = oldArrow.Y2 + 40,
                Y2 = oldArrow.Y2 + 70,
                AllowDrop = oldArrow.AllowDrop,
                HeadHeight = oldArrow.HeadHeight,
                HeadWidth = oldArrow.HeadWidth,
                Stroke = oldArrow.Stroke,
                StrokeThickness = oldArrow.StrokeThickness
            };
            DropCanvas.Children.Add(newArrow);
        }
    
       
        private void CreateNewPolygon(Polygon oldPolygon)
        {
            i++;
            
            string nazwa = oldPolygon.Name;
            var newPolygon = new Polygon
            {   
                
                Name = nazwa + i,
                Stroke = oldPolygon.Stroke,
                StrokeThickness = oldPolygon.StrokeThickness,
                Fill = oldPolygon.Fill,
                Points = oldPolygon.Points
               
                
            };
            newPolygon.MouseLeftButtonDown += new MouseButtonEventHandler(p_MouseLeftButtonDown);
            //utworzenie 'faktycznych' wymiarów, inaczej 0
            newPolygon.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var polyWidth = newPolygon.DesiredSize.Width;

            _dropPoint.X = _dropPoint.X - (polyWidth) / 2;
            _dropPoint.Y = _dropArrow.Y2;
            
            //MessageBox.Show(newPolygon.Name);
            Console.Text += oldPolygon.Name + "\n";
            Console.ScrollToEnd();
            Canvas.SetLeft(newPolygon, _dropPoint.X);
            Canvas.SetTop(newPolygon, _dropPoint.Y);
            DropCanvas.Children.Add(newPolygon);
        }

        void p_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var p = (Polygon) sender;
            var s = p.Name;
            Form1 form = new Form1();
            form.Show();
            
            
           // MessageBox.Show(s);
        }

        private double GetArrowPosition(Arrow child)
        {
            return child.Y1;
        }

        private void MoveExistingBlocksIfNecessary(Canvas canvas)
        {
            foreach (UIElement child in canvas.Children)
            {
                var pos = child is Arrow ? GetArrowPosition(child as Arrow) : Canvas.GetTop(child);

                if (pos >= _dropPoint.Y)
                {
                    if (child is Arrow)
                    {
                        var arrow = child as Arrow;
                        arrow.Y1 = arrow.Y1 + 70;
                        arrow.Y2 = arrow.Y2 + 70;
                    }
                    else
                        child.SetValue(Canvas.TopProperty, pos + 70);
                }
            }
        }

        private void ResizeCanvas(Canvas canvas)
        {
            foreach (UIElement child in canvas.Children)
                _totalHeight += child is Arrow ? 30 : child.DesiredSize.Height;

            if (canvas.ActualHeight <= _totalHeight)
                canvas.Height = canvas.ActualHeight + 80;
        }

     

        private void DropCanvas_OnDrop(object sender, DragEventArgs e)
        {


        



            //pobranie wybranego bloku z listy
            var polygon = (Polygon)e.Data.GetData(typeof(Polygon));
            
            //pobranie strzałki, na którą rzucane są bloki
            _dropArrow = (Arrow) e.OriginalSource;

            //"zabezpieczenie" przed pustym obiektem
            if (polygon == null) return;

            //pobranie miejsca, w którym został upuszczony blok
            _dropPoint = e.GetPosition(DropCanvas);

            //przesunięcie istniejących elementów o wysokości >= _dropPoint
            MoveExistingBlocksIfNecessary(DropCanvas);

            //tworzenie nowej strzałki
            DrawNewArrow(_dropArrow);

            //tworzenie nowego bloku
            CreateNewPolygon(polygon);

            //jeżeli strzałka -> wysokość + 30 lub wysokość bloku
            ResizeCanvas(DropCanvas);
            
        }

        private void FwSymbols_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var item = FwSymbols.SelectedItem as Polygon;
                if (item == null) return;
                var data = new DataObject(typeof (Polygon), item);
                DragDrop.DoDragDrop(FwSymbols, data, DragDropEffects.Copy);
            }
            
        }

      
    }
}
