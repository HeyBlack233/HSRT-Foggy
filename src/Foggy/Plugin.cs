using BepInEx;
using BepInEx.Logging;
using HSRTimer;

namespace Foggy
{
    /// <summary>
    /// BepInEx plugin entry point. Registers the Foggy tag rule with HSRTimer's
    /// rule registry and applies the Harmony hooks that pin the fog multiplier
    /// (disabling the game's ','/'.' fog keys while the rule is in scope).
    /// </summary>
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("HSRTimer")]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        private void Awake()
        {
            Logger = base.Logger;

            // The tag engine invokes a rule only while its tag is enabled in the
            // timer's settings panel (persisted to tags.ini) — that is the rule's
            // gate. A duplicate id (another Foggy already registered) is logged
            // and ignored by the registry; then there is nothing left to hook.
            if (!TagRuleRegistry.Instance.Register(new FoggyRule()))
                return;

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

        // Watch for the override leaving its scope (run exited to a menu/lobby,
        // tag unchecked) — no rule callback covers those edges.
        private void Update() => FogOverride.Poll();
    }
}
