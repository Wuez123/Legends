﻿using Legends.Core.IO.Inibin;
using Legends.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legends.Core.Attributes
{
    public class InibinFieldAttribute : Attribute
    {
        public InibinHashEnum hash;

        public InibinFieldAttribute(InibinHashEnum hash)
        {
            this.hash = hash;
        }
    }
    public class InibinFieldFileName : Attribute
    {

    }
}
