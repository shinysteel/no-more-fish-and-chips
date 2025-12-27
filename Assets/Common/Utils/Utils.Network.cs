using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace ShinyOwl.Common.Utils
{
    public static partial class Utils
    {
        public static class Network
        {
            public static string GetLocalIpAddress()
            {
                // Abitrary values just so we can discover the address
                string probeIp = "8.8.8.8";
                int probePort = 7776;

                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP))
                {
                    socket.Connect(probeIp, probePort);
                    IPEndPoint endPoint = (IPEndPoint)socket.LocalEndPoint;
                    return endPoint.Address.ToString();
                }
            }
        }
    }
}