using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using PasswordClient.Models;

namespace PasswordClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //192.168.14.193
            PwTcpClient pwClient = new PwTcpClient("192.168.14.193", 10000);
            pwClient.RequestData();
            
            Console.WriteLine("Creating user info");
            Console.WriteLine(pwClient.Password);
            var userInfoArray = pwClient.Password.Split(':');
            UserInfo user = new UserInfo(userInfoArray[0], userInfoArray[1]);

            Console.WriteLine("Start cracking");
            Cracking cracker = new Cracking();
            cracker.RunCracking(user);

            Console.WriteLine("End");
            Console.ReadKey();
        }
    }
}
