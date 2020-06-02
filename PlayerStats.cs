using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyspace.Stamina
{
    class PlayerStats
    {
        public float Stamina { get; set; }

        public PlayerStats(float stamina)
        {
            this.Stamina = stamina;
        }

        public PlayerStats()
        {
            Stamina = 1.0f;
        }
    }
}
