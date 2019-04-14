using RCG.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RCG.Models
{
    static class NetworkHelper
    {
        public static int Port { get; } = 4556;

        public static bool CheckNetwork()
        {
            Ping ping = new Ping();
            if (ping.Send("8.8.8.8", 1000).Status == IPStatus.Success)
                return true;
            return false;
        }

        public static string GetLocalIPAddress()
        {
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                var ipString = ip.ToString();
                if (ip.AddressFamily == AddressFamily.InterNetwork && ipString.Contains("192.168."))
                    return ipString;
            }
            throw new Exception(LocalizationManager.GetLocalizationManager().GetString("WrongWifi"));
        }

        static string GetBaseIp(string ip)
        {
            var parts = ip.Split('.');
            var list = parts.ToList();
            list.RemoveAt(list.Count - 1);
            return string.Join(".", list);
        }

        static List<string> AllIps(string ip)
        {
            List<string> ips = new List<string>();
            ip = GetBaseIp(ip);
            for (int i = 1; i < 255; i++)
                ips.Add(string.Concat(ip, ".", i.ToString()));
            return ips;
        }

        public static Dictionary<string, TcpClient> FindHosts()
        {
            Dictionary<string, TcpClient> res = new Dictionary<string, TcpClient>();
            List<Task> tasks = new List<Task>();
            foreach(string ip in AllIps(GetLocalIPAddress()))
                tasks.Add(TryConnectAsync(res, ip));
            Task.WaitAll(tasks.ToArray());
            return res;
        }

        static void TryConnect(Dictionary<string, TcpClient> res, string ip)
        {
            TcpClient client = new TcpClient();
            try
            {
                client.SendTimeout = 100;
                client.Connect(ip, Port);
                var name = client.GetReader().ReadString();
                res.Add(name, client);
            }
            catch
            {
                client.Close();
            }
        }

        async static Task TryConnectAsync(Dictionary<string, TcpClient> res, string ip)
        {
            await Task.Run(() => TryConnect(res, ip));
        }

        public static BinaryReader GetReader(this TcpClient tcpClient)
        {
            if (tcpClient.Connected)
                return new BinaryReader(tcpClient.GetStream());
            throw new Exception("Wrong client");
        }

        public static BinaryWriter GetWriter(this TcpClient tcpClient)
        {
            if (tcpClient.Connected)
                return new BinaryWriter(tcpClient.GetStream());
            throw new Exception("Wrong client");
        }

        public static void CloseAll(List<TcpClient> tcpClients)
        {
            foreach (var tcpClient in tcpClients)
                tcpClient.Close();
        }

        static void SendMail(string subj, string body, Stream attachment = null)
        {
            MailMessage msg = new MailMessage("kompasha.ent.report@gmail.com", "kompasha.ent.report@gmail.com",
                string.Concat("Mafia RCG - ", subj, " - ", SaveLoad.SaveObject.Instance.PlayerName),
                string.Concat(body, "\n\n", DateTime.Now.ToString(), "\n", Thread.CurrentThread.CurrentCulture.ToString()));
            SmtpClient client = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential("kompasha.ent.report@gmail.com", "Sweatdreams47"),
            };
            if (attachment != null)
            {
                var att = new Attachment(attachment, new ContentType(MediaTypeNames.Image.Jpeg));
                att.ContentDisposition.FileName = "Screenshot.jpg";
                msg.Attachments.Add(att);
            }
            client.Send(msg);
            client.Dispose();
            attachment?.Close();
        }

        public async static Task SendMailAsync(string subj, string body, Stream attachment = null)
        {
            await Task.Run(() => SendMail(subj, body, attachment));
        }
    }
}
