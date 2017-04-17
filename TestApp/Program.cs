using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                int waitTime = int.Parse(args[1]);
                Console.WriteLine("Sleeping for {0} ms", waitTime);
                Thread.Sleep(waitTime);
            }

            if (args[0].ToUpper() == "SUCCESS")
            {
                Console.WriteLine("Returning success code.");
                Environment.ExitCode = 0;
            }

            if (args[0].ToUpper() == "FAILURE")
            {
                Console.WriteLine("Returning failure code.");
                Environment.ExitCode = 1;
            }
        }
    }
}
