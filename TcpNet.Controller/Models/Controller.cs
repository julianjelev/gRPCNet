using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpNet.Controller.Models
{
    public class Controller
    {
        public string ConcentratorId { get; set; }

        public string GameControllerId { get; set; }

        public List<Card> Cards { get; set; }

        public int EndpointRssi { get; set; }

        public int ConcentratorRssi { get; set; }

        public void VerifyCards(CancellationToken token)
        {
            Console.WriteLine($"Controller {GameControllerId} is Connecting ...");
            using (var client = new TcpClient())
            {
                try
                {
                    //client.Connect("192.168.1.198", 8877);
                    client.Connect("192.168.1.198", 502);
                    Console.WriteLine($"Controller {GameControllerId} Connected");

                    var indx = 0;
                    var transactionId = 1;
                    for (; ; )
                    {
                        if (token.IsCancellationRequested)
                            break;

                        if (indx >= Cards.Count)
                            indx = 0;

                        if (transactionId > 255)
                            transactionId = 1;

                        var request = new CanPlayRequest
                        {
                            ConcentratorId = ConcentratorId,
                            GameControllerId = GameControllerId,
                            CardType = Cards[indx].CardType,
                            CardId = Cards[indx].CardId,
                            ShoulPay = false,
                            TransactionId = transactionId,
                            EndpointRssi = EndpointRssi,
                            ConcentratorRssi = ConcentratorRssi
                        };
                        byte[] outputBuffer = request.SerializeASCII();

                        // Get a client stream for reading and writing.
                        NetworkStream stream = client.GetStream();

                        // Send the message to the connected TcpServer.
                        stream.Write(outputBuffer, 0, outputBuffer.Length);
                        stream.Flush();

                        Console.WriteLine("Controller {0} Sent: {1}", GameControllerId, BitConverter.ToString(outputBuffer));

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

                        //извличане на получените данни
                        byte[][] splittedMessage = SplitByteArray(receivedBytes, 0x1F).ToArray();

                        if (splittedMessage.Count() <= 1)
                            Console.WriteLine("Controller {0} Received: {1}",
                                GameControllerId,
                                new string(
                                    receivedBytes
                                        .Select(b => (char)b)
                                        .ToArray()));
                        else
                        {
                            var response = CanPlaySuccessResponse.DeserializeASCII(splittedMessage);
                            Console.WriteLine("Controller {0} Received:\n\tconcentrator {1}\n\tcontroller {2}\n\tcard {3}\n\ttransaction {4}\n\tgame {5}\n\tpermission {6}\n\trelay type {7}\n\trelay pulse {8}\n\trelay on time {9}\n\trelay off time {10}\n\trelay display time {11}\n\tline1 {12}\n\tline2 {13}",
                                response.GameControllerId,
                                response.ConcentratorId,
                                response.GameControllerId,
                                response.CardId,
                                response.TransactionId,
                                response.GameId,
                                response.Permission,
                                response.RelayType,
                                response.RelayPulse,
                                response.RelayOnTime,
                                response.RelayOffTime,
                                response.RelayDisplayTime,
                                response.MessageLine1,
                                response.MessageLine2);
                        }

                        indx++;
                        transactionId++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            Console.WriteLine($"Controller {GameControllerId} disconnected");
        }

        #region utils

        private IEnumerable<byte[]> SplitByteArray(IEnumerable<byte> source, byte delimiter)
        {
            if (source == null) throw new ArgumentNullException("source is null");

            List<byte> current = new List<byte>();

            foreach (byte b in source)
            {
                if (b == delimiter)
                {
                    if (current.Count > 0)
                        yield return current.ToArray(); // copies the elements of the List to a new Array

                    current.Clear();
                }
                current.Add(b);
            }

            if (current.Count > 0)
                yield return current.ToArray(); // copies the elements of the List to a new Array
        }

        #endregion
    }
}
