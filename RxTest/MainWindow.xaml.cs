using RxTest.Models;
using RxTest.UI.Elements;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RxTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<EllipseElement> Ellipses;

        private List<IObservable<ShapePoint>> LocationsSource;

        public MainWindow()
        {
            InitializeComponent();

            Ellipses = new List<EllipseElement>();

            LocationsSource = new List<IObservable<ShapePoint>>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var random = new Random();
            for (var i = 0; i < 20; i++)
            {
                var brush = new SolidColorBrush(Color.FromRgb((byte)random.Next(1, 255),
                        (byte)random.Next(1, 255), (byte)random.Next(1, 233)));

                var ellipse = new EllipseElement(
                    i,
                    new Point(random.Next(0, (int)RenderSize.Width), random.Next(0, (int)RenderSize.Height)),
                    new Size(90, 90),
                    this.RenderSize,
                    brush);

                Ellipses.Add(ellipse);

                this.MainManvas.Children.Add(ellipse.Shape);

                var locationsSource = ellipse
                    .Start();

                locationsSource
                    .ObserveOnDispatcher()
                    .Subscribe(p =>
                    {
                        Canvas.SetLeft(ellipse.Shape, p.Point.X);
                        Canvas.SetTop(ellipse.Shape, p.Point.Y);
                    });

                LocationsSource.Add(locationsSource);
            }

            IObservable<ShapePoint> mergedLocationSources = Observable.Empty<ShapePoint>();
            foreach(var ls in LocationsSource)
            {
                mergedLocationSources = mergedLocationSources.Merge(ls);
            }

            foreach(var el in Ellipses)
            {
                el.UpdateNegborsLocationSource(mergedLocationSources);
            }

            Console.WriteLine(Ellipses.Count);
        }
    }
}
