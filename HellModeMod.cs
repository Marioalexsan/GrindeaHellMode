using Behaviours;
using Grindless;
using HarmonyLib;
using Marioalexsan.GrindeaHellMode;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static SoG.Inventory;
using static SoG.Inventory.PreviewPair;
using static SoG.SpellVariable;

namespace Marioalexsan.GrindeaQoL
{
    public class HellModeMod : Mod
    {
        internal static StatusEffectEntry DefenseBreak => Instance.GetStatusEffect("DefenseBreak");

        public override string Name => "Marioalexsan.GrindeaHellMode";
        public override Version Version => new Version(1, 0, 0);

        public static HellModeMod Instance { get; private set; }

        public static bool HellModeEnabled => true;

        private static readonly SpellVariableEditor _editor = new SpellVariableEditor();

        public HellModeMod()
        {
            HellModeResources.ReloadResources();
        }

        public override void Load()
        {
            Instance = this;

            var defenseBreak = CreateStatusEffect("DefenseBreak");
        }

        public override void Unload()
        {
            Instance = null;
        }
    }
}
