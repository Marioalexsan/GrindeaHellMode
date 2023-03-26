using Grindless;
using HarmonyLib;
using Marioalexsan.GrindeaQoL;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marioalexsan.GrindeaHellMode.Patches
{
    [HarmonyPatch]
    internal static class DefenseBreakPatches
    {
        // Absurd values for lols
        private const int TimeBeforeDecayInFrames = 360;
        private const int TimeBetweenDecaysInFrames = 60;

        // The buff timer is actually used to track how many frames elapsed since the buff was placed.
        // The actual condition for buff to expire is for the break percentage to go to 0

        private static void UpdateDefenseBreak(PlayerEntity target, float percentage, int customDecayTime = TimeBeforeDecayInFrames, bool keepExisting = false)
        {
            if (!HellModeMod.HellModeEnabled)
                return;

            if (NetUtils.IsClient)
                return;

            customDecayTime = Math.Min(TimeBeforeDecayInFrames, customDecayTime);

            float existingBuffValue = GetDefenseBreakPercentage(target);
            int existingBuffTime = GetDefenseBreakCounter(target) > 0 ? int.MaxValue - GetDefenseBreakCounter(target) : 0;

            int newBuffTime = Math.Max(existingBuffTime, int.MaxValue - (TimeBeforeDecayInFrames - customDecayTime));

            if (keepExisting && GetDefenseBreakCounter(target) > 0)
                newBuffTime = existingBuffTime;

            float newBuffValue = Math.Max(0, Math.Min(100, existingBuffValue + percentage));

            target.xBaseStats.RemoveStatusEffect(HellModeMod.DefenseBreak.GameID);

            if (newBuffValue > 0)
                target.xBaseStats.AddStatusEffect(HellModeMod.DefenseBreak.GameID, new BaseStats.EBuffFloat(newBuffTime, newBuffValue, EquipmentInfo.StatEnum.PLACEBO, false));
        }

        private static float GetDefenseBreakPercentage(PlayerEntity target)
        {
            if (target.xBaseStats.denxActiveEffects.TryGetValue(HellModeMod.DefenseBreak.GameID, out var existingBuff))
                return existingBuff.fCurrentEffect;

            return 0;
        }

        private static int GetDefenseBreakCounter(PlayerEntity target)
        {
            if (target.xBaseStats.denxActiveEffects.TryGetValue(HellModeMod.DefenseBreak.GameID, out var existingBuff))
                return int.MaxValue - existingBuff.GetLongestDuration();

            return 0;
        }

        [HarmonyPatch(typeof(BaseStats), nameof(BaseStats.iDefense), MethodType.Getter)]
        [HarmonyPostfix]
        public static void ApplyDefenseBreakEffect(ref int __result, BaseStats __instance)
        {
            if (!HellModeMod.HellModeEnabled)
                return;

            if (!(__instance.xOwner is PlayerEntity))
                return;

            if (!__instance.denxActiveEffects.TryGetValue(HellModeMod.DefenseBreak.GameID, out var buff))
                return;

            float amount = Math.Max(0f, Math.Min(100f, buff.fCurrentEffect));

            if (__result > 0)
            {
                // Reduce positive defense by value
                __result = (int)(__result * (1 - amount / 100f));
            }
            else
            {
                // Increase negative defense by half of the value
                __result = (int)(__result * (1 + amount / 100f / 2f));
            }
        }

        [HarmonyPatch(typeof(HudRenderComponent), nameof(HudRenderComponent.RenderTopGUI))]
        [HarmonyPostfix]
        private static void RenderDefenseBreakAmountInHUD()
        {
            if (!HellModeMod.HellModeEnabled)
                return;

            var existingBuffValue = GetDefenseBreakPercentage(Globals.Game.xLocalPlayer.xEntity);
            var appliedRecentlyFactor = Math.Min(1f, 1f - GetDefenseBreakCounter(Globals.Game.xLocalPlayer.xEntity) / (float) TimeBeforeDecayInFrames);
            var decayedRecentlyFactor = Math.Min(1f, 1f - GetDefenseBreakCounter(Globals.Game.xLocalPlayer.xEntity) % TimeBetweenDecaysInFrames / (float)TimeBetweenDecaysInFrames);

            var textColor = Color.Lerp(Color.White, Color.Red, appliedRecentlyFactor);

            if (appliedRecentlyFactor <= 0f)
                textColor = Color.Lerp(Color.White, Color.LightGray, decayedRecentlyFactor);

            var text = $"DEF Break: {(int)existingBuffValue}%";

            if (existingBuffValue == 0)
            {
                textColor = Color.White;
                text = $"DEF Break: None";
            }

            Globals.Game._RenderMaster_RenderTextWithOutline(
                FontManager.Reg7,
                text, 
                new Vector2(90, 70), 
                Vector2.Zero,
                1f,
                textColor,
                Color.Black);
        }

        [HarmonyPatch(typeof(Game1), nameof(Game1._Player_TakeDamage))]
        [HarmonyPostfix]
        public static void AddDefenseBreakOnHit(PlayerView xView, byte byType)
        {
            const float HighBreak = 35f;
            const float MediumBreak = 20f;
            const float LowBreak = 10f;

            const int LowDecay = TimeBeforeDecayInFrames / 2;

            float breakAmount = byType switch
            {
                DamageTypes.Type0_HealthDamage_Stun_TriggerInvuln => HighBreak,
                DamageTypes.Type7_HealthDamage_Stun => HighBreak,
                DamageTypes.Type8_HealthDamage_ShortStun => MediumBreak,
                DamageTypes.Type9_HealthDamage_ShortStun_TriggerInvuln => MediumBreak,
                DamageTypes.Type103_HealthDamage => MediumBreak,
                DamageTypes.Type3_HealthDamage => MediumBreak,
                DamageTypes.Type102_BlockedByDodge => MediumBreak,
                DamageTypes.Type1_ShieldDamage => MediumBreak,
                DamageTypes.Type2_ShieldDamage_PerfectGuard => LowBreak,
                DamageTypes.Type101_BreakShield => HighBreak,
                _ => 0f
            };

            int decayTime = byType switch
            {
                DamageTypes.Type103_HealthDamage => LowDecay,
                DamageTypes.Type3_HealthDamage => LowDecay,
                DamageTypes.Type102_BlockedByDodge => LowDecay,
                DamageTypes.Type1_ShieldDamage => LowDecay,
                DamageTypes.Type2_ShieldDamage_PerfectGuard => 0,
                _ => TimeBeforeDecayInFrames
            };

            bool keepExisting = byType == DamageTypes.Type2_ShieldDamage_PerfectGuard;

            if (breakAmount > 0f)
            {
                UpdateDefenseBreak(xView.xEntity, breakAmount, customDecayTime: decayTime, keepExisting: keepExisting);
            }
        }

        [HarmonyPatch(typeof(PlayerEntity), nameof(PlayerEntity.Update))]
        [HarmonyPostfix]
        private static void ApplyDefenseBreakDecay(PlayerEntity __instance)
        {
            int buffExistFrames = GetDefenseBreakCounter(__instance);

            if (buffExistFrames >= TimeBeforeDecayInFrames)
            {
                // Start decaying. Decay applies every second
                if (buffExistFrames % TimeBetweenDecaysInFrames == 0)
                {
                    UpdateDefenseBreak(__instance, -10, keepExisting: true);
                }
            }
        }

        [HarmonyPatch(typeof(Game1), nameof(Game1._Player_PerformBlink))]
        [HarmonyPostfix]
        private static void ApplyDefenseBreakOnBlink(PlayerView xOwnerView)
        {
            UpdateDefenseBreak(xOwnerView.xEntity, 20, customDecayTime: TimeBeforeDecayInFrames / 2);
        }
    }
}
