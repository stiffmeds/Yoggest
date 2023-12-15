using BepInEx;
using HarmonyLib;
// #TODO: post-Essentials update using static Obeliskial_Essentials.Essentials;
using BepInEx.Logging;
using UnityEngine;
using BepInEx.Configuration;

namespace Yoggest
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    // #TODO: post-Essentials update [BepInDependency("com.stiffmeds.obeliskialessentials")]
    [BepInProcess("AcrossTheObelisk.exe")]
    public class Yoggest : BaseUnityPlugin
    {
        public static ConfigEntry<float> medsYoggestIncreasePerVitality { get; private set; }
        public static ConfigEntry<bool> medsYoggestEveryone { get; private set; }
        public static ConfigEntry<bool> medsYoggestEveryoneIncludingThem { get; private set; }
        internal const int ModDate = 20231215;
        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);
        internal static ManualLogSource Yog;
        private void Awake()
        {
            Yog = Logger;
            medsYoggestIncreasePerVitality = Config.Bind(new ConfigDefinition("Settings", "Increase per Vitality stack"), 1f, new ConfigDescription("X% increase in Yogger's model size for each Vitality stack."));
            medsYoggestEveryone = Config.Bind(new ConfigDefinition("Settings", "Not Just Yogger"), false, new ConfigDescription("Let heroes other than Yogger scale with Vitality stacks."));
            medsYoggestEveryoneIncludingThem = Config.Bind(new ConfigDefinition("Settings", "Not Just You"), false, new ConfigDescription("Let NPCs scale with Vitality stacks."));
            Yog.LogInfo($"{PluginInfo.PLUGIN_GUID} {PluginInfo.PLUGIN_VERSION} is loaded!");
            harmony.PatchAll();
        }
    }
    [HarmonyPatch]
    internal class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "GetMaxHP")]
        public static void GetMaxHPPostfix(ref Character __instance)
        {
            if (__instance != null && __instance.HeroData != null && __instance.HeroData.HeroSubClass != null && __instance.Alive && __instance.HeroItem != null && __instance.HeroItem.animatedTransform != null)
            { // it's a hero...
                string subclassID = __instance.HeroData.HeroSubClass.Id;
                /* #TODO: post-Essentials update
                if (subclassID == "medsdlctwo")
                    subclassID = medsCloneTwo;
                else if (subclassID == "medsdlcthree")
                    subclassID = medsCloneThree;
                else if (subclassID == "medsdlcfour")
                    subclassID = medsCloneFour;*/
                if (subclassID == "bandit" || Yoggest.medsYoggestEveryone.Value)
                { // it's yogger (soon to be yoggest)!
                    float sizeIncrease = 1f + ((float)__instance.GetAuraCharges("vitality") * 0.01f * Yoggest.medsYoggestIncreasePerVitality.Value);
                    __instance.HeroItem.animatedTransform.localScale = new Vector3(sizeIncrease, sizeIncrease, __instance.HeroItem.animatedTransform.localScale.z);
                    Yoggest.Yog.LogDebug("Setting " + subclassID + " size to: " + sizeIncrease.ToString());
                }
            }
            else if (Yoggest.medsYoggestEveryoneIncludingThem.Value && __instance != null && __instance.NpcData != null && __instance.Alive && __instance.NPCItem != null && __instance.NPCItem.animatedTransform != null)
            {
                float sizeIncrease = 1f + ((float)__instance.GetAuraCharges("vitality") * 0.01f * Yoggest.medsYoggestIncreasePerVitality.Value);
                __instance.NPCItem.animatedTransform.localScale = new Vector3(sizeIncrease, sizeIncrease, __instance.NPCItem.animatedTransform.localScale.z);
                Yoggest.Yog.LogDebug("Setting " + __instance.NpcData.Id + " size to: " + sizeIncrease.ToString());
            }
        }
    }
}
