using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpNet.Controller
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Controller> controllers = null;

            using (StreamReader sr = new StreamReader("../../../controllers.json")) 
            {
                string json = sr.ReadToEnd();
                try
                {
                    controllers = JsonConvert.DeserializeObject<List<Controller>>(json);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine(ex.ToString());
                    controllers = null;
                }
            }

            if (controllers == null)
            {
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Type Enter to start messaging ...");
            Console.ReadLine();

            foreach (var controller in controllers) 
            {
                new Thread(() => 
                {
                    Thread.CurrentThread.IsBackground = true;
                    controller.VerifyCards();
                }).Start();
            }

            // blocking current thread
            Console.WriteLine("Type Enter to terminate the program");
            Console.ReadLine();
        }   
    }

    public class Controller
    {
        public int Id { get; set; }
        public List<string> Cards { get; set; }

        public void VerifyCards() 
        {
            Console.WriteLine($"Controller {Id} is Connecting ...");
            using (var client = new TcpClient())
            {
                try
                {
                    client.Connect("192.168.1.198", 8877);
                    Console.WriteLine($"Controller {Id} Connected");
                    foreach (var card in Cards)
                    {
                        // Get a client stream for reading and writing.
                        NetworkStream stream = client.GetStream();
                        byte[] outputBuffer = card.Select(c => (byte)(c - '0')).ToArray();
                        // Send the message to the connected TcpServer.
                        stream.Write(outputBuffer, 0, outputBuffer.Length);
                        stream.Flush();

                        Console.WriteLine("Controller {0} Sent: {1}", Id, string.Join(string.Empty, outputBuffer.Select(b => b.ToString())));

                        // Buffer to store the response bytes.
                        byte[] inputBuffer = new byte[32];
                        List<byte> receivedBytes = new List<byte>();
                        int numberOfBytesRead = 0;
                        do
                        {
                            if (stream.CanRead)
                                numberOfBytesRead = stream.Read(inputBuffer, 0, inputBuffer.Length);
                            else
                                break;

                            receivedBytes.AddRange(inputBuffer.Take(numberOfBytesRead));
                        }
                        while (stream.DataAvailable);

                        Console.WriteLine("Controller {0} Received: {1}", Id, string.Join(string.Empty, receivedBytes.Select(b => b.ToString())));

                        //stream.Close();// !!!
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            Console.WriteLine($"Controller {Id} disconnected");
        }
    }
}

    /*TcpClient client = new TcpClient();

            try
            {
                Console.WriteLine("Try to connect");
                client.Connect("192.168.1.198", 8877);

                while (true)
                {
                    // Get a client stream for reading and writing.
                    // Stream stream = client.GetStream();
                    NetworkStream stream = client.GetStream();

                    byte[] outputBuffer = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

                    // Send the message to the connected TcpServer.
                    stream.Write(outputBuffer, 0, outputBuffer.Length);
                    stream.Flush();

                    Console.WriteLine("Sent: {0}", string.Join(string.Empty, outputBuffer.Select(b => b.ToString())));

                    // Receive the TcpServer.response.
                    // Buffer to store the response bytes.
                    byte[] inputBuffer = new byte[10];//
                    // Read the first batch of the TcpServer response bytes.
                    int numberOfBytesRead = stream.Read(inputBuffer, 0, inputBuffer.Length);

                    Console.WriteLine("Received: {0}", string.Join(string.Empty, inputBuffer.Select(b => b.ToString())));
                    
                    string ln = Console.ReadLine();
                    
                    if (ln.Contains("exit"))
                    {
                        stream.Close();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            client.Close();
        }*/