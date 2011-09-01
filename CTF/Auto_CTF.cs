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
    public class Base
    {
        public int x;
        public int y;
        public int z;
        public byte block;
        public Base(int x, int y, int z, Team team)
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
                        redbase.x = int.Parse(l.Split('=')[1]);
                        break;
                    case "base.red.y":
                        redbase.y = int.Parse(l.Split('=')[1]);
                        break;
                    case "base.red.z":
                        redbase.z = int.Parse(l.Split('=')[1]);
                        break;
                    case "base.red.block":
                        redbase.block = Block.Byte(l.Split('=')[1]);
                        break;
                    case "base.blue.x":
                        bluebase.x = int.Parse(l.Split('=')[1]);
                        break;
                    case "base.blue.y":
                        bluebase.y = int.Parse(l.Split('=')[1]);
                        break;
                    case "base.blue.z":
                        bluebase.z = int.Parse(l.Split('=')[1]);
                        break;
                    case "base.blue.block":
                        bluebase.block = Block.Byte(l.Split('=')[1]);
                        break;
                    case "map.line.x":
                        xline = int.Parse(l.Split('=')[1]);
                        break;
                    case "map.line.z":
                        zline = int.Parse(l.Split('=')[1]);
                        break;
                }
            }
            File.Copy("CTF/maps/" + mapname + ".lvl", "levels/ctf.lvl");
            Command.all.Find("load").Use(null, "ctf");
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
            throw new NotImplementedException();
        }
        void Player_PlayerCommand(string cmd, Player p, string message)
        {
            if (cmd == "goto")
            {
                if (message == "ctf" && p.level.name != "ctf")
                {
                    if (blueteam.members.Count > redteam.members.Count)
                    {
                        redteam.Add(p);
                        Player.GlobalMessageLevel(mainlevel, p.color + p.name + " " + c.Parse("red") + "joined the RED Team");
                    }
                    else if (redteam.members.Count > blueteam.members.Count)
                    {
                        blueteam.Add(p);
                        Player.GlobalMessageLevel(mainlevel, p.color + p.name + " " + c.Parse("blue") + "joined the BLUE Team");
                    }
                    else if (new Random().Next(2) == 0)
                    {
                        redteam.Add(p);
                        Player.GlobalMessageLevel(mainlevel, p.color + p.name + " " + c.Parse("red") + "joined the RED Team");
                    }
                    else
                    {
                        blueteam.Add(p);
                        Player.GlobalMessageLevel(mainlevel, p.color + p.name + " " + c.Parse("blue") + "joined the BLUE Team");
                    }
                }
                else if (message != "ctf" && p.level.name == "ctf")
                {
                    if (blueteam.members.Contains(p))
                    {
                        blueteam.members.Remove(p);
                        Player.GlobalMessageLevel(mainlevel, p.color + p.name + " " + blueteam.color + "left the ctf game");
                    }
                    else if (redteam.members.Contains(p))
                    {
                        redteam.members.Remove(p);
                        Player.GlobalMessageLevel(mainlevel, p.color + p.name + " " + redteam.color + "left the ctf game");
                    }
                }
            }
        }
        void Player_PlayerChat(Player p, string message)
        {
            throw new NotImplementedException();
        }

        void Player_PlayerDeath(Player p, byte deathblock)
        {
            throw new NotImplementedException();
        }

        void Player_PlayerMove(Player p, ushort x, ushort y, ushort z)
        {
            throw new NotImplementedException();
        }
    }
}
