using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var cancelationTokeSource1 = new CancellationTokenSource();
            var source1 = Unfold<Shape>(new Shape(1, 1), shape =>
            {
                Thread.Sleep(1);
                return new Shape(shape.Id, shape.Value + 1);
            }, cancelationTokeSource1)
                .ToObservable()
                .SubscribeOn(ThreadPoolScheduler.Instance);

            var cancelationTokeSource2 = new CancellationTokenSource();
            var source2 = Unfold<Shape>(new Shape(2, 1), shape =>
            {
                Thread.Sleep(1);
                return new Shape(shape.Id, shape.Value + 1);
            }, cancelationTokeSource2)
                .ToObservable()
                .SubscribeOn(ThreadPoolScheduler.Instance);

            var source = source1
                .Merge(source2);

            var disposableSource = source.Subscribe(o =>
            {
                Console.WriteLine($"{o.Id} {o.Value}");
            });

            System.Threading.Thread.Sleep(3000);

            disposableSource.Dispose();

            var cancelationTokeSource3 = new CancellationTokenSource();
            var source3 = Unfold<Shape>(new Shape(3, 1), shape =>
            {
                Thread.Sleep(1);
                return new Shape(shape.Id, shape.Value + 1);
            }, cancelationTokeSource3)
                .ToObservable()
                .SubscribeOn(ThreadPoolScheduler.Instance);

            source = source
                .Merge(source3);

            source.Subscribe(o =>
            {
                Console.WriteLine($"{o.Id} {o.Value}");
            });

            Console.WriteLine("Done!");
            Console.Read();
        }

        private static IEnumerable<T> Unfold<T>(T seed, Func<T, T> accumulator, CancellationTokenSource cancellationToken)
        {
            var nextValue = seed;

            while (!cancellationToken.IsCancellationRequested)
            {
                yield return nextValue;
                nextValue = accumulator(nextValue);
            }
        }
    }

    struct Shape
    {
        public int Id { get; }

        public int Value { get; }

        public Shape(int id, int value)
        {
            Id = id;
            Value = value;
        }
    }
}
