using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace EagerVsLazySequenceCreation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Eagerly created sequence");
            var eagerSequence = BlockingMethod();
            var subscription = eagerSequence
                .Take(1)
                .Subscribe(Console.WriteLine);
            subscription.Dispose();

            Console.WriteLine("Press key");
            Console.ReadKey();

            Console.WriteLine("Lazierly created sequence");
            var lazySequence = NonBlocking();
            subscription = lazySequence
                .Take(1)
                .Subscribe(Console.WriteLine);
            subscription.Dispose();

            Console.WriteLine("Press key to exit");
            Console.ReadKey();
        }

        private static IObservable<string> BlockingMethod()
        {
            var subject = new ReplaySubject<string>();
            subject.OnNext("a");
            subject.OnNext("b");
            subject.OnCompleted();
            Thread.Sleep(5000); // blocks subscription until 5 sec pass
            return subject;
        }
        private static IObservable<string> NonBlocking()
        {
            return Observable.Create<string>(
            (IObserver<string> observer) =>
            {
                observer.OnNext("a");
                observer.OnNext("b");
                observer.OnCompleted();
                Thread.Sleep(5000); // Doesn't block subscription
                return Disposable.Create(() => Console.WriteLine("Observer has unsubscribed"));
                //or can return an Action like 
                //return () => Console.WriteLine("Observer has unsubscribed"); 
            });
        }
    }
}
