using System;
using Exiled.API.Features;

namespace ZombieAttackInfection.com.github.sekasin.zombieattackinfection
{
    public class ZombieAttackInfection: Plugin<Config>
    {
        public override string Name => "ZombieAttackInfection";
        public override string Author => "Ugi0 & TenDRILLL";
        public override Version Version => new Version(2, 0, 5);
        public EventHandler eventHandler;

        public override void OnEnabled() {
            Log.Info("ZombieAttackInfection loading...");
            if (!Config.IsEnabled) {
                Log.Warn("ZombieAttackInfection disabled from config, unloading...");
                OnDisabled();
                return;
            }
            eventHandler = new EventHandler(this);
            Log.Info("ZombieAttackInfection loaded...");
        }

        public override void OnDisabled()
        {
            eventHandler.UnregisterEvents();
            Log.Info("ZombieAttackInfection unloaded.");
        }
    }
}