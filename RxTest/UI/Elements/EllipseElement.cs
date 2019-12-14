using RxTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RxTest.UI.Elements
{
    class EllipseElement
    {
        public Ellipse Shape { get; }

        public int Id { get; }

        private CancellationTokenSource cancellationToken;

        private ShapePoint location;
        private readonly Size size;
        private readonly Size boundary;

        private IDisposable NeighborsLocations = null;

        private int dx = 1, dy = 1;

        public EllipseElement(int id, Point location, Size size, Size boundary, Brush brush)
        {
            Id = id;
            this.size = size;
            this.location = new ShapePoint(location, id);
            this.boundary = boundary;

            Shape = new Ellipse()
            {
                Width = size.Width,
                Height = size.Height,
                Fill = brush,
            };
        }

        public IObservable<ShapePoint> Start()
        {
            cancellationToken = new CancellationTokenSource();

            return Unfold(location, _ =>
            {
                var point = location.Point;

                if ((point.X + size.Width) >= boundary.Width) dx = -Math.Abs(dx);
                if (point.X < 0) dx = Math.Abs(dx);
                if ((point.Y + size.Height) >= boundary.Height) dy = -Math.Abs(dy);
                if (point.Y < 0) dy = Math.Abs(dy);

                var newPoint = new Point(point.X + dx, point.Y + dy);

                location = new ShapePoint(newPoint, Id);

                return location;
            })
                .ToObservable()
                .SubscribeOn(NewThreadScheduler.Default);
        }

        public void UpdateNegborsLocationSource(IObservable<ShapePoint> source)
        {
            NeighborsLocations?.Dispose();

            NeighborsLocations = source
                .SubscribeOn(NewThreadScheduler.Default)
                .Buffer(TimeSpan.FromMilliseconds(1))
                .Subscribe(ob =>
                {
                    var neighborsLocations = ob
                        .GroupBy(g => g.ShapeId)
                        .Where(g => g.Key != Id)
                        .Select(g => g.Last());

                    foreach (var o in neighborsLocations)
                    {
                        CheckNeighborLocation(o);
                    }
                });
        }

        private void CheckNeighborLocation(ShapePoint o)
        {
            if (o.ShapeId == this.Id) return;

            int? newDx = null, newDy = null;

            var neighborLeftEdge = o.Point.X;
            var neighborRightEdge = o.Point.X + this.size.Width;

            var leftEdge = location.Point.X;
            var rightEdge = location.Point.X + this.size.Width;

            if (neighborRightEdge >= leftEdge && neighborRightEdge < rightEdge)
            {
                newDx = Math.Abs(dx);
            }
            else if (rightEdge >= neighborLeftEdge && rightEdge < neighborRightEdge)
            {
                newDx = -Math.Abs(dx);
            }

            if (!newDx.HasValue) return;

            var neighborTopEdge = o.Point.Y;
            var neighborBottomEdge = o.Point.Y + this.size.Height;

            var topEdge = location.Point.Y;
            var bottomEdge = location.Point.Y + this.size.Height;

            if (neighborBottomEdge >= topEdge && neighborBottomEdge < bottomEdge)
            {
                newDy = Math.Abs(dy);
            }
            else if (bottomEdge >= neighborTopEdge && bottomEdge < neighborBottomEdge)
            {
                newDy = -Math.Abs(dy);
            }

            if (!newDy.HasValue) return;

            dx = newDx.Value;
            dy = newDy.Value;
        }

        private IEnumerable<T> Unfold<T>(T seed, Func<T, T> accumulator)
        {
            var nextValue = seed;

            while (!cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(1));
                nextValue = accumulator(nextValue);

                yield return nextValue;
            }
        }

        public void Stop()
        {
            cancellationToken.Cancel();
        }
    }
}
