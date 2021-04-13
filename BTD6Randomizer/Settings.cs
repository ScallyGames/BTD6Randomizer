using NKHook6.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6Randomizer
{
    public class Settings : ModSettings
    {
        public int NumberOfRandomTowers { get; set; } = 2;
        public bool RerollAfterBuild { get; set; } = true;
        public bool RerollAfterWave { get; set; } = false;
    }
}
