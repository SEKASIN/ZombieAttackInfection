# ZombieAttackInfection
EXILED plugin that causes SCP-049-2 to spread a disease.

## Installation
Download ZombieAttackInfection.dll from [Releases](/Releases).

Move ZombieAttackInfection.dll to .config/EXILED/Plugins and restart server.

## Configuration
Edit values in .config/EXILED/Configs/PORT-config.yml

Example config with default values:
```
zombie_attack_infection:
# Is the Plugin enabled.
  is_enabled: true
  # Debug mode.
  debug: false
  # Roles which wont get Infected.
  immune_roles:
  - Tutorial
  # What is the chance a infection occurs.
  infection_chance: 100
  # What is the chance that a medkit heals the infection.
  cure_chance: 50
  # How often should the damage tick. Time in seconds.
  tick_rate: 1
  # How much damage should the infection do every tick.
  tick_damage: 1
  # How much damage should the infection do in total.
  infection_total_damage: 50

```
* is_enabled
> A boolean; Controls if SCP_Kick is enabled or not.
* debug
> A boolean; Enables some extra logging.
* immune_roles
> A List; Which roles are not affected by the infection.
* infection_chance
> An int; How likely it is that a hit by SCP-049-2 causes an infection.
* cure_chance
> An int; How likely it is that a medkit cures the infection prematurely.
* tick_rate
> An int; How often the damage tick happens in seconds.
* tick_damage
> An int; How much damage is done everytime the damage tick occurs.
* infection_total_damage
> An int; How much damage is done in total. If tick_damage doesn't divide this, the total damage might be higher.
