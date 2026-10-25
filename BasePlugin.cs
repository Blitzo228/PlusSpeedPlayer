using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace PlusSpeedPlayer
{
    [BepInPlugin("blitzo.baldiremastered.plusspeedplayer", "Plus Speed Player", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> EnablePlusSpeed = null!;
        public static ConfigEntry<bool> EnableBonusStamina = null!;

        private void Awake()
        {
            EnablePlusSpeed = Config.Bind("General", "Enable Plus Speed", true, "If true, your speed will be like in Baldi's Basics Plus");
            EnableBonusStamina = Config.Bind("General", "Enable Bonus Stamina", true, "If true, gives stamina when you answering bonus question");

            new Harmony("blitzo.baldiremastered.plusspeedplayer").PatchAll();
        }
    }

    [HarmonyPatch(typeof(PlayerMovement), "Start")]
    public class PlayerMovementPatch
    {
        static void Postfix(PlayerMovement __instance)
        {
            if (!Plugin.EnablePlusSpeed.Value) return;
            __instance.walkSpeed = 16f;
            __instance.runSpeed = 24f;
        }
    }

    [HarmonyPatch(typeof(ClassicNullManager), "BeginPlay")]
    public class ClassicNullManagerBeginPlayPatch
    {
        static void Postfix()
        {
            if (!Plugin.EnablePlusSpeed.Value) return;
            var pm = Singleton<CoreGameManager>.Instance.GetPlayer(0);
            if (pm != null && pm.plm != null)
            {
                pm.plm.runSpeed = 24f;
                pm.plm.walkSpeed = 16f;
            }
        }
    }

    [HarmonyPatch(typeof(ClassicNullManager), "ElevatorClosed")]
    public class ClassicNullManagerElevatorClosedPatch
    {
        static void Postfix(ClassicNullManager __instance)
        {
            if (!Plugin.EnablePlusSpeed.Value) return;

            var elevatorsClosed = Traverse.Create(__instance).Field("elevatorsClosed").GetValue<int>();
            if (elevatorsClosed >= 4)
            {
                var pm = Singleton<CoreGameManager>.Instance.GetPlayer(0);
                if (pm != null && pm.plm != null)
                {
                    pm.plm.runSpeed = 16f;
                    pm.plm.walkSpeed = 10f;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Activity), "Completed", new System.Type[] { typeof(int), typeof(bool), typeof(Activity) })]
    public class BonusMathPatch
    {
        static void Prefix(Activity __instance, int player, bool correct, Activity activity)
        {
            if (!Plugin.EnableBonusStamina.Value) return;

            if (activity is MathMachine && correct && activity.InBonusMode)
            {
                var p = Singleton<CoreGameManager>.Instance.GetPlayer(player);
                if (p != null && p.plm != null)
                {
                    p.plm.stamina = p.plm.staminaMax;
                }
            }
        }
    }
}