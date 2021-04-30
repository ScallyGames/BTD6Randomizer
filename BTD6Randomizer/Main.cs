using MelonLoader;
using System;
using System.Linq;
using UnhollowerBaseLib;
using Assets.Scripts.Simulation.Input;
using Assets.Scripts.Unity.UI_New.InGame.RightMenu;
using Assets.Scripts.Unity.UI_New.InGame.StoreMenu;
using Assets.Scripts.Simulation.Towers;
using Assets.Scripts.Simulation.Objects;
using Assets.Scripts.Models;
using BloonsTD6_Mod_Helper;
using BloonsTD6_Mod_Helper.Extensions;
using Assets.Scripts.Unity.UI_New.InGame;
using UnityEngine;
using System.Text.RegularExpressions;

namespace BTD6Randomizer
{
    public class Main : BloonsTD6Mod
    {
        // TODO: Fix farm being rollable in CHIMPS
        // TODO: Include heroes in rolls with settings
        // TODO: Fix settings
        // TODO: Remove water towers in non-water maps

        static System.Random random = new System.Random();
        const string preferencesCategoryIdentifier = "BTD6Randomizer";
        private static string[] towerNames = new string[] { };
        private static string[] heroNames = new string[] { };
        private static Il2CppArrayBase<TowerPurchaseButton> towerButtons;
        private static TowerInventory currentInventory;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            MelonLogger.Msg("BTD6Randomizer mod loaded");

            MelonPreferences.CreateCategory(preferencesCategoryIdentifier, "BTD6 Randomizer Settings");

            MelonPreferences.CreateEntry(preferencesCategoryIdentifier, "NumberOfRandomTowers", 2, "Number of Random Towers");
            MelonPreferences.CreateEntry(preferencesCategoryIdentifier, "RerollAfterBuild", true, "Reroll After Build");
            MelonPreferences.CreateEntry(preferencesCategoryIdentifier, "RerollAfterWave", false, "Reroll After Wave");
            MelonPreferences.CreateEntry(preferencesCategoryIdentifier, "RerollOnStart", false, "Reroll On Start");
        }

        public override void OnUpdate()
        {
            if(MelonPreferences.GetEntryValue<bool>(preferencesCategoryIdentifier, "RerollOnStart"))
            {
                try
                {
                    if (towerButtons == null || towerButtons.Length == 0 || currentInventory != InGame.instance.GetTowerInventory())
                    {
                        RerollTowers();
                    }
                }
                catch (Exception) { }
            }
        }

        public override void OnKeyDown(KeyCode keyCode)
        {
            base.OnKeyDown(keyCode);

            if(keyCode == KeyCode.F9)
            {
                RerollTowers();
            }
        }

        public override void OnRoundEnd()
        {
            if (MelonPreferences.GetEntryValue<bool>(preferencesCategoryIdentifier, "RerollAfterWave"))
            {
                RerollTowers();
            }
        }

        public override void OnTowerCreated(Tower tower, Entity target, Model modelToUse)
        {
            if(towerNames.Length == 0)
            {
                SetTowerNames();
                heroNames = InGame.instance.GetGameModel().GetAllTowerDetails().ToArray().Where(x => x.name.Contains("HeroDetailsModel")).Select(x => x.name.Replace("HeroDetailsModel_", "")).ToArray();
            }

            base.OnTowerCreated(tower, target, modelToUse);

            if (!heroNames.Contains(tower.model.name) && !towerNames.Contains(tower.towerModel.name)) return; // ignore summoned towers   

            if (MelonPreferences.GetEntryValue<bool>(preferencesCategoryIdentifier, "RerollAfterBuild"))
            {
                RerollTowers();
            }
        }

        public static void RerollTowers()
        {
            if (towerNames.Length == 0)
            {
                SetTowerNames();
            }
            if (towerButtons == null || towerButtons.Length == 0 || currentInventory != InGame.instance.GetTowerInventory())
            {
                towerButtons = ShopMenu.instance.towerButtons.GetComponentsInChildren<TowerPurchaseButton>();
                currentInventory = InGame.instance.GetTowerInventory();
            }

            int numberOfRandomTowers = MelonPreferences.GetEntryValue<int>(preferencesCategoryIdentifier, "NumberOfRandomTowers");
            Il2CppStringArray enabledTowers = new Il2CppStringArray(towerNames.OrderBy(x => random.Next()).Take(Math.Max(Math.Min(numberOfRandomTowers, towerNames.Length), 0)).ToArray());
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

        public static void SetTowerNames()
        {
            towerNames = InGame.instance.GetGameModel().GetAllTowerDetails().ToArray().Where(x => x.name.Contains("ShopTowerDetailsModel")).Select(x => x.name.Replace("ShopTowerDetailsModel_", "")).ToArray();
            if (towerNames.Any(x => Regex.IsMatch(x, @"(\d)(\d)(\d)")))
            {
                towerNames = towerNames.Where(x => Regex.IsMatch(x, @"(\d)(\d)(\d)")).ToArray();
            }
        }
    }
}
