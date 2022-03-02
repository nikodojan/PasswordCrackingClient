using System;
using System.Collections.Generic;
using System.Linq;

namespace PasswordClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            PwTcpClient pwClient = new PwTcpClient("192.168.14.193", 10000);
            pwClient.RequestData();
            Console.WriteLine(pwClient.Password);

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(pwClient.Dict[i]);
            }


            Console.ReadKey();
        }
    }
}
