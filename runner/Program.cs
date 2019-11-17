using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace labyrinth.runner
{
    public class Program
    {
        public static bool Filter(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;
            if (line.StartsWith("0x0"))
                return false;
            if (line.StartsWith("(Filename"))
                return false;

            return true;
        } 

        static void Main(string[] args)
        {
            using (var proc = Process.Start("simulator", "-logFile simulator.log -stackTraceLogType None"))
            {
                var cancel = new CancellationTokenSource();

                Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    var logFile = "simulator.log";
                    Console.WriteLine($"Streaming Unity logfile {logFile}");

                    using var logStream = new StreamReader(new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

                    while (true)
                    {
                        var lines = logStream.ReadToEnd();
                        foreach (var line in lines.Split(Environment.NewLine).Where(Filter)) 
                        {
                            Console.WriteLine(line);
                        }
                        await Task.Yield();
                    }

                }, cancel.Token);

                proc.WaitForExit();
                cancel.Cancel();
            }
        }
    }
}
