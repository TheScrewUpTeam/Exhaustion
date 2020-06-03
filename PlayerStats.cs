using VRage.Game.ModAPI;

namespace Keyspace.Stamina
{
    class PlayerStats
    {
        public float Stamina { get; set; }

        public PlayerStats(float stamina)
        {
            Stamina = stamina;
        }

        public PlayerStats()
        {
            Stamina = 1.0f;
        }

        public void Update(IMyPlayer player)
        {
            // TODO: proper calc, this here a mock
            Stamina = player.Character.SuitEnergyLevel * 100.0f + player.Character.Integrity;
        }
    }
}
