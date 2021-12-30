using System;
using System.Reactive.Linq;

namespace HotObservable
{
    class Program
    {
        static void Main(string[] args)
        {
            var period = TimeSpan.FromSeconds(1);
            var observable = Observable.Interval(period)
                .Do(l => Console.WriteLine("Publishing {0}", l)) //Side effect to show it is running
                .Publish();

            observable.Connect();

            Console.WriteLine("Press any key to subscribe");
            Console.ReadKey();

            var subscription = observable.Subscribe(i => Console.WriteLine("subscription : {0}", i));

            Console.WriteLine("Press any key to unsubscribe.");
            Console.ReadKey();

            subscription.Dispose();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
