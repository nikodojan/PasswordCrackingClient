using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using PasswordClient.Models;

namespace PasswordClient
{
    public class PwTcpClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;
        
        public PwTcpClient(string hostname, int port)
        {
            IpAddress = hostname;
            Port = port;
        }
        
        public string IpAddress { get; set; }
        public int Port { get; set; }

        /// <summary>
        /// Connect to the server and create instances of: TcpClient, NetworkStream, StreamReader, StreamWriter
        /// </summary>
        public void ConnectToServer()
        {
            _client = new TcpClient(IpAddress, Port);
            Console.WriteLine("Connected to server");
            _stream = _client.GetStream();
            _reader = new StreamReader(_stream);
            _writer = new StreamWriter(_stream);
        }

        /// <summary>
        /// Disconnect from server. (Close all resources)
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _reader?.Close();
                _writer?.Close();
                _stream?.Close();
                _client?.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Requests and reveices the dictionary file from the server.
        /// Reads the received file into a List<string>.
        /// </summary>
        /// <returns></returns>
        public List<string> RequestDictionary()
        {
            _writer.WriteLine("dictionary");
            _writer.Flush();
            List<string> words = new List<string>();
            // receive a file from the server and save it in the specified directory with the specified filename
            int thisRead = 1;
            int bytesPerRead = 1024;
            byte[] buffer = new byte[bytesPerRead];

            using (FileStream fs = File.Create("dictionary.txt"))
            {
                do
                {
                    thisRead = _stream.Read(buffer, 0, buffer.Length);

                    fs.Write(buffer, 0, thisRead);
                    Console.Write($"\r{thisRead}");
                } while (_stream.DataAvailable);
                fs.Close();
                Console.WriteLine("Dictionary received");
            }

            // read all words from the received file and add them to a list
            words = File.ReadLines("dictionary.txt").ToList();
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

        public void ReportResult(UserInfoClearText result)
        {
            Console.WriteLine("Reporting in");
            _writer.WriteLine("report");
            _writer.Flush();

            if(_reader.ReadLine() != "OK") _client.Close();
            Console.WriteLine("Server accepts report");
            var userInfo = result.UserName + ":" + result.Password;
            _writer.WriteLine(userInfo);
            _writer.Flush();
        }
    }
}
