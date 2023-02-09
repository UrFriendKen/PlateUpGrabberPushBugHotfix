using HarmonyLib;
using Kitchen;
using KitchenLib;
using KitchenMods;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using System;
using Unity.Entities;

// Namespace should have "Kitchen" in the beginning
namespace KitchenGrabberPushBugHotfix
{
    public class Main : BaseMod
    {
        // guid must be unique and is recommended to be in reverse domain name notation
        // mod name that is displayed to the player and listed in the mods menu
        // mod version must follow semver e.g. "1.2.3"
        public const string MOD_GUID = "IcedMilo.PlateUp.GrabberPushBugHotfix";
        public const string MOD_NAME = "Grabber Push Bug Hotfix";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "IcedMilo";
        public const string MOD_GAMEVERSION = ">=1.1.1";
        // Game version this mod is designed for in semver
        // e.g. ">=1.1.1" current and all future
        // e.g. ">=1.1.1 <=1.2.3" for all from/until

        private static bool isInit = false;
        private static EntityManager entityManager;

        public Main() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void Initialise()
        {
            base.Initialise();
            // For log file output so the official plateup support staff can identify if/which a mod is being used
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!"); 

        }

        protected override void OnUpdate()
        {
            if (!isInit)
            {
                if (EntityManager != null)
                {
                    entityManager = EntityManager;
                    isInit = true;
                }
            }
        }

        public static bool HasHeldItem(CItemHolder held)
        {
            return isInit ? entityManager.HasComponent<CItem>(held.HeldItem) : true;
        }

        #region Logging
        // You can remove this, I just prefer a more standardized logging
        public static void LogInfo(string _log) { Debug.Log($"{MOD_NAME} " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"{MOD_NAME} " + _log); }
        public static void LogError(string _log) { Debug.LogError($"{MOD_NAME} " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }

    [HarmonyPatch]
    public static class ManageApplianceGhostsOriginalLambdaBodyPatch
    {
        public static MethodBase TargetMethod()
        {
            Type type = AccessTools.FirstInner(typeof(PushItems), t => t.Name.Contains("c__DisplayClass_OnUpdate_LambdaJob0"));
            return AccessTools.FirstMethod(type, method => method.Name.Contains("OriginalLambdaBody"));
        }

        public static bool Prefix(ref CConveyCooldown cooldown, ref CConveyPushItems push, ref CItemHolder held)
        {
            if (cooldown.Remaining > 0f || !Main.HasHeldItem(held))
            {
                push.State = CConveyPushItems.ConveyState.None;
                push.Progress = 0f;
                return false;
            }
            return true;
        }
    }
}
