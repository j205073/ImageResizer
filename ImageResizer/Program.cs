using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizer
{
    public enum ProcessStyle
    {
        OldStyle, NewStyle
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            ThreadPool.QueueUserWorkItem(x =>
            {
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.C)
                {
                    cts.Cancel();
                }
            });


            Dictionary<ProcessStyle, Dictionary<int, long>> process = new Dictionary<ProcessStyle, Dictionary<int, long>>();

            foreach (ProcessStyle p in Enum.GetValues(typeof(ProcessStyle)))
            {
                //if (p == ProcessStyle.OldStyle)
                //    continue;

                Dictionary<int, long> times = new Dictionary<int, long>();
                for (int i = 0; i < 4; i++)
                {
                    string sourcePath = Path.Combine(Environment.CurrentDirectory, "images");
                    string destinationPath = Path.Combine(Environment.CurrentDirectory, "output"); ;

                    ImageProcess imageProcess = new ImageProcess();
                    imageProcess.Clean(destinationPath);

                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    switch (p)
                    {
                        case ProcessStyle.OldStyle:
                            imageProcess.ResizeImages(sourcePath, destinationPath, 2.0);
                            break;

                        case ProcessStyle.NewStyle:

                            //imageProcess.ResizeImagesAsync(sourcePath, destinationPath, 2.0);
                            Task.Run(async () =>
                            {
                                await imageProcess.ResizeImagesAsync(sourcePath, destinationPath, 2.0);
                            })
                            .Wait();
                            break;

                        default:
                            break;
                    }

                    sw.Stop();
                    times.Add(i, sw.ElapsedMilliseconds);
                    Console.WriteLine($"{p.ToString()} 【{i + 1}】花費時間: {sw.ElapsedMilliseconds} ms");
                }
                if (process.ContainsKey(p))
                    process.Add(p, times);
                else
                    process[p] = times;
            }

            var oldSum = (decimal)process[ProcessStyle.OldStyle].Values.Sum();
            var newSum = (decimal)process[ProcessStyle.NewStyle].Values.Sum();

            var percentage = (oldSum - newSum) / oldSum;
            Console.WriteLine($"總提升{String.Format("{0:P2}.", percentage)}");
            Console.ReadKey();

        }
    }
}