using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace FullSpeedNotifications
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(ConfigurationManager.ConfigurationManager.GUID, BepInDependency.DependencyFlags.HardDependency)]
    public class Patches : BaseUnityPlugin
    {
        static ConfigEntry<bool> bDisableFullscreenNotifications;
        static ConfigEntry<bool> bDisableFullscreenNotificationsHighSpeed;
        static ConfigEntry<bool> bDisableNotificationsCompletely;
        static ConfigEntry<bool> bDisableRoadMissingNotification;

        public ConfigDefinition bFullscreenNotificationsdef = new ConfigDefinition(PluginInfo.PLUGIN_GUID, "Disable fullscreen notifications");
        public ConfigDefinition bDisableFullscreenOnHighSpeeddef = new ConfigDefinition(PluginInfo.PLUGIN_GUID, "Disable fullscreen notifications when speed is 5 or higher");
        public ConfigDefinition bDisableNotificationsCompletelydef = new ConfigDefinition(PluginInfo.PLUGIN_GUID, "Disable notifications completely");
        public ConfigDefinition bDisableRoadMissingNotificationdef = new ConfigDefinition(PluginInfo.PLUGIN_GUID, "Disable No Road Access notifications");

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            MethodInfo original = AccessTools.Method(typeof(NotificationsUI), "ShowAndRegisterNotification");
            MethodInfo patch = AccessTools.Method(typeof(Patches), "ShowAndRegisterNotification_Patch");
            harmony.Patch(original, new HarmonyMethod(patch));
        }

        public Patches()
        {
            bDisableFullscreenNotifications = Config.Bind(bFullscreenNotificationsdef, false, new ConfigDescription("Disable fullscreen notifications"));
            bDisableFullscreenNotificationsHighSpeed = Config.Bind(bDisableFullscreenOnHighSpeeddef, false, new ConfigDescription("Disable fullscreen notifications when speed is 5 or higher"));
            bDisableNotificationsCompletely = Config.Bind(bDisableNotificationsCompletelydef, false, new ConfigDescription("Disable notifications completely"));
            bDisableRoadMissingNotification = Config.Bind(bDisableRoadMissingNotificationdef, false, new ConfigDescription("Disable No Road Access notifications"));
        }

        private static bool ShowAndRegisterNotification_Patch(NotificationContext context, Stack<NotificationContext> ____toDisplayFullscreen, Stack<NotificationContext> ____toDisplay)
        {
            Stack<NotificationContext> stack;
            TimeManager timemanager = TimeManager.Instance;

            if (bDisableNotificationsCompletely.Value && !(context.Type == NotificationType.Invasion))
                return false;

            if (timemanager != null && timemanager.SpeedFactor > 4f && bDisableFullscreenNotificationsHighSpeed.Value && !(context.Type == NotificationType.Invasion))
                context.ShowFullscreen = false;

            if (bDisableFullscreenNotifications.Value && !(context.Type == NotificationType.Invasion))
                context.ShowFullscreen = false;

            if (bDisableRoadMissingNotification.Value && context.SourceType == SourceType.RoadBlocked)
                return false;

            if (context.AsPopup || context.ShowFullscreen)
            {
                stack = ____toDisplayFullscreen;
            }
            else
            {
                stack = ____toDisplay;
            }
            if (stack.Count == 0 || stack.Peek().HasDifferentInfos(context))
            {
                stack.Push(context);
            }
            return false;
        }

    }
}
