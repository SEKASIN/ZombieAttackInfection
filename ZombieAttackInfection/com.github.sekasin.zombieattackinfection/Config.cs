using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;
using PlayerRoles;

namespace ZombieAttackInfection.com.github.sekasin.zombieattackinfection
{
    public class Config : IConfig
    {
        [Description("Is the Plugin enabled.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Debug mode.")]
        public bool Debug { get; set; } = false;
        
        [Description("Roles which wont get Infected.")]
        public List<RoleTypeId> ImmuneRoles { get; set; } = new List<RoleTypeId>() { RoleTypeId.Tutorial };

        [Description("What is the chance a infection occurs.")]
        public double InfectionChance { get; set; } = 100;

        [Description("What is the chance that a medkit heals the infection.")]
        public double CureChance { get; set; } = 50;

        [Description("How often should the damage tick. Time in seconds.")]
        public float TickRate { get; set; } = 1;

        [Description("How much damage should the infection do every tick.")]
        public int TickDamage { get; set; } = 1;
        
        [Description("How much damage should the infection do in total.")]
        public int InfectionTotalDamage { get; set; } = 60;
    }
}