﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Legends.World.Entities;
using Legends.Core.Protocol.Enum;
using Legends.World.Entities.AI;

namespace Legends.World.Champions
{
    [Champion(ChampionEnum.Ezreal)]
    public class Ezreal : Champion
    {
        public Ezreal(AIHero player) : base(player)
        {

        }

        public override void ApplyE()
        {
            throw new NotImplementedException();
        }

        public override void ApplyQ()
        {
            throw new NotImplementedException();
        }

        public override void ApplyR()
        {
            throw new NotImplementedException();
        }

        public override void ApplyW()
        {
            throw new NotImplementedException();
        }
    }
}