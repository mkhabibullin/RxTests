using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace Generator
{
    static class Program
    {
        static string FilePath = @"C:\Temp\BigFile.txt";

        static Random randomSeed;

        static object locker = new object();

        static void Main(string[] args)
        {
            WriteLog("Start");

            randomSeed = new Random();

            var threadsCount = 8;

            var wordSize = 4;

            var maxSize = 10.GigaBites();

            var currentSize = 0D;

            var observable = Observable
                .Create<string>(o =>
                {
                    while (true)
                    {
                        var word = RandomString(wordSize, true);
                        
                        o.OnNext(word);

                        currentSize += word.Length * 1; /* UTF-8 */

                        if (currentSize >= maxSize)
                        {
                            break;
                        }
                    }
                    //o.OnCompleted();
                    WriteLog("Generating stop");
                    return Disposable.Empty;
                })
                .SubscribeOn(NewThreadScheduler.Default);

            var stremWriter = new StreamWriter(FilePath);
            var currentWirtedSize = 0;
            for (var i = 0; i < threadsCount; i++)
            {
                observable
                    .ObserveOn(NewThreadScheduler.Default)
                    .Subscribe(word =>
                    {
                        lock (locker)
                        {
                            if (currentWirtedSize >= maxSize)
                            {
                                WriteLog("Generating stop");
                                return;
                            }

                            if (string.IsNullOrEmpty(word)) return;

                            stremWriter.Write(word);

                            currentWirtedSize += word.Length * 1; /* UTF-8 */
                        }
                    });
            }

            WriteLog("Done");
            Console.Read();
        }

        /// <summary>
        /// Generates a random string with the given length
        /// </summary>
        /// <param name="size">Size of the string</param>
        /// <param name="lowerCase">If true, generate lowercase string</param>
        /// <returns>Random string</returns>
        public static string RandomString(int size, bool lowerCase)
        {
            // StringBuilder is faster than using strings (+=)
            StringBuilder RandStr = new StringBuilder(size);

            // Ascii start position (65 = A / 97 = a)
            int Start = (lowerCase) ? 97 : 65;

            randomSeed = new Random();
            // Add random chars
            for (int i = 0; i < size; i++)
                RandStr.Append((char)(26 * randomSeed.NextDouble() + Start));

            return RandStr.ToString();
        }

        private static void WriteLog(string head)
        {
            Console.WriteLine($"{head}: {DateTime.Now.ToLongTimeString()}");
        }

        public static double MegaBites(this int megaBites)
        {
            return (double)1024/*1 KB*/ * 1024 * megaBites;
        }

        private static double GigaBites(this int gigaBites)
        {
            return 1.MegaBites() * 1024 * gigaBites;
        }
    }
}
