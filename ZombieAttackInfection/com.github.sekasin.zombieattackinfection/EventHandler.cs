using System;
using Exiled.API.Features;
using Exiled.API.Enums;
using Handlers = Exiled.Events.Handlers;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using CommandSystem.Commands.RemoteAdmin.Tickets;
using MEC;
using Random = System.Random;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using PlayerRoles;
using UnityEngine;

namespace ZombieAttackInfection.com.github.sekasin.zombieattackinfection;

public class EventHandler {
    private readonly Plugin<Config> _main;
    private readonly bool _debugMode;
    private Timer timer;
    private readonly List<RoleTypeId> ImmuneRoles;
    private readonly double InfectionChance;
    private readonly double CureChance;

    private readonly float TickRate;
    private readonly int TickDamage;
    private readonly int InfectionTotalDamage;
    private Random random = new Random();

    private bool running;

    private List<Player> infectedPlayers = new List<Player>();
    private Dictionary<string, int> infectionDamage = new Dictionary<string, int>();

    public EventHandler(Plugin<Config> plugin)
    {
        _main = plugin;
        _debugMode = plugin.Config.Debug;

        ImmuneRoles = plugin.Config.ImmuneRoles;
        InfectionChance = plugin.Config.InfectionChance;
        CureChance = plugin.Config.CureChance;

        TickDamage = plugin.Config.TickDamage;
        TickRate = plugin.Config.TickRate;
        InfectionTotalDamage = plugin.Config.InfectionTotalDamage;

        Handlers.Player.Hurting += Hurting;
        Handlers.Player.UsedItem += AttemptToHeal;
        Handlers.Player.Dying += DieWithInfection;
        
        Handlers.Server.RoundStarted += StartInfectionMainLoop;
        Handlers.Server.RestartingRound += StopInfectionMainLoop;
        Handlers.Server.RoundEnded += StopInfectionMainLoop;
    }

    public void UnregisterEvents() {
        Handlers.Player.Hurting -= Hurting;
        Handlers.Player.UsedItem -= AttemptToHeal;
        Handlers.Player.Dying -= DieWithInfection;
        
        Handlers.Server.RoundStarted -= StartInfectionMainLoop;
        Handlers.Server.RestartingRound -= StopInfectionMainLoop;
        Handlers.Server.RoundEnded -= StopInfectionMainLoop;
    }
    private void EventLoop(object sender, ElapsedEventArgs e) {
        foreach (Player player in infectedPlayers) {
            if (!player.IsAlive || player.Role.Team == Team.SCPs || ImmuneRoles.Contains(player.Role)) {
                removeFromInfected(player);
                break;
            }
            infectionDamage[player.UserId] += TickDamage;
            //This is disabled due to the thing not fucking working on the server.
            //Can be re-enabled once it works.
            Newtonsoft.Json.JsonConvert.SerializeObject(player);
            player.Hurt(TickDamage, DamageType.Poison);
            if (_debugMode)
            {
                Log.Info("Tickings " + infectionDamage[player.UserId]);
            }
            if (infectionDamage[player.UserId] >= InfectionTotalDamage)
            {
                removeFromInfected(player);
            }
        }
    }

    private void removeFromInfected(Player player)
    {
        if (infectedPlayers.Contains(player)) infectedPlayers.Remove(player);
        if (infectionDamage.ContainsKey(player.UserId)) infectionDamage.Remove(player.UserId);
    }

    private void Hurting(HurtingEventArgs args) {
        if (ImmuneRoles.Contains(args.Player.Role)) return;
        if (args.DamageHandler.Type == DamageType.Poison) return;
        if (args.Attacker == null) return;
        if (args.Attacker.Role == RoleTypeId.Scp0492)
        {
            if (_debugMode)
            {
                Log.Info("SCP-049-2 hit someone");
            }
            if (args.Player.Role.Team != Team.SCPs)
            {
                if (infectedPlayers.Contains(args.Player)) return;
                if (IsInChance(InfectionChance))
                {
                    Infect(args.Player);
                }
            }
        }
    }

    private bool IsInChance(double chance) {
        random ??= new Random();
        if (random.Next(100) < chance)
        {
            return true;
        }
        return false;
    }

    private void AttemptToHeal(UsedItemEventArgs args) {
        if (infectedPlayers.Contains(args.Player)) {
            if (args.Item.Type == ItemType.SCP500) {
                CureInfection(args.Player);
            }
            else if (args.Item.Type == ItemType.Medkit) {
                if (IsInChance(CureChance))
                {
                    CureInfection(args.Player);
                }
            }
        }
    }

    private void CureInfection(Player player) {
        if (_debugMode) {
            Log.Info(player.Nickname+" has cured the infection.");
        }
        removeFromInfected(player);
    }

    private void Infect(Player player) {
        if (_debugMode) {
            Log.Info($"Infected "+player.Nickname);
        }
        infectedPlayers.Add(player);
        infectionDamage.Add(player.UserId, 0);
    }

    private void DieWithInfection(DyingEventArgs args) {
        if (infectedPlayers.Contains(args.Player))
        {
            args.IsAllowed = false;
            if (_debugMode) {
                Log.Info(args.Player.Nickname+" is being turned into SCP-049-2");
            }
            Vector3? position = null;
            if (args.Player.IsInPocketDimension) {
                foreach (Player player in Player.List) {
                    if (player.Role == RoleTypeId.Scp049) {
                        position = player.Position;
                        break;
                    }
                }
                if (position == null) {
                    foreach (Player player in Player.List) {
                        if (player.Role == RoleTypeId.Scp0492) {
                            position = player.Position;
                            break;
                        }
                    }
                }
            }
            else {
                position = args.Player.Position;
            }
            
            if (position == null) {
                return;
            }

            args.Player.RoleManager.ServerSetRole(RoleTypeId.Scp0492, RoleChangeReason.Revived);
            args.Player.Teleport(args.Player.Position);
            removeFromInfected(args.Player);
        }
    }

    private void StartInfectionMainLoop() {
        infectedPlayers.Clear();
        timer = new Timer(TickRate * 1000);
        timer.Elapsed += EventLoop;
        timer.Start();
    }

    private void StopInfectionMainLoop()
    {
        timer.Close();
        infectedPlayers.Clear();
        infectionDamage.Clear();
    }

    private void StopInfectionMainLoop(RoundEndedEventArgs args)
    {
        StopInfectionMainLoop();
    }
}
