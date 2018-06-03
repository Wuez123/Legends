﻿using ENet;
using Legends.Core.Cryptography;
using Legends.Core.IO.MOB;
using Legends.Core.Protocol;
using Legends.Core.Protocol.Enum;
using Legends.Core.Protocol.Game;
using Legends.Core.Protocol.LoadingScreen;
using Legends.Core.Protocol.Messages.Game;
using Legends.Core.Protocol.Other;
using Legends.Core.Utils;
using Legends.Network;
using Legends.Records;
using Legends.World.Buildings;
using Legends.World.Entities;
using Legends.World.Entities.AI;
using Legends.World.Games.Maps;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Legends.World.Games
{
    public class Game
    {
        private Logger logger = new Logger();

        public const double REFRESH_RATE = 1000 / 60;

        public NetIdProvider NetIdProvider
        {
            get;
            private set;
        }
        public int Id
        {
            get;
            private set;
        }
        public string Name
        {
            get;
            private set;
        }
        public AIHero[] Players
        {
            get
            {
                return BlueTeam.Units.Values.OfType<AIHero>().Concat(PurpleTeam.Units.Values.OfType<AIHero>()).ToArray();
            }
        }
        public Team BlueTeam
        {
            get;
            private set;
        }
        public Team PurpleTeam
        {
            get;
            private set;
        }
        private int LastPlayerNo
        {
            get;
            set;
        }
        public bool CanStart
        {
            get
            {
                return Players.All(x => x.ReadyToSpawn);
            }
        }
        public bool Started
        {
            get;
            private set;
        }
        public Map Map
        {
            get;
            private set;
        }
        private Timer Timer
        {
            get;
            set;
        }
        private Stopwatch Stopwatch
        {
            get;
            set;
        }
        public float GameTime
        {
            get;
            private set;
        }
        private double NextSyncTime
        {
            get;
            set;
        }
        public Game(int id, string name, int mapId) // Enum MapType, switch -> instance
        {
            this.NetIdProvider = new NetIdProvider();
            this.Id = id;
            this.Name = name;
            this.BlueTeam = new Team(this, TeamId.BLUE);
            this.PurpleTeam = new Team(this, TeamId.PURPLE);
            this.Map = Map.CreateMap(mapId, this);
            this.Timer = new Timer(REFRESH_RATE);
        }
        /// <summary>
        /// Add player to the game and to his team 
        /// </summary>
        /// <param name="player"></param>
        public void AddUnit(Unit unit, TeamId teamId)
        {
            if (teamId == TeamId.BLUE)
            {
                unit.DefineTeam(BlueTeam);
                BlueTeam.AddUnit(unit);

            }
            else if (teamId == TeamId.PURPLE)
            {
                unit.DefineTeam(PurpleTeam);
                PurpleTeam.AddUnit(unit);
            }
            unit.Initialize();
        }
        public void RemoveUnit(Unit unit)
        {
            unit.Team.RemoveUnit(unit);
        }
        public bool Contains(long userId)
        {
            return Players.FirstOrDefault(x => x.Client.UserId == userId) != null;
        }
        public int PopNextPlayerNo()
        {
            return LastPlayerNo++;
        }
        public void Send(Message message, Channel channel = Channel.CHL_S2C, PacketFlags flags = PacketFlags.Reliable)
        {
            foreach (var player in Players)
            {
                player.Client.Send(message, channel, flags);
            }
        }
        public void Start()
        {
           

            Spawn();

            StartCallback();
            this.Started = true;
        }
        public void StartCallback()
        {
            Stopwatch = Stopwatch.StartNew();
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            long deltaTime = Stopwatch.ElapsedMilliseconds;
            GameTime += deltaTime;
            NextSyncTime += deltaTime;
            Update(deltaTime);
            Stopwatch = Stopwatch.StartNew();
        }
        private void Update(long deltaTime)
        {
            if (NextSyncTime >= 10 * 1000)
            {
                Send(new GameTimerMessage(0, GameTime / 1000f));
                NextSyncTime = 0;
            }

            BlueTeam.Update(deltaTime);
            PurpleTeam.Update(deltaTime);
            Map.Update(deltaTime);
        }
        public void Announce(AnnounceEnum announce)
        {
            Send(new AnnounceMessage(0, (int)Map.Id, announce));
        }
        public void UnitAnnounce(UnitAnnounceEnum announce, int netId, int sourceNetId = 0, int[] assitsNetId = null)
        {
            Send(new UnitAnnounceMessage(netId, announce, sourceNetId, assitsNetId));
        }
        private void Spawn()
        {
            foreach (AIHero player in Players)
            {
                Map.AddUnit(player);
            }

            Map.Script.Spawn();

        
            Send(new StartSpawnMessage());

            foreach (var player in Map.Units.OfType<AIHero>())
            {
                Send(new HeroSpawnMessage(player.NetId, player.PlayerNo, player.Data.TeamId, player.SkinId, player.Data.Name,
                    player.Model));

                player.UpdateInfos();
                player.UpdateStats(false);
                player.UpdateHeath();
            }




            foreach (var turret in Map.Units.OfType<AITurret>())
            {
                Send(new TurretSpawnMessage(0, turret.NetId, turret.GetClientName()));
                turret.UpdateHeath();
            }

            BlueTeam.InitializeFog();
            PurpleTeam.InitializeFog();

            Send(new EndSpawnMessage());
        }


    }
}