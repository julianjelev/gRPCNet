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
            List<Models.Controller> controllers = null;

            using (StreamReader sr = new StreamReader("../../../controllers.json")) 
            {
                string json = sr.ReadToEnd();
                try
                {
                    controllers = JsonConvert.DeserializeObject<List<Models.Controller>>(json);
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
            // blocking current thread
            Console.ReadLine();

            

            CancellationTokenSource cts = new CancellationTokenSource();
            foreach (var controller in controllers) 
            {
                new Thread(() => 
                {
                    Thread.CurrentThread.IsBackground = true;
                    controller.VerifyCards(cts.Token);
                }).Start();
            }

            Console.WriteLine("Type Enter to terminate the program");
            // blocking current thread
            Console.ReadLine();
            // Request cancellation.
            cts.Cancel();
            Console.WriteLine("Cancellation set in token source... Please Wait 5 sec");
            Thread.Sleep(5000);
            // Cancellation should have happened, so call Dispose.
            cts.Dispose();
            Console.WriteLine("Progam is terminated");
        }   
    }
}




/*
 

//Console.WriteLine("Controller {0} Sent: {1}", Id, string.Join(string.Empty, outputBuffer.Select(b => b.ToString())));

// convert ascii char to byte: '5' - '0' => 0x35 - 0x30 = 0x5
//sendBytes.AddRange(Concentrator.Select(c => (byte)(c - '0')));


//Console.WriteLine("Controller {0} Received: {1}", 
                        //    Id, 
                        //    new string(
                        //        receivedBytes
                        //            .Select(b => b != 0x0A ? (char)(b + (byte)0x30) : (char)b) // convert byte to ascii char except '\n' 
                        //            .ToArray()));

                        //Console.WriteLine("Controller {0} Received: {1}", Id, string.Join(string.Empty, receivedBytes.Select(b => b.ToString())));
 */