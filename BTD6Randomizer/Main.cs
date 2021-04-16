using MelonLoader;
using Harmony;
using System;
using System.Linq;
using UnhollowerBaseLib;
using Assets.Scripts.Simulation.Input;
using Assets.Scripts.Models.TowerSets;
using Assets.Scripts.Unity.UI_New.InGame.RightMenu;
using Assets.Scripts.Unity.UI_New.InGame.StoreMenu;
using Assets.Scripts.Simulation.Towers;
using Assets.Scripts.Simulation.Objects;
using Assets.Scripts.Models;
using Assets.Scripts.Simulation;

namespace BTD6Randomizer
{
    public class Main : MelonMod
    {
        static System.Random random = new System.Random();
        private static Settings settings = null;
        private static string[] towerNames = new string[] { };
        private static Il2CppArrayBase<TowerPurchaseButton> towerButtons;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            HarmonyInstance.Create("ScallyGames.BTD6Randomizer").PatchAll();

            settings = new Settings();

            MelonLogger.Msg("BTD6Randomizer has finished loading");
        }

        [HarmonyPatch(typeof(Simulation), "OnRoundEnd")]
        class SimulationRoundEnd_Patch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (settings.RerollAfterWave)
                {
                    RerollTowers();
                }
            }
        }

        [HarmonyPatch(typeof(Tower), "Initialise")]
        class TowerInitialise_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(ref Tower __instance, ref Entity target, ref Model modelToUse)
            {
                if (!towerNames.Contains(__instance.towerModel.name)) return; // ignore summoned towers   

                if (settings.RerollAfterBuild)
                {
                    RerollTowers();
                }
            }
        }

        public static void RerollTowers()
        {
            if(Main.towerButtons == null)
            {
                Main.towerButtons = ShopMenu.instance.towerButtons.GetComponentsInChildren<Assets.Scripts.Unity.UI_New.InGame.StoreMenu.TowerPurchaseButton>();
            }

            Il2CppStringArray enabledTowers = new Il2CppStringArray(towerNames.OrderBy(x => random.Next()).Take(Math.Max(Math.Min(settings.NumberOfRandomTowers, towerNames.Length), 0)).ToArray());
            foreach (var purchaseButton in Main.towerButtons)
            {
                if(enabledTowers.Contains(purchaseButton.baseTowerModel.name))
                {
                    purchaseButton.transform.parent.gameObject.SetActive(true);
                }
                else
                {
                    purchaseButton.transform.parent.gameObject.SetActive(false);
                }
            }
        }

        [HarmonyPatch(typeof(TowerInventory), "Init")]
        public class TowerInit_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref Il2CppSystem.Collections.Generic.List<TowerDetailsModel> allTowersInTheGame)
            {
                Main.towerButtons = null;

                Main.towerNames = allTowersInTheGame.ToArray().Where(x => x.name.Contains("ShopTowerDetailsModel")).Select(x => x.name.Replace("ShopTowerDetailsModel_", "")).ToArray();

                return true;
            }
        }
    }
}
