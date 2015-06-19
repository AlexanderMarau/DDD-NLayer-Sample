﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLayer.Application.Modules
{
    public enum NLayerModulesType
    {
        UserSystem = 1,
        BootstrapSamples = 2,
        AuthMenuSample = 3,
    }

    public class NLayerModules
    {
        public NLayerModules(NLayerModulesType type, string name)
        {
            Type = type;
            Name = name;
        }

        public NLayerModulesType Type { get; set; }

        public string Name { get; set; }
    }
}
