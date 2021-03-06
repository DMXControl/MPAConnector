﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MPAConnector.Elements;
using MPAConnector.json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MPAConnector
{
    public class MPAGlueConnector : IDisposable, IEnumerable<MPAChain>
    {
        private TcpClient client;

        private readonly ConcurrentDictionary<string, MPAChain> _chains = new ConcurrentDictionary<string, MPAChain>();
        private BlockingCollection<Event> _sendQueue;

        public event EventHandler<MPAChainEventArgs> ChainAdded;
        public event EventHandler<MPAChainEventArgs> ChainRemoved;

        public bool Connected => client?.Connected ?? false;

        public string RemoteHost { get; set; } = "127.0.0.1";

        public int RemotePort { get; set; } = 8192;

        public Action<string, bool> MessageDebugLogCallback { get; set; }

        public async Task connect()
        {
            Disconnect();

            client = new TcpClient();
            await client.ConnectAsync(RemoteHost, RemotePort).ConfigureAwait(false);

            _sendQueue = new BlockingCollection<Event>();

            Task.Run(() => RecieveJsonMessages());
            Task.Run(() => SendJsonMessages());
        }

        public void Disconnect()
        {
            _sendQueue?.CompleteAdding();
            _sendQueue = null;

            client?.Close();
            client = null;
        }

        private async void RecieveJsonMessages()
        {
            try
            {
                while (Connected)
                {
                    var n = client.GetStream();
                    byte[] sizeBytes = new byte[2];
                    await n.ReadAsync(sizeBytes, 0, 2).ConfigureAwait(false);

                    int size = sizeBytes[0] | (sizeBytes[1] << 8);

                    byte[] payload = new byte[size];
                    await n.ReadAsync(payload, 0, size).ConfigureAwait(false);

                    ParsePayload(payload);
                }
            }
            catch (Exception e) { }
        }

        private async void SendJsonMessages()
        {
            try
            {
                var x = _sendQueue;
                foreach (var e in x.GetConsumingEnumerable())
                {
                    if (!Connected) break;

                    var n = client.GetStream();
                    var json = JsonConvert.SerializeObject(e);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    var size = new byte[2];
                    size[0] = (byte)bytes.Length;
                    size[1] = (byte)(bytes.Length >> 8);

                    await n.WriteAsync(size, 0, 2).ConfigureAwait(false);
                    await n.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                }
            }
            catch (Exception ex) { }
        }

        internal void SendEvent(Event e)
        {
            if (Connected && !(_sendQueue?.IsAddingCompleted ?? true))
            {
                _sendQueue?.Add(e);
            }
        }

        private void ParsePayload(byte[] payload)
        {
            string s = Encoding.UTF8.GetString(payload);

            JObject result = JObject.Parse(s);

            JToken msg_type;
            bool success = false;
            if (result.TryGetValue("msg_type", StringComparison.InvariantCultureIgnoreCase, out msg_type))
            {
                switch (msg_type.Value<string>().ToLowerInvariant())
                {
                    case "chain-init":
                        var x = JsonConvert.DeserializeObject(s, typeof(ChainInit)) as ChainInit;
                        success = ProcessChainInit(x);
                        break;

                    case "chain-exit":
                        JToken chainId;
                        if (result.TryGetValue("chain_id", StringComparison.InvariantCultureIgnoreCase, out chainId))
                            success = ProcessChainExit(chainId.Value<string>());
                        break;

                    case "event":
                        var y = JsonConvert.DeserializeObject(s, typeof(Event)) as Event;
                        success = ProcessEvent(y);
                        break;
                }
            }

            MessageDebugLogCallback?.Invoke(s, success);
        }

        private bool ProcessChainInit(ChainInit i)
        {
            MPAChain nc = new MPAChain(i.ChainID, this);
            foreach (var bi in i.BoardInfos)
            {
                MPATile t = new MPATile(bi.Bid, bi.Pid, bi.Nid, nc);
                nc.AddTile(t);
            }

            _chains[i.ChainID] = nc;

            ChainAdded?.Invoke(this, new MPAChainEventArgs(nc));
            return true;
        }

        private bool ProcessChainExit(string chainId)
        {
            MPAChain c = null;
            if (_chains.TryRemove(chainId, out c))
            {
                ChainRemoved?.Invoke(this, new MPAChainEventArgs(c));
            }
            return true;
        }

        private bool ProcessEvent(Event e)
        {
            MPATile t = _chains.Values.Select(c => c[e.Nid]).FirstOrDefault(c => c != null);

            var success = t?.ProcessEvent(e) ?? false;

            return success;
        }
        
        public void Dispose()
        {
            Disconnect();

            ChainAdded = null;
            ChainRemoved = null;
        }

        public IEnumerator<MPAChain> GetEnumerator()
        {
            return _chains.Values.ToList().GetEnumerator(); //Return Enumerator on copy
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class MPAChainEventArgs : EventArgs
    {
        public readonly MPAChain Chain;

        public MPAChainEventArgs(MPAChain chain)
        {
            this.Chain = chain;
        }
    }
}
