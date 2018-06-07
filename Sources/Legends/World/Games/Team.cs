﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Legends.World.Entities;
using Legends.Core.Protocol.Enum;
using Legends.Core.Protocol;
using ENet;
using Legends.World.Entities.AI;
using Legends.Core.Protocol.Game;
using System.Numerics;
using Legends.Core.DesignPattern;
using Legends.Core.Protocol.Messages.Game;
using Legends.World.Games.Maps.Fog;
using Legends.Network;

namespace Legends.World.Games
{
    public class Team : IUpdatable
    {
        public TeamId Id
        {
            get;
            private set;
        }
        public Dictionary<int, Unit> Units
        {
            get;
            set;
        }
        public AttackableUnit[] AliveUnits
        {
            get
            {
                return Array.FindAll(Units.Values.OfType<AttackableUnit>().ToArray(), x => x.Alive);
            }
        }
        public int Size
        {
            get
            {
                return Units.Count;
            }
        }
        /// <summary>
        /// Dictionary (VisibleUnit,VisibleByUnit)
        /// </summary>
        private Dictionary<Unit, Unit> VisibleUnits
        {
            get;
            set;
        }
        private Game Game
        {
            get;
            set;
        }
        private List<FogUpdate> FogUpdates
        {
            get;
            set;
        }
        public Team(Game game, TeamId id)
        {
            this.Id = id;
            this.Units = new Dictionary<int, Unit>();
            this.VisibleUnits = new Dictionary<Unit, Unit>();
            this.Game = game;
            this.FogUpdates = new List<FogUpdate>();
        }
        public void Send(Message message, Channel channel = Channel.CHL_S2C, PacketFlags flags = PacketFlags.Reliable)
        {
            foreach (var player in Units.Values.OfType<AIHero>())
            {
                player.Client.Send(message, channel, flags);
            }
        }
        public void AddUnit(Unit unit)
        {
            unit.TeamNo = Size + 1;
            Units.Add(unit.TeamNo, unit);
        }

        public void RemoveUnit(Unit unit)
        {
            Units.Remove(unit.TeamNo);
        }
        public Team GetOposedTeam()
        {
            return Id == TeamId.BLUE ? Game.PurpleTeam : Game.BlueTeam;
        }
        public bool HasVision(Unit player)
        {
            return VisibleUnits.ContainsKey(player);
        }
        public Unit[] GetVisibleUnits()
        {
            return VisibleUnits.Keys.ToArray();
        }
        [InDeveloppement(InDeveloppementState.TEMPORARY)]
        private void OnTeamEnterVision(Unit unit)
        {
            if (unit.IsMoving)
            {
                AIUnit attackableUnit = (AIUnit)unit;
                Send(new EnterVisionMessage(false, unit.NetId, unit.Position, attackableUnit.Path.WaypointsIndex, attackableUnit.Path.GetWaypoints(), Game.Map.Record.MiddleOfMap));
            }
            else
            {
                Send(new EnterVisionMessage(false, unit.NetId, unit.Position, 1, new Vector2[] { unit.Position, unit.Position }, Game.Map.Record.MiddleOfMap));
            }
        }
        private void OnTeamLeaveVision(Unit unit)
        {
            Send(new LeaveVisionMessage(unit.NetId));
        }
        public void InitializeFog()
        {
            foreach (var unit in Units.Values.OfType<AITurret>())
            {
                AddFogUpdate(new FogUpdate(Game.NetIdProvider.PopNextNetId(), Id, unit));
            }
        }
        private void AddFogUpdate(FogUpdate fogUpdate)
        {
            this.Send(new FogUpdate2Message(Id, fogUpdate.Source.NetId,
                fogUpdate.Source.Position, fogUpdate.NetId, fogUpdate.Source.PerceptionBubbleRadius));

            FogUpdates.Add(fogUpdate);
        }
        public void Update(long deltaTime)
        {
            foreach (var opponent in GetOposedTeam().AliveUnits)
            {
                bool visible = false;

                foreach (var unit in AliveUnits)
                {
                    if (unit.PerceptionBubbleRadius > 0)
                    {
                        if (unit.InFieldOfView(opponent))
                        {
                            visible = true;
                            if (!VisibleUnits.ContainsKey(opponent))
                            {
                                OnTeamEnterVision(opponent);
                                unit.OnUnitEnterVision(opponent);
                                VisibleUnits.Add(opponent, unit);
                            }
                        }

                    }

                }

                if (visible == false)
                {
                    if (VisibleUnits.ContainsKey(opponent))
                    {
                        var visibleBy = VisibleUnits[opponent];

                        VisibleUnits.Remove(opponent);

                        OnTeamLeaveVision(opponent);

                        visibleBy.OnUnitLeaveVision(opponent);
                    }
                }
            }
        }

    }
}
