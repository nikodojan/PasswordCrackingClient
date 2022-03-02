using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace PasswordClient
{
    public class PwTcpClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;

        private const string Dir =
            @"D:\_4thSem\_SECURITY\PasswordCrackingAssignment\PasswordClient\PasswordClient\Files\";

        public PwTcpClient(string hostname, int port)
        {
            IpAddress = hostname;
            Port = port;
            _client = new TcpClient(IpAddress, Port);

        }

        public List<string> Dict { get; set; } = new List<string>();
        public string Password { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }

        public void RequestData()
        {

            Console.WriteLine("Connected to server");

            _stream = _client.GetStream();
            _reader = new StreamReader(_stream);
            _writer = new StreamWriter(_stream);

            Password = RequestPassword();
            Console.WriteLine("Debug: pw received");
            Dict = RequestDictionary();

            Console.WriteLine("All data received");
        }

        /// <summary>
        /// Requests and reveices the dictionary file from the server.
        /// Reads the received file into a List<string>.
        /// </summary>
        /// <returns></returns>
        private List<string> RequestDictionary()
        {
            _writer.WriteLine("dictionary");
            _writer.Flush();
            List<string> words = new List<string>();
            Console.WriteLine("Debug: dict requested");
            // receive a file from the server and save it in the specified directory with the specified filename
            int thisRead = 1;
            int bytesPerRead = 1024;
            byte[] buffer = new byte[bytesPerRead];

            using (FileStream fs = File.Create(Dir + "dictionary.txt"))
            {
                Console.WriteLine("Start reading dictionary");
                while (thisRead > 0)
                {
                    thisRead = _stream.Read(buffer, 0, buffer.Length);
                    fs.Write(buffer, 0, thisRead);
                }
                fs.Close();
                Console.WriteLine("Dictionary received");
            }

            // read all words from the received file and add them to a list
            foreach (var line in File.ReadLines(Dir + "dictionary.txt"))
            {
                words.Add(line);
            }

            return words;
        }

        public string RequestPassword()
        {
            Console.WriteLine("Requesting pw");
            _writer.WriteLine("password");
            _writer.Flush();
            string pw =_reader.ReadLine();
            Console.WriteLine("Password received");
            return pw;
        }


    }
}
