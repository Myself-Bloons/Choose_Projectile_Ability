using System.Collections;
using MelonLoader;
using HarmonyLib;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using UnityEngine;
using Il2CppTMPro;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Weapons;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;

[assembly: MelonInfo(typeof(Replace_Projectile_Ability.Main), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace Replace_Projectile_Ability
{
    public class Main : BloonsTD6Mod
    {
        public static readonly ModSettingBool ModEnabled = new(false)
        {
            displayName = "Ability Projectile Replacement Enabled",
            button = true,
            enabledText = "ON",
            disabledText = "OFF"
        };

        public static readonly ModSettingString AbilitySourceId = new("TackShooter-050")
        {
            displayName = "Ability Source Tower Path",
            description = "Type an exact, case-sensitive Tower Path (Like: TackShooter-050)"
        };

        public static readonly ModSettingHotkey ToggleKey = new(KeyCode.F9)
        {
            displayName = "Toggle Replacement ON/OFF",
            description = "Enable/Disable during the game (Careful, affects NEW towers)"
        };

        public static readonly ModSettingHotkey ChooseAbilityKey = new(KeyCode.F8)
        {
            displayName = "Choose Ability Source (Popup)",
            description = "Open popup to type an exact Tower Path (case-sensitive)"
        };

        private const string InjectedAbilityTag = "Every Projectile Tower Ability Replace";

        public override void OnGameModelLoaded(GameModel model)
        {
            MelonLogger.Msg("Tower Paths and Ability Names:");
            foreach (var tower in model.towers)
                foreach (var ability in tower.GetBehaviors<AbilityModel>())
                    MelonLogger.Msg($"{tower.name} -> {ability.displayName}");
        }

        private static void OpenTowerIdPopup(string title, string body, string initial, System.Action<string> onConfirm)
        {
            PopupScreen.instance.ShowSetNamePopup(title, body, onConfirm, initial);
            MelonCoroutines.Start(RelaxTMPForActivePopup());
        }

        private static IEnumerator RelaxTMPForActivePopup()
        {
            for (int i = 0; i < 60; i++)
            {
                var popup = PopupScreen.instance?.GetFirstActivePopup();
                if (popup != null)
                {
                    var field = popup.GetComponentInChildren<TMP_InputField>(true);
                    if (field != null)
                    {
                        field.contentType = TMP_InputField.ContentType.Standard;
                        field.inputType = TMP_InputField.InputType.Standard;
                        field.characterValidation = TMP_InputField.CharacterValidation.None;
                        field.lineType = TMP_InputField.LineType.SingleLine;
                        field.keyboardType = TouchScreenKeyboardType.Default;
                        field.characterLimit = 128;

                        field.Select();
                        field.ActivateInputField();
                    }
                }
                yield return null;
            }
        }

        private static void OnConfirmTowerId(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            var gm = InGame.instance?.GetGameModel() ?? Game.instance?.model;
            if (gm == null) return;

            TowerModel? found = null;
            try { found = gm.GetTowerFromId(text); } catch { }
            var hasAbility = found?.GetBehavior<AbilityModel>() != null;

            if (found == null || !hasAbility)
            {
                System.Action<string> retry = OnConfirmTowerId;
                OpenTowerIdPopup(
                    "Not Found (Case-Sensitive)",
                    "Tower id not found OR has no Ability.\n" +
                    "IDs are case-sensitive (e.g., TackShooter-050). Try again:",
                    ((string)AbilitySourceId ?? string.Empty).Trim(),
                    retry
                );
                return;
            }

            AbilitySourceId.SetValueAndSave(text);
            MelonLogger.Msg($"[Replace_Projectile_Ability] Ability source set to: {(string)AbilitySourceId}");
        }

        private static void ApplyAbilityReplacement(TowerModel towerModel, GameModel? gameModel)
        {
            if (gameModel == null) return;

            var id = ((string)AbilitySourceId ?? string.Empty).Trim();
            if (id.Length == 0) return;

            TowerModel? template = null;
            try { template = gameModel.GetTowerFromId(id); } catch { }
            var templateAbility = template?.GetBehavior<AbilityModel>();
            if (templateAbility == null) return;

            var lname = towerModel.name.ToLowerInvariant();

            var currAbs = towerModel.GetBehaviors<AbilityModel>();
            if (currAbs != null)
            {
                foreach (var ab in currAbs)
                {
                    if (ab.name == InjectedAbilityTag)
                        return;
                }
            }

            var attacks = towerModel.GetAttackModels();
            if (attacks == null || attacks.Count == 0) return;

            var ability = templateAbility.Duplicate();
            ability.name = InjectedAbilityTag;

            var activate = ability.GetBehavior<ActivateAttackModel>();
            if (activate == null || activate.attacks == null || activate.attacks.Length == 0)
                return;

            bool modified = false;

            foreach (var attack in attacks)
            {
                foreach (var proj in attack.GetDescendants<ProjectileModel>().ToIl2CppList())
                {
                    if (proj.HasBehavior<TravelStraitModel>() || lname.Contains("boomer"))
                    {
                        var weapon = activate.attacks[0].weapons[0].Duplicate();
                        weapon.projectile = proj.Duplicate();

                        if (!modified)
                        {
                            activate.attacks[0].weapons =
                                new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<WeaponModel>(0);
                            modified = true;
                        }

                        activate.attacks[0].AddWeapon(weapon);
                    }
                }
            }

            if (modified)
                towerModel.AddBehavior(ability);
        }

        public override void OnTowerCreated(Tower tower, Entity target, Model modelToUse)
        {
            if (!ModEnabled) return;

            var gm = InGame.instance?.GetGameModel() ?? Game.instance?.model;
            var modified = tower.towerModel.Duplicate();
            ApplyAbilityReplacement(modified, gm);
            tower.UpdateRootModel(modified);
        }

        public override void OnTowerUpgraded(Tower tower, string upgradeName, TowerModel newBaseTowerModel)
        {
            if (!ModEnabled) return;

            var gm = InGame.instance?.GetGameModel() ?? Game.instance?.model;
            var modified = tower.towerModel.Duplicate();

            var existingAbs = modified.GetBehaviors<AbilityModel>();
            if (existingAbs != null)
            {
                AbilityModel? toRemove = null;
                foreach (var ab in existingAbs)
                {
                    if (ab.name == InjectedAbilityTag)
                    {
                        toRemove = ab;
                        break;
                    }
                }
                if (toRemove != null)
                    modified.RemoveBehavior(toRemove);
            }

            ApplyAbilityReplacement(modified, gm);
            tower.UpdateRootModel(modified);
        }

        public override void OnUpdate()
        {
            if (ToggleKey.JustPressed())
            {
                ModEnabled.SetValueAndSave(!ModEnabled);
                MelonLogger.Msg($"[Replace_Projectile_Ability] {(ModEnabled ? "ENABLED" : "DISABLED")}");
            }

            if (ChooseAbilityKey.JustPressed())
            {
                var current = ((string)AbilitySourceId ?? string.Empty).Trim();
                System.Action<string> confirm = OnConfirmTowerId;

                OpenTowerIdPopup(
                    "Ability Source (Case-Sensitive)",
                    "Enter exact Tower Path (Like: TackShooter-050):",
                    current,
                    confirm
                );
            }
        }
    }
}
