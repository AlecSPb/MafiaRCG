using RCG.Infrastructure;
using RCG.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RCG.Models
{
    class Host : NotifiedBase
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Any, NetworkHelper.Port);
        bool isAction = true;
        bool isNext = false;
        int dayN = 0;
        string actionName;
        int alreadyVoted;
        GamePhase currentPhase;
        int currentPhaseIndex = 0;
        Dictionary<GamePhase, Action<Player>> gamePhases = new Dictionary<GamePhase, Action<Player>>();
        string resString = string.Empty;
        LocalizationManager lm = LocalizationManager.GetLocalizationManager();
        Timer timer;
        DateTime dayStartTime;

        public List<Player> Players { get; } = new List<Player>();
        public HostTemplate HostTemplate { get; }
        int PlayerCount { get => Players.Count(p => p.Alive); }
        public Action<Player> Action { get => gamePhases[currentPhase]; }
        public string ActionName { get => actionName; set { actionName = value; Notify(); } }
        public bool IsAction { get => isAction; set { isAction = value; Notify(); } }
        public bool IsVote { get => currentPhase == GamePhase.Vote; }
        public bool IsNext { get => isNext; set { isNext = value; Notify(); } }
        int GetBlack { get => Players.Where(p => p.Alive && (p.Role == Role.Mafia || p.Role == Role.Don)).Count(); }
        int GetRed { get => Players.Where(p => p.Alive && (p.Role != Role.Mafia && p.Role != Role.Don && p.Role != Role.Assassin)).Count(); }
        bool IsManiacDead { get => Players.Where(p => p.Alive && p.Role == Role.Assassin).Count() == 0; }
        public string AliveStr => string.Concat(lm.GetString("Alive"), " - ", PlayerCount.ToString());
        public string CountStr => string.Concat(lm.GetString("PlayersConnected"), " - ", PlayerCount.ToString());
        public string ResString { get => resString; set { resString = value; Notify(); } }
        public string GamePhaseStr
        {
            get
            {
                string res = lm.GetString(currentPhase.ToString());
                if (currentPhase == GamePhase.Day)
                    res = string.Concat(res, " ", dayN.ToString(), " - ", (dayStartTime - DateTime.Now).ToString("hh\\:mm\\:ss"));
                return res;
            }
        }

        public Host(HostTemplate hostTemplate)
        {
            HostTemplate = hostTemplate;
        }

        void Listen()
        {
            tcpListener.Start();
            while (true)
            {
                try
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    var writer = client.GetWriter();
                    writer.Write(HostTemplate.Name);
                    ThreadPool.QueueUserWorkItem(o => GetPlayer(client, writer));
                }
                catch { break; }
            }
        }

        void GetPlayer(TcpClient client, BinaryWriter writer)
        {
            while (true)
                if (client.GetReader().ReadString() == HostTemplate.Password)
                {
                    writer.Write(MessageType.Connected.ToString());
                    var player = new Player(client);
                    ThreadPool.QueueUserWorkItem(o => player.DisconnectListener(this));
                    Players.Add(player);
                    SendCount();
                    Notify("CountStr");
                    break;
                }
                else
                    writer.Write(MessageType.WrongPass.ToString());
        }

        void SendCount()
        {
            foreach (var player in Players)
                player.SendPlayerCount(PlayerCount);
        }

        public void Open()
        {
            ThreadPool.QueueUserWorkItem(o => Listen());
        }


        public void Start()
        {
            if (PlayerCount > 5)
            {
                tcpListener.Stop();
                RandCards();
                foreach (var player in Players)
                    player.SendRole();
            }
            else
                throw new Exception(lm.GetString("NotEnoghPlayers"));
        }

        public void Stop()
        {
            tcpListener.Stop();
            timer?.Dispose();
            foreach (var player in Players)
                player.Close();
        }

        void Victory()
        {
            Status status = Status.None;
            if ((GetRed == 0 && GetBlack == 0) || (!IsManiacDead && PlayerCount == 2))
                status = Status.AssassinWon;
            else if (IsManiacDead && GetBlack == 0)
                status = Status.CitizensWon;
            else if (IsManiacDead && GetRed <= GetBlack)
                status = Status.MafiaWon;
            if(status != Status.None)
            {
                foreach (var player in Players)
                    player.SendStatus(status);
                throw new Exception(lm.GetString(status.ToString()));
            }
        }

        public void NextPhase()
        {
            if (currentPhase == GamePhase.Vote)
                Arrest();
            if (currentPhaseIndex < gamePhases.Count - 1)
                currentPhaseIndex++;
            else
                currentPhaseIndex = 0;
            currentPhase = gamePhases.Keys.ToList()[currentPhaseIndex];
            if (currentPhase == GamePhase.Day)
            {
                NightEnd();
                dayN++;
                timer = new Timer((o) => Notify("GamePhaseStr"), null, 0, 100);
                dayStartTime = DateTime.Now;
            }
            if(currentPhase == GamePhase.Vote)
                timer.Dispose();
            if (currentPhase == GamePhase.Day
                || currentPhase == GamePhase.NightDetecive
                || currentPhase == GamePhase.NightDon
                || currentPhase == GamePhase.Vote
                || (currentPhase == GamePhase.NightDoctor && !Players.Where(p => p.Role == Role.Doctor).Single().Alive)
                || (currentPhase == GamePhase.NichtGirl && !Players.Where(p => p.Role == Role.Girl).Single().Alive)
                || (currentPhase == GamePhase.NightAssassin && IsManiacDead)
                || (currentPhase == GamePhase.NightMafia && GetBlack == 0))
            {
                IsAction = false;
                IsNext = true;
            }
            else
            {
                IsNext = false;
                IsAction = true;
                SetActionName();
            }
            Notify("IsVote");
            Notify("GamePhaseStr");

        }

        void SetActionName()
        {
            switch (currentPhase)
            {
                case GamePhase.NightMafia:
                case GamePhase.NightAssassin:
                    ActionName = lm.GetString("Kill");
                    break;
                case GamePhase.NichtGirl:
                    ActionName = lm.GetString("Alibi");
                    break;
                case GamePhase.NightDoctor:
                    ActionName = lm.GetString("Heal");
                    break;
            }
        }

        void Vote(Player player)
        {
            if (alreadyVoted < PlayerCount && !player.IsAlibi)
            {
                player.VotedFor++;
                alreadyVoted++;
            }
        }

        public void Devote(Player player)
        {
            if (player.VotedFor > 0 && !player.IsAlibi)
            {
                player.VotedFor--;
                alreadyVoted--;
            }
        }

        void Arrest()
        {
            string res = lm.GetString("DrawVote");
            var list = Players.Where(p => p.VotedFor == Players.Max(pl => pl.VotedFor)).ToList();
            if (list.Count == 1)
            {
                list[0].Alive = false;
                list[0].SendStatus(Status.Killed);
                SendCount();
                res = string.Concat(lm.GetString("Arrested"), " - ", list[0].Name);
            }
            var lucky = Players.Where(p => p.IsAlibi).SingleOrDefault();
            if (lucky != null)
                lucky.IsAlibi = false;
            ResetVotes();
            Notify("AliveStr");
            Victory();
            ResString = res;
        }

        void NightEnd()
        {
            string res = string.Empty;
            foreach (var player in Players.Where(p => p.Killed))
            {
                res = string.Concat(res, "\t", player.Name, "\n");
                player.Killed = false;
                player.Alive = false;
                player.SendStatus(Status.Killed);
                SendCount();
            }
            Notify("AliveStr");
            Victory();
            if (res != string.Empty)
                res = string.Concat(lm.GetString("Killed"), ":\n", res);
            else
                res = lm.GetString("NobodyDied");
            ResString = res;
        }

        void Kill(Player player)
        {
            if (player.Role != Role.Immortal && player.Role != Role.Girl || (player.Role == Role.Girl && player.IsAlibi == true))
                player.Killed = true;
            if (player.IsAlibi)
                Players.Where(p => p.Role == Role.Girl).Single().Killed = true;
            NextPhase();
        }

        void Heal(Player player)
        {
            if (dayN - player.HealedDay > 1)
            {
                player.Killed = false;
                player.HealedDay = dayN;
                NextPhase();
            }
        }

        void Alibi(Player player)
        {
            if (dayN - player.GotAlibiDay > 1)
            {
                player.IsAlibi = true;
                player.GotAlibiDay = dayN;
                NextPhase();
            }
        }

        public void ResetVotes()
        {
            alreadyVoted = 0;
            foreach (var player in Players)
                player.VotedFor = 0;
        }


        void RandCards()
        {
            Dictionary<Role, int> dict = new Dictionary<Role, int>();
            foreach(Role role in Enum.GetValues(typeof(Role)))
                dict.Add(role, 0);
            if (HostTemplate.IsGirl)
            {
                dict[Role.Girl] = 1;
                gamePhases.Add(GamePhase.NichtGirl, Alibi);
            }
            gamePhases.Add(GamePhase.NightMafia, Kill);
            if (HostTemplate.IsManiac && PlayerCount > 8)
            {
                dict[Role.Assassin] = 1;
                gamePhases.Add(GamePhase.NightAssassin, Kill);
            }
            if (HostTemplate.IsDoctor)
            {
                dict[Role.Doctor] = 1;
                gamePhases.Add(GamePhase.NightDoctor, Heal);
            }
            if (HostTemplate.IsDetective)
            {
                dict[Role.Detective] = 1;
                gamePhases.Add(GamePhase.NightDetecive, null);
            }
            if (HostTemplate.IsDon)
            {
                dict[Role.Don] = 1;
                gamePhases.Add(GamePhase.NightDon, null);
            }
            if (HostTemplate.IsImmortal && PlayerCount > 8)
                dict[Role.Immortal] = 1;
            dict[Role.Mafia] = PlayerCount / 3 - 1;
            dict[Role.Civilian] = PlayerCount - dict.Values.Sum(v => v);
            Random r = new Random();
            var keys = dict.Keys.ToList();
            foreach(Role role in keys)
                while(dict[role] != 0)
                {
                    int n = r.Next(PlayerCount);
                    if (Players[n].Role != Role.None)
                        continue;
                    Players[n].Role = role;
                    dict[role]--;
                }
            gamePhases.Add(GamePhase.Day, null);
            gamePhases.Add(GamePhase.Vote, Vote);
            currentPhase = gamePhases.Keys.ToList()[0];
            Notify("GamePhaseStr");
            SetActionName();
        }

        public bool ActionPredicate(object o)
        {
            var player = o as Player;
            if (player != null)
                if ((currentPhase == GamePhase.Vote && (alreadyVoted >= PlayerCount || player.IsAlibi))
                    || (currentPhase == GamePhase.NightDoctor && dayN - player.HealedDay <= 1)
                    || (currentPhase == GamePhase.NichtGirl && dayN - player.GotAlibiDay <= 1))
                    return false;
            return true;
        }
    }
}
