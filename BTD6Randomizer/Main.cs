using MelonLoader;
using Harmony;

using Assets.Scripts.Unity.UI_New.InGame;

using Assets.Scripts.Models.Towers;
using Assets.Scripts.Unity;
using Assets.Scripts.Utils;
using System;
using System.Text.RegularExpressions;
using System.IO;
using Assets.Main.Scenes;
using System.Linq;
using Assets.Scripts.Models.Towers.Behaviors.Attack;
using Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.Bloons.Behaviors;
using Assets.Scripts.Models.Towers.Projectiles.Behaviors;
using System.Collections.Generic;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Towers.Projectiles;
using Assets.Scripts.Models.Towers.Behaviors.Emissions;
using Assets.Scripts.Models.Towers.Behaviors.Abilities;
using Assets.Scripts.Simulation.Track;
using Assets.Scripts.Unity.Bridge;
using Assets.Scripts.Simulation.Towers;
using UnhollowerBaseLib;
using Assets.Scripts.Simulation.Input;
using Assets.Scripts.Models.TowerSets;
using Assets.Scripts.Models.Powers;
using Assets.Scripts.Models.Profile;
using NKHook6.Api.Events;
using NKHook6;
using Assets.Scripts.Models.Bloons;
using NKHook6.Api.Extensions;
using static NKHook6.Api.Events._Bloons.BloonEvents;
using Logger = NKHook6.Logger;
using NKHook6.Api;

namespace BTD6Randomizer
{
    public class Main : MelonMod
    {
        static System.Random random = new System.Random();
        private static TowerInventory inventory = null;
        private static Settings settings = null;
        private static string[] towerNames = new string[] {};

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            EventRegistry.instance.listen(typeof(Main));
            settings = new Settings();
            settings = settings.Load<Settings>();
            settings.Save(settings);
            Logger.Log("BTD6Randomizer has finished loading");
        }

        [EventAttribute("RoundStartEvent")]
        public static void RoundStart(NKHook6.Api.Events._Simulation.SimulationEvents.RoundStartEvent e)
        {
            Main.inventory = e.simulation.GetTowerInventory(-1);
        }

        [EventAttribute("RoundEndEvent")]
        public static void RoundEnd(NKHook6.Api.Events._Simulation.SimulationEvents.RoundEndEvent e)
        {
            if(settings.RerollAfterWave)
            {
                RerollTowers();
            }
        }

        [EventAttribute("TowerCreatedEvent")]
        public static void TowerBuilt(NKHook6.Api.Events._Towers.TowerEvents.CreatedEvent e)
        {
            if (!towerNames.Contains(e.model.name)) return; // ignore summoned towers   

            if(settings.RerollAfterBuild)
            {
                RerollTowers();
            }
        }

        public static void RerollTowers()
        {
            if (Main.inventory != null)
            {
                Il2CppStringArray towers = new Il2CppStringArray(towerNames.OrderBy(x => random.Next()).Skip(Math.Max(Math.Min(settings.NumberOfRandomTowers, towerNames.Length), 0)).ToArray());
                Main.inventory.towerDiscounts.Clear();
                Main.inventory.AddTowerDiscount("Restriction", towers, 99999, 0, -1000000);
            }
        }

        [HarmonyPatch(typeof(TowerInventory), "Init")]
        public class TowerInit_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref Il2CppSystem.Collections.Generic.List<TowerDetailsModel> allTowersInTheGame)
            {
                Main.towerNames = allTowersInTheGame.ToArray().Where(x => x.name.Contains("ShopTowerDetailsModel")).Select(x => x.name.Replace("ShopTowerDetailsModel_", "")).ToArray();

                return true;
            }
        }
    }
}
