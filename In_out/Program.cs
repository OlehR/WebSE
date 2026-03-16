using System;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace InOut
{
    class Program
    {
        static async Task Main(string[] args)
        {          
            
            Console.WriteLine("Start");
            VisitingSC  p =new ();
            await p.RequestAsync(20231219);
            
            Console.WriteLine("End");

            Thread.Sleep(10000);
        }
    }
}
