using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using PasswordClient.Models;

namespace PasswordClient
{
    internal class Program
    {
        private const string ServerIp = "localhost";
        private const int ServerPort = 10_000;
        //192.168.14.115

        static void Main(string[] args)
        {
            Console.WriteLine("Client started");
            List<string> dictionary = new List<string>();
            while (true)
            {
                //Console.WriteLine($"Fetching new data from server {ServerIp}:{ServerPort}");

                PwTcpClient pwClient = new PwTcpClient(ServerIp, ServerPort);
                pwClient.ConnectToServer();
                if (!dictionary.Any())
                {
                    dictionary = pwClient.RequestDictionary();
                }

                string userInfoString = pwClient.RequestPassword();
                Console.WriteLine(userInfoString);

                if (userInfoString == "NONE") break;

                pwClient.Disconnect();

                Console.WriteLine("Creating user info");
                var userInfoArray = userInfoString.Split(':');
                UserInfo user = new UserInfo(userInfoArray[0], userInfoArray[1]);

                Console.WriteLine(dictionary.Count);
                Console.WriteLine(user.Username);
                
                var cracked = DoCrackIt(user, dictionary);

                if (cracked is not null)
                { 
                    pwClient.ConnectToServer();
                    pwClient.ReportResult(cracked);
                    pwClient.Disconnect();
                }
            }

            Console.WriteLine("End");
            Console.ReadKey();
        }

        public static UserInfoClearText DoCrackIt(UserInfo user, List<string> words)
        {
            ParallelCracking manager = new ParallelCracking();

            UserInfoClearText result = null;
            Stopwatch watch = new Stopwatch();

            try
            {
                watch.Start();
                result = manager.StartParallelCracking(user, words);
                watch.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Console.WriteLine($"Elapsed time: {watch.Elapsed.TotalSeconds} seconds");
            }

            if (result != null)
            {
                Console.WriteLine($"Success: {result.UserName}:{result.Password}");
                return result;
            }

            Console.WriteLine("Not found.");
            return null;
            
        }
    }
}
