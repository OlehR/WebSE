using System;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace In_out
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var Coffe = new CoffeeMachine();
            DateTime Dt;
                Dt = DateTime.Now.Date.AddDays(-20);
            while(Dt< DateTime.Now.Date)
            {
                Console.WriteLine($"Start{Dt}");
                await Coffe.SendAsync(Dt);
                Dt =Dt.AddDays(1);
            }
            return;

             Dt = DateTime.Now.Date.AddDays(-1);
            if(args.Length > 0)
            {
                var res=args[0].ToDateTime("yyyy-MM-dd");
                if (DateTime.MinValue != res)
                    Dt = res;
            }
            
            Console.WriteLine("Start");
            await Coffe.SendAsync(Dt);
           // string v = await e.RequestAsync();
            Console.WriteLine("End");

            Thread.Sleep(10000);
        }
    }
}
