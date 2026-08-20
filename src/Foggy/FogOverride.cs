using HarmonyLib;
using Multiplayer;

namespace Foggy
{
    /// <summary>
    /// Owns the fog-density override.
    ///
    /// The game has a single global fog dial: <c>CaveRender.fogDensityMultiplier</c>.
    /// <c>CaveRender.OnPreCull</c> re-derives <c>RenderSettings.fogDensity</c> from it
    /// on every rendered frame (multiplier × the level's own fog density), so the
    /// override never touches RenderSettings — it pins the multiplier. Two things
    /// fight back while a level is active: the ','/'.' fog keys in
    /// <c>FreeRoamCam.Update</c> (±10/s, clamped to [0, kFogMax]) and the pause
    /// menu, which resets the multiplier to 1 (<c>MenuCameraEffects.FadeInPauseMenu</c>).
    /// Both are neutralized by re-pinning the value every rule tick and every
    /// <c>FreeRoamCam.Update</c> (see <see cref="FreeRoamCamPatches"/>).
    /// </summary>
    internal static class FogOverride
    {
        /// <summary>The game's own fog-key clamp maximum (FreeRoamCam.kFogMax).</summary>
        public const float MaxMultiplier = 60f;

        /// <summary>The multiplier the game normally runs at.</summary>
        private const float DefaultMultiplier = 1f;

        /// <summary>True while the override is engaged (from level enter until the run scope ends).</summary>
        public static bool Active { get; private set; }

        /// <summary>Engage the override: pin the fog multiplier to its maximum.</summary>
        public static void Activate()
        {
            Active = true;
            CaveRender.fogDensityMultiplier = MaxMultiplier;
        }

        /// <summary>Re-pin the multiplier while engaged. Cheap enough to call every tick/frame.</summary>
        public static void Enforce()
        {
            if (Active)
                CaveRender.fogDensityMultiplier = MaxMultiplier;
        }

        /// <summary>
        /// Per-frame scope check (called from the plugin's Update). The override
        /// stays engaged through level-to-level loading and retry reloads — a retry
        /// stays inside <c>AppSate.LoadLevel</c> — and is restored as soon as the
        /// game leaves the load/play states for a menu/lobby, or the tag is
        /// unchecked in the timer's settings. The rule itself has no callback for
        /// those edges, so the plugin watches for them here.
        /// </summary>
        public static void Poll()
        {
            if (!Active)
                return;

            if (!TagStillEnabled() || !InRunScope())
                Deactivate();
        }

        private static bool TagStillEnabled()
        {
            var cfg = HSRTimer.ConfigService.Instance;
            return cfg != null && cfg.EnabledTags.HasTag(FoggyRule.TagId);
        }

        /// <summary>
        /// True while the app is loading or playing a level (single-player,
        /// hosting or client) — i.e. inside a run, including its loading phases.
        /// Menus, customization and multiplayer lobbies are out of scope.
        /// </summary>
        private static bool InRunScope()
        {
            switch (App.state)
            {
                case AppSate.LoadLevel:
                case AppSate.PlayLevel:
                case AppSate.ServerLoadLevel:
                case AppSate.ServerPlayLevel:
                case AppSate.ClientLoadLevel:
                case AppSate.ClientWaitServerLoad:
                case AppSate.ClientPlayLevel:
                    return true;
                default:
                    return false;
            }
        }

        private static void Deactivate()
        {
            Active = false;
            CaveRender.fogDensityMultiplier = DefaultMultiplier;
        }
    }

    /// <summary>
    /// While the override is engaged, re-pin the fog multiplier right after the
    /// game's own fog-key handling runs. This disables the ','/'.' keys'
    /// effect on the same frame, before anything renders — the held-key drift
    /// never becomes visible, and the pause-menu reset is reverted too.
    /// </summary>
    [HarmonyPatch(typeof(FreeRoamCam))]
    internal static class FreeRoamCamPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        private static void UpdatePostfix()
        {
            FogOverride.Enforce();
        }
    }
}
