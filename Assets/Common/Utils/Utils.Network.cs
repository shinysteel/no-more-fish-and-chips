using PurrNet;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System;
using UnityEngine.Pool;

using Object = UnityEngine.Object;

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

            public static void CacheSyncDictionaryChange<T, U>(Dictionary<T, U> cache, SyncDictionaryChange<T, U> change, Action<T, U, U> raiseChanged) 
                where U : Object
            {
                U previous = cache.GetValueOrDefault(change.key);

                switch (change.operation)
                {
                    case SyncDictionaryOperation.Added:
                        cache.Add(change.key, change.value);
                        raiseChanged?.Invoke(change.key, null, cache[change.key]);
                        break;

                    case SyncDictionaryOperation.Removed:
                        cache.Remove(change.key);
                        raiseChanged?.Invoke(change.key, previous, null);
                        break;

                    case SyncDictionaryOperation.Set:
                        cache[change.key] = change.value;
                        raiseChanged?.Invoke(change.key, previous, cache[change.key]);
                        break;

                    case SyncDictionaryOperation.Cleared:
                        Dictionary<T, U> dictionary = DictionaryPool<T, U>.Get();

                        foreach (KeyValuePair<T, U> kvp in cache)
                        {
                            dictionary.Add(kvp.Key, kvp.Value);
                        }

                        cache.Clear();

                        foreach (KeyValuePair<T, U> kvp in cache)
                        {
                            raiseChanged?.Invoke(kvp.Key, kvp.Value, null);
                        }

                        DictionaryPool<T, U>.Release(dictionary);

                        break;
                }
            }
        }
    }
}