using System;
using System.Collections.Generic;
using System.IO;

namespace MCForge
{
    public class Team
    {
        public string color;
        public List<Player> members;
        public Team(string color)
        {
            color = c.Parse(color);
            members = new List<Player>();
        }
        public void Add(Player p)
        {
            members.Add(p);
        }
        public bool isOnTeam(Player p)
        {
            if (members.IndexOf(p) != -1)
                return true;
            else
                return false;
        }
    }
    public class Data
    {
        public Player p;
        public bool hasflag;
        public bool blue;
        public bool chatting = false;
        public Data(bool team, Player p)
        {
            blue = team; this.p = p;
        }
    }
    public class Base
    {
        public ushort x;
        public ushort y;
        public ushort z;
        public byte block;
        public Base(ushort x, ushort y, ushort z, Team team)
        {
            this.x = x; this.y = y; this.z = z;
        }
        public Base()
        {
        }
    }
    public class Auto_CTF
    {
        public int xline;
        public bool started = false;
        public int zline;
        public int yline;
        Team redteam;
        Team blueteam;
        Base bluebase;
        Base redbase;
        Level mainlevel;
        List<string> maps = new List<string>();
        List<Data> cache = new List<Data>();
        string mapname = "";
        public void LoadMap(string map)
        {
            mapname = map;
            string[] lines = File.ReadAllLines("CTF/" + mapname + ".config");
            foreach (string l in lines)
            {
                switch (l.Split('=')[0])
                {
                    case "base.red.x":
                        redbase.x = ushort.Parse(l.Split('=')[1]);
                        break;
                    case "base.red.y":
                        redbase.y = ushort.Parse(l.Split('=')[1]);
                        break;
                    case "base.red.z":
                        redbase.z = ushort.Parse(l.Split('=')[1]);
                        break;
                    case "base.red.block":
                        redbase.block = Block.Byte(l.Split('=')[1]);
                        break;
                    case "base.blue.x":
                        bluebase.x = ushort.Parse(l.Split('=')[1]);
                        break;
                    case "base.blue.y":
                        bluebase.y = ushort.Parse(l.Split('=')[1]);
                        break;
                    case "base.blue.z":
                        bluebase.z = ushort.Parse(l.Split('=')[1]);
                        break;
                    case "base.blue.block":
                        bluebase.block = Block.Byte(l.Split('=')[1]);
                        break;
                    case "map.line.x":
                        xline = ushort.Parse(l.Split('=')[1]);
                        break;
                    case "map.line.z":
                        zline = ushort.Parse(l.Split('=')[1]);
                        break;
                }
            }
            Command.all.Find("unload").Use(null, "ctf");
            File.Copy("CTF/maps/" + mapname + ".lvl", "levels/ctf.lvl");
            Command.all.Find("load").Use(null, "ctf");
            mainlevel = Level.Find("ctf");
        }
        public Auto_CTF()
        {
            //Lets get started
            Player.PlayerMove += new Player.OnPlayerMove(Player_PlayerMove);
            Player.PlayerDeath += new Player.OnPlayerDeath(Player_PlayerDeath);
            Player.PlayerChat += new Player.OnPlayerChat(Player_PlayerChat);
            Player.PlayerCommand += new Player.OnPlayerCommand(Player_PlayerCommand);
            Player.PlayerBlockChange += new Player.BlockchangeEventHandler2(Player_PlayerBlockChange);
            //Load some configs
            string[] lines = File.ReadAllLines("CTF/maps.config");
            foreach (string l in lines)
                maps.Add(l);
            redbase = new Base();
            bluebase = new Base();
        }
        public void Start()
        {
            if (started)
                return;
            blueteam = new Team("blue");
            redteam = new Team("red");
            LoadMap(maps[new Random().Next(maps.Count)]);
            started = true;
        }

        void Player_PlayerBlockChange(Player p, ushort x, ushort y, ushort z, byte type)
        {
            if (p.level == mainlevel && !blueteam.members.Contains(p) && !redteam.members.Contains(p))
            {
                p.SendBlockchange(x, y, z, p.level.GetTile(x, y, z));
                Player.SendMessage(p, "You are not on a team!");
                Plugins.Plugin.CancelEvent(Plugins.Events.BlockChange, p);
            }
            if (p.level == mainlevel && blueteam.members.Contains(p) && x == redbase.x && y == redbase.y && z == redbase.z && mainlevel.GetTile(redbase.x, redbase.y, redbase.z) != Block.air)
            {
                Player.GlobalMessage(blueteam.color + p.name + " took the " + redteam.color + " red team's FLAG!");
                GetPlayer(p).hasflag = true;
            }
            if (p.level == mainlevel && redteam.members.Contains(p) && x == bluebase.x && y == bluebase.y && z == bluebase.z && mainlevel.GetTile(bluebase.x, bluebase.y, bluebase.z) != Block.air)
            {
                Player.GlobalMessage(redteam.color + p.name + " took the " + blueteam.color + " blue team's FLAG");
                GetPlayer(p).hasflag = true;
            }
            if (p.level == mainlevel && blueteam.members.Contains(p) && x == bluebase.x && y == bluebase.y && z == bluebase.z && mainlevel.GetTile(bluebase.x, bluebase.y, bluebase.z) != Block.air)
            {
                Player.SendMessage(p, "You cant take your own flag!");
                p.SendBlockchange(x, y, z, p.level.GetTile(x, y, z));
                Plugins.Plugin.CancelEvent(Plugins.Events.BlockChange, p);
            }
            if (p.level == mainlevel && redteam.members.Contains(p) && x == redbase.x && y == redbase.y && z == redbase.z && mainlevel.GetTile(redbase.x, redbase.y, redbase.z) != Block.air)
            {
                Player.SendMessage(p, "You cant take your own flag!");
                p.SendBlockchange(x, y, z, p.level.GetTile(x, y, z));
                Plugins.Plugin.CancelEvent(Plugins.Events.BlockChange, p);
            }
        }
        public Data GetPlayer(Player p)
        {
            foreach (Data d in cache)
            {
                if (d.p == p)
                    return d;
            }
            return null;
        }
        void Player_PlayerCommand(string cmd, Player p, string message)
        {
            if (cmd == "teamchat" && p.level == mainlevel)
            {
                if (GetPlayer(p) != null)
                {
                    Data d = GetPlayer(p);
                    if (d.chatting)
                    {
                        Player.SendMessage(d.p, "You are no longer chatting with your team!");
                        d.chatting = !d.chatting;
                    }
                    else
                    {
                        Player.SendMessage(d.p, "You are now chatting with your team!");
                        d.chatting = !d.chatting;
                    }
                    Plugins.Plugin.CancelEvent(Plugins.Events.PlayerCommand, p);
                }
            }
            if (cmd == "goto")
            {
                if (message == "ctf" && p.level.name != "ctf")
                {
                    if (blueteam.members.Count > redteam.members.Count)
                    {
                        cache.Add(new Data(false, p));
                        redteam.Add(p);
                        Player.GlobalMessageLevel(mainlevel, p.color + p.name + " " + c.Parse("red") + "joined the RED Team");
                    }
                    else if (redteam.members.Count > blueteam.members.Count)
                    {
                        cache.Add(new Data(true, p));
                        blueteam.Add(p);
                        Player.GlobalMessageLevel(mainlevel, p.color + p.name + " " + c.Parse("blue") + "joined the BLUE Team");
                    }
                    else if (new Random().Next(2) == 0)
                    {
                        cache.Add(new Data(false, p));
                        redteam.Add(p);
                        Player.GlobalMessageLevel(mainlevel, p.color + p.name + " " + c.Parse("red") + "joined the RED Team");
                    }
                    else
                    {
                        cache.Add(new Data(true, p));
                        blueteam.Add(p);
                        Player.GlobalMessageLevel(mainlevel, p.color + p.name + " " + c.Parse("blue") + "joined the BLUE Team");
                    }
                }
                else if (message != "ctf" && p.level.name == "ctf")
                {
                    if (blueteam.members.Contains(p))
                    {
                        cache.Remove(GetPlayer(p));
                        blueteam.members.Remove(p);
                        Player.GlobalMessageLevel(mainlevel, p.color + p.name + " " + blueteam.color + "left the ctf game");
                    }
                    else if (redteam.members.Contains(p))
                    {
                        cache.Remove(GetPlayer(p));
                        redteam.members.Remove(p);
                        Player.GlobalMessageLevel(mainlevel, p.color + p.name + " " + redteam.color + "left the ctf game");
                    }
                }
            }
        }
        void Player_PlayerChat(Player p, string message)
        {
            if (p.level == mainlevel)
            {
                if (GetPlayer(p).chatting)
                {
                    if (blueteam.members.Contains(p))
                    {
                        Player.players.ForEach(delegate(Player p1)
                        {
                            if (blueteam.members.Contains(p1))
                                Player.SendMessage(p1, blueteam.color + "<Team-Chat>" + p.color + p.name + ": " + message);
                        });
                        Plugins.Plugin.CancelEvent(Plugins.Events.PlayerChat, p);
                    }
                    if (redteam.members.Contains(p))
                    {
                        Player.players.ForEach(delegate(Player p1)
                        {
                            if (redteam.members.Contains(p1))
                                Player.SendMessage(p1, redteam.color + "<Team-Chat>" + p.color + p.name + ": " + message);
                        });
                        Plugins.Plugin.CancelEvent(Plugins.Events.PlayerChat, p);
                    }
                }
            }
        }
        void Player_PlayerDeath(Player p, byte deathblock)
        {

        }
        void Player_PlayerMove(Player p, ushort x, ushort y, ushort z)
        {
            if (blueteam.members.Contains(p) && x < xline && z < zline)
            {
                foreach (Player p1 in redteam.members)
                {
                    if (Math.Abs(p1.pos[0] - x) < 2 && Math.Abs(p1.pos[1] - y) < 2 && Math.Abs(p1.pos[2] / 32 - z) < 2)
                    {
                        Player.SendMessage(p1, p.color + p.name + Server.DefaultColor + " tagged you!");
                        Random rand = new Random();
                        ushort xx = (ushort)(rand.Next(0, mainlevel.width));
                        ushort yy = (ushort)(rand.Next(0, mainlevel.depth));
                        ushort zz = (ushort)(rand.Next(0, mainlevel.height));
                        while (mainlevel.GetTile(xx, yy, zz) != Block.air && xx < xline && zz < zline)
                        {
                            xx = (ushort)(rand.Next(0, mainlevel.width));
                            yy = (ushort)(rand.Next(0, mainlevel.depth));
                            zz = (ushort)(rand.Next(0, mainlevel.height));
                        }
                        p1.SendPos(0, xx, yy, zz, p1.rot[0], p1.rot[1]);
                        if (GetPlayer(p1).hasflag)
                        {
                            Player.GlobalMessage(redteam.color + p.name + " DROPPED THE FLAG!");
                            mainlevel.Blockchange(redbase.x, redbase.y, redbase.z, Block.red);
                            GetPlayer(p).hasflag = false;
                        }
                    }
                }
            }
            if (redteam.members.Contains(p) && x > xline && z > zline)
            {
                foreach (Player p1 in blueteam.members)
                {
                    if (Math.Abs(p1.pos[0] - x) < 2 && Math.Abs(p1.pos[1] - y) < 2 && Math.Abs(p1.pos[2] / 32 - z) < 2)
                    {
                        Player.SendMessage(p1, p.color + p.name + Server.DefaultColor + " tagged you!");
                        Random rand = new Random();
                        ushort xx = (ushort)(rand.Next(0, mainlevel.width));
                        ushort yy = (ushort)(rand.Next(0, mainlevel.depth));
                        ushort zz = (ushort)(rand.Next(0, mainlevel.height));
                        while (mainlevel.GetTile(xx, yy, zz) != Block.air && xx < xline && zz < zline)
                        {
                            xx = (ushort)(rand.Next(0, mainlevel.width));
                            yy = (ushort)(rand.Next(0, mainlevel.depth));
                            zz = (ushort)(rand.Next(0, mainlevel.height));
                        }
                        p1.SendPos(0, xx, yy, zz, p1.rot[0], p1.rot[1]);
                        if (GetPlayer(p1).hasflag)
                        {
                            Player.GlobalMessage(blueteam.color + p.name + " DROPPED THE FLAG!");
                            mainlevel.Blockchange(bluebase.x, bluebase.y, bluebase.z, Block.blue);
                            GetPlayer(p).hasflag = false;
                        }
                    }
                }
            }
        }
    }
}
