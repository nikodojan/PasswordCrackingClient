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

            PwTcpClient pwClient = new PwTcpClient("ipaddress", 1000);
            pwClient.ConnectToServer();
            Console.WriteLine(pwClient.Password);

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(pwClient.Dict[i]);
            }


            Console.ReadKey();
        }
    }
}
