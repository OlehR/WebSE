using System;
using System.Threading;
using System.Threading.Tasks;

namespace In_out
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var e = new Parse();
            Console.WriteLine("Start");
            string v = await e.RequestAsync();
            Console.WriteLine("End");

            Thread.Sleep(10000);
        }
    }
}
