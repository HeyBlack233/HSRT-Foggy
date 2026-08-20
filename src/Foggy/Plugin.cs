using BepInEx;
using BepInEx.Logging;

namespace Foggy
{
    /// <summary>
    /// BepInEx plugin entry point. Detects which timer plugin — HSRTimer or
    /// its fork TwilightTimer — is installed, registers the Foggy tag rule
    /// with it once its registry is up, and applies the Harmony hooks that pin
    /// the fog multiplier (disabling the game's ','/'.' fog keys while the rule
    /// is in scope).
    /// </summary>
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("HSRTimer", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("TwilightTimer", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        private void Awake()
        {
            Logger = base.Logger;

            // Both timer dependencies are soft: the plugin loads with
            // whichever fork is installed (and stays inert if neither is —
            // bridges are only created for installed GUIDs).
            TimerIntegration.DetectTimers();

            // The fog pinning itself is timer-independent; it only ever
            // engages after a timer engine invoked the rule.
            try
            {
                var harmony = new HarmonyLib.Harmony(PluginInfo.PLUGIN_GUID);
                harmony.PatchAll(typeof(Plugin).Assembly);
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Foggy: failed to apply Harmony patches: {ex}");
            }

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
        }

        // Registration is deferred until each installed timer's registry is up
        // (soft dependencies impose no load order); afterwards keep watching
        // for the override leaving its scope (run exited to a menu/lobby, tag
        // unchecked) — no rule callback covers those edges.
        private void Update()
        {
            TimerIntegration.Poll();
            FogOverride.Poll();
        }
    }
}
