using RCG.Infrastructure;
using RCG.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RCG.Models
{
    class PlayerClient : NotifiedBase
    {
        int playerCount;
        Role role;
        Status status;
        TcpClient client;
        Dictionary<string, TcpClient> hosts;

        public string Name { get => SaveLoad.SaveObject.Instance.PlayerName; set { SaveLoad.SaveObject.Instance.PlayerName = value; Notify(); } }
        public Status Status { get => status; private set { status = value; Notify(); } }
        public Role Role { get => role; private set { role = value; Notify(); } }
        public List<string> ServerList { get => hosts.Keys.ToList(); }
        public int PlayerCount { get => playerCount; private set { playerCount = value; Notify(); } }

        public PlayerClient() { }

        public void Listen()
        {
            if (client != null && client.Connected)
            {
                var reader = client.GetReader();
                while (true)
                {
                    try
                    {
                        MessageType messageType = (MessageType)Enum.Parse(typeof(MessageType), reader.ReadString());
                        switch (messageType)
                        {
                            case MessageType.StartWithRole:
                                Role = (Role)Enum.Parse(typeof(Role), reader.ReadString());
                                break;
                            case MessageType.Status:
                                Status = (Status)Enum.Parse(typeof(Status), reader.ReadString());
                                break;
                            case MessageType.WaitingPlayerCount:
                                PlayerCount = reader.ReadInt32();
                                break;
                            case MessageType.Disconnect:
                                Disconnect();
                                Status = Status.Disconnected;
                                break;
                        }
                    }
                    catch { break; }
                }
            }
        }

        public void Connect(string hostName, string password)
        {
            hosts[hostName].GetWriter().Write(password);
            if ((MessageType)Enum.Parse(typeof(MessageType), hosts[hostName].GetReader().ReadString()) == MessageType.Connected)
            {
                client = hosts[hostName];
                hosts.Remove(hostName);
                NetworkHelper.CloseAll(hosts.Values.ToList());
                PlayerCount = 0;
                ThreadPool.QueueUserWorkItem(o => Listen());
            }
            else
                throw new Exception(LocalizationManager.GetLocalizationManager().GetString("WrongPass"));
        }

        public void Refresh()
        {
            client = null;
            hosts = new Dictionary<string, TcpClient>();
            Notify("ServerList");
            hosts = NetworkHelper.FindHosts();
            Notify("ServerList");
        }

        public void Disconnect()
        {
            try
            {
                client?.GetWriter().Write(MessageType.Disconnect.ToString());
                client?.Close();
            }
            catch { }
        }
    }
}
