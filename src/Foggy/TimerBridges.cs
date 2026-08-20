using System.Collections.Generic;
using BepInEx.Bootstrap;

namespace Foggy
{
    /// <summary>
    /// Connection to one timer plugin — HSRTimer or its fork TwilightTimer.
    /// The two forks expose identical extension surfaces under different
    /// GUIDs/namespaces, so each gets a thin bridge. A bridge's members that
    /// touch timer types are only ever invoked after the chainloader confirmed
    /// that timer is installed (see <see cref="TimerIntegration"/>), so a
    /// missing fork's assembly never breaks JIT of the rest of this plugin.
    /// </summary>
    internal interface ITimerBridge
    {
        /// <summary>The timer plugin's BepInEx GUID.</summary>
        string TimerGuid { get; }

        /// <summary>True once the timer's Awake ran and its registry is usable.</summary>
        bool IsRegistryReady { get; }

        /// <summary>Register the Foggy rule; false on a duplicate id.</summary>
        bool TryRegisterRule();

        /// <summary>Live check of the timer's enabled-tag config.</summary>
        bool IsTagEnabled(string tagId);
    }

    /// <summary>
    /// Deferred registration and live tag queries against every installed
    /// timer. Both timer dependencies are soft (so this plugin loads with
    /// whichever fork — or neither — is present), and BepInEx only orders hard
    /// dependencies, so the timer's Awake may not have run yet when this
    /// plugin's does: registration retries each frame until the target's
    /// registry is up.
    /// </summary>
    internal static class TimerIntegration
    {
        private static readonly List<ITimerBridge> _pending = new List<ITimerBridge>();
        private static readonly List<ITimerBridge> _connected = new List<ITimerBridge>();

        /// <summary>
        /// Called once from the plugin's Awake: create a bridge per installed
        /// timer plugin. Only plugin GUIDs are inspected here — no timer types
        /// are touched, so this is safe with either or neither fork installed.
        /// </summary>
        public static void DetectTimers()
        {
            var guids = Chainloader.PluginInfos != null ? Chainloader.PluginInfos.Keys : null;
            if (guids == null)
                return;
            foreach (var guid in guids)
            {
                if (guid == "HSRTimer")
                    _pending.Add(new HsrTimerBridge());
                else if (guid == "TwilightTimer")
                    _pending.Add(new TwilightTimerBridge());
            }
        }

        /// <summary>Per frame: promote pending bridges whose registry is now up.</summary>
        public static void Poll()
        {
            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                var bridge = _pending[i];
                if (!bridge.IsRegistryReady)
                    continue;
                _pending.RemoveAt(i);
                if (bridge.TryRegisterRule())
                {
                    _connected.Add(bridge);
                    Plugin.Logger.LogInfo($"Foggy: registered the '{FoggyRuleCore.TagId}' rule with {bridge.TimerGuid}.");
                }
                else
                {
                    Plugin.Logger.LogWarning($"Foggy: {bridge.TimerGuid} rejected the rule registration (duplicate '{FoggyRuleCore.TagId}' id?).");
                }
            }
        }

        /// <summary>True while any connected timer has the tag enabled in its config.</summary>
        public static bool IsTagEnabled(string tagId)
        {
            for (int i = 0; i < _connected.Count; i++)
            {
                if (_connected[i].IsTagEnabled(tagId))
                    return true;
            }
            return false;
        }
    }

    /// <summary>Bridge to HSRTimer.</summary>
    internal sealed class HsrTimerBridge : ITimerBridge
    {
        public string TimerGuid => "HSRTimer";

        public bool IsRegistryReady => HSRTimer.TagRuleRegistry.Instance != null;

        public bool TryRegisterRule() => HSRTimer.TagRuleRegistry.Instance.Register(new HsrFoggyRule());

        public bool IsTagEnabled(string tagId)
        {
            var cfg = HSRTimer.ConfigService.Instance;
            return cfg != null && cfg.EnabledTags.HasTag(tagId);
        }
    }

    /// <summary>Bridge to TwilightTimer (HSRTimer's fork).</summary>
    internal sealed class TwilightTimerBridge : ITimerBridge
    {
        public string TimerGuid => "TwilightTimer";

        public bool IsRegistryReady => TwilightTimer.TagRuleRegistry.Instance != null;

        public bool TryRegisterRule() => TwilightTimer.TagRuleRegistry.Instance.Register(new TwilightFoggyRule());

        public bool IsTagEnabled(string tagId)
        {
            var cfg = TwilightTimer.ConfigService.Instance;
            return cfg != null && cfg.EnabledTags.HasTag(tagId);
        }
    }
}
