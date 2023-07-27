﻿using System;
using Exiled.API.Features;
using Exiled.API.Enums;
using Handlers = Exiled.Events.Handlers;
using System.Collections.Generic;
using System.Linq;
using MEC;
using Random = System.Random;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using PlayerRoles;
using UnityEngine;

namespace ZombieAttackInfection.com.github.sekasin.zombieattackinfection;

public class EventHandler
{
    private readonly bool _debugMode;
    private readonly List<RoleTypeId> ImmuneRoles;
    private readonly double InfectionChance;
    private readonly double CureChance;
    private CoroutineHandle timer;
    private readonly float TickRate;
    private readonly int TickDamage;
    private readonly int InfectionTotalDamage;
    private Random random = new ();

    private bool running;

    private List<string> infectedPlayers = new ();
    private Dictionary<string, int> infectionDamage = new ();

public EventHandler(Plugin<Config> plugin)
{
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
    
    private IEnumerator<float> EventLoop() {
        for (;;) {
            try {
                foreach (string playerID in infectedPlayers.ToList())
                {
                    Dictionary<string, int> checkInfectionDamage = new Dictionary<string, int>(infectionDamage);
                    Player player = Player.Get(playerID);
                    if (player == null) { removeFromInfected(playerID, "playerNull"); }
                    else {
                        if (!player.IsAlive || player.Role.Team == Team.SCPs || ImmuneRoles.Contains(player.Role)) {
                            Log.Debug("No longer inf");
                            removeFromInfected(playerID, "noINF");
                        } else {
                            if (checkInfectionDamage.ContainsKey(playerID)) {
                                infectionDamage[playerID] += TickDamage;
                            }
                            player.Hurt(TickDamage, DamageType.Poison);
                            player.ShowHint(new Hint("\nYou got <color=\"green\">INFECTED ("+ (InfectionTotalDamage - checkInfectionDamage[playerID]-1)*TickRate +"s) </color> by <color=\"red\">SCP-049-2</color>.\nSeek immidiate medical attention.", 1f, true));
                            if (checkInfectionDamage.ContainsKey(playerID) && _debugMode) {
                                Log.Debug("DMGTick " + checkInfectionDamage[playerID]);  
                            }
                            if (checkInfectionDamage.ContainsKey(playerID) && checkInfectionDamage[playerID] >= InfectionTotalDamage) {
                                removeFromInfected(playerID,"dmgExceeds");
                            }
                        }
                    }
                }
            } catch (Exception e) {
                Log.Error(e);
            }
            yield return Timing.WaitForSeconds(TickRate);
        }
    }

    private void removeFromInfected(string player, string source)
    {
        if (_debugMode)
        {
            Log.Debug("Calling Remove from " + source);
        }
        if (infectedPlayers.Contains(player)) infectedPlayers.Remove(player);
        if (infectionDamage.ContainsKey(player)) infectionDamage.Remove(player);
    }

    private void Hurting(HurtingEventArgs args) {
        if (ImmuneRoles.Contains(args.Player.Role)) return;
        if (args.DamageHandler.Type == DamageType.Poison) return;
        if (args.Attacker == null) return;
        if (args.Attacker.Role == RoleTypeId.Scp0492)
        {
            if (_debugMode)
            {
                Log.Debug("SCP-049-2 hit someone");
            }
            if (args.Player.Role.Team != Team.SCPs)
            {
                if (infectedPlayers.Contains(args.Player.UserId)) return;
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
        if (infectedPlayers.Contains(args.Player.UserId)) {
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
        Log.Info(player.Nickname+" has cured the infection.");
        player.ShowHint(new Hint("\nYou got <color=\"yellow\">CURED</color> from the <color=\"green\">INFECTION</color>.", 3f, true));
        removeFromInfected(player.UserId,"cured");
    }

    private void Infect(Player player) {
        Log.Info("Infected "+player.Nickname);
        infectedPlayers.Add(player.UserId);
        infectionDamage.Add(player.UserId, 0);
    }

    private void DieWithInfection(DyingEventArgs args) {
        if (infectedPlayers.Contains(args.Player.UserId))
        {
            args.IsAllowed = false;
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

            args.Player.Role.Set(RoleTypeId.Scp0492);
            args.Player.Teleport(args.Player.Position);
            Log.Info(args.Player.Nickname+" became SCP-049-2");
            removeFromInfected(args.Player.UserId,"died");
        }
    }

    private void StartInfectionMainLoop() {
        infectedPlayers.Clear();
        infectionDamage.Clear();
        timer = Timing.RunCoroutine(EventLoop());
    }

    private void StopInfectionMainLoop()
    {
        Timing.KillCoroutines(timer);
        infectedPlayers.Clear();
        infectionDamage.Clear();
    }

    private void StopInfectionMainLoop(RoundEndedEventArgs args)
    {
        StopInfectionMainLoop();
    }
}