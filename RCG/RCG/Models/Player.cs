using RCG.Infrastructure;
using RCG.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Xamarin.Forms;

namespace RCG.Models
{
    class Player : NotifiedBase
    {
        string name;
        TcpClient client;
        BinaryWriter writer;
        BinaryReader reader;
        Role role = Role.None;
        bool active = true;
        int votedFor;
        bool isAlibi = false;

        public string Name { get => name; set { name = value; Notify(); } }
        public bool Alive { get => active; set { active = value; Notify(); } }
        public int VotedFor { get => votedFor; set { votedFor = value; Notify(); } }
        public bool Killed { get; set; }
        public Role Role { get => role; set { role = value; Notify(); Notify("RoleStr"); } }
        public string RoleStr { get => LocalizationManager.GetLocalizationManager().GetString(role.ToString()); }
        public bool IsAlibi { get => isAlibi; set { isAlibi = value; Notify(); Notify("Color"); } }
        public Color Color { get => isAlibi ? Color.Green : Color.WhiteSmoke; }
        public int HealedDay { get; set; } = -2;
        public int GotAlibiDay { get; set; } = -2;

        public Player(TcpClient client)
        {
            this.client = client;
            writer = client.GetWriter();
        }

        public void DisconnectListener(Host host)
        {
            reader = client.GetReader();
            try
            {
                if ((MessageType)Enum.Parse(typeof(MessageType), reader.ReadString()) == MessageType.Disconnect)
                {
                    host.Players.Remove(this);
                    reader.Close();
                    Close();
                }
            }
            catch { }
        }

        public void SendRole()
        {
            writer.Write(MessageType.StartWithRole.ToString());
            writer.Write(role.ToString());
            writer.Flush();
        }

        public void SendStatus(Status status)
        {
            writer.Write(MessageType.Status.ToString());
            writer.Write(status.ToString());
            writer.Flush();
        }

        public void SendPlayerCount(int count)
        {
            writer.Write(MessageType.WaitingPlayerCount.ToString());
            writer.Write(count);
            writer.Flush();
        }

        public void Close()
        {
            if (client.Connected)
            {
                reader.Close();
                try
                {
                    writer.Write(MessageType.Disconnect.ToString());
                    writer.Flush();
                }
                catch { }
                writer.Close();
                client.Close();
            }
        }
    }
}
