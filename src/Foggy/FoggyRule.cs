namespace Foggy
{
    /// <summary>
    /// Timer-agnostic core of the "Foggy" tag rule: with the tag enabled in the
    /// timer's settings, every level the run enters is played at maximum fog
    /// density, and the game's default ','/'.' fog keys are disabled for as
    /// long as the override is in scope (level play, the loading phases between
    /// levels, and retry reloads). The rule raises no validity of its own — it
    /// changes the level instead of judging it; the timer's engine invokes it
    /// only while the tag is enabled.
    /// </summary>
    internal static class FoggyRuleCore
    {
        /// <summary>The stable tag id users toggle in the settings panel.</summary>
        public const string TagId = "Foggy";

        // Engage at level enter. Re-pin every tick: CaveRender.OnPreCull
        // re-derives RenderSettings.fogDensity from the multiplier every frame,
        // and the pause menu resets the multiplier to 1. No restore on level
        // exit — the load into the run's next level (or a retry's reload) is
        // still inside the override's scope; leaving the run for a menu/lobby
        // is handled by FogOverride.Poll in the plugin's Update.
        public static void OnLevelEnter() => FogOverride.Activate();

        public static void OnTick() => FogOverride.Enforce();

        public static void OnLevelExit()
        {
        }
    }

    /// <summary>The "Foggy" rule as registered with HSRTimer.</summary>
    public sealed class HsrFoggyRule : HSRTimer.ITagRule
    {
        public string Id => FoggyRuleCore.TagId;

        // No localization key: the settings panel falls back to the raw id.
        public string DisplayNameKey => null;

        public void OnLevelEnter(HSRTimer.ValidationContext ctx) => FoggyRuleCore.OnLevelEnter();

        public void OnTick(HSRTimer.ValidationContext ctx) => FoggyRuleCore.OnTick();

        public void OnLevelExit(HSRTimer.ValidationContext ctx) => FoggyRuleCore.OnLevelExit();
    }

    /// <summary>
    /// The "Foggy" rule as registered with TwilightTimer (HSRTimer's fork;
    /// same extension surface under a different GUID/namespace). Both rule
    /// shells share one tag id, so users — and TwilightTimer's match-mode tag
    /// push — enable it identically with either fork installed.
    /// </summary>
    public sealed class TwilightFoggyRule : TwilightTimer.ITagRule
    {
        public string Id => FoggyRuleCore.TagId;

        public string DisplayNameKey => null;

        public void OnLevelEnter(TwilightTimer.ValidationContext ctx) => FoggyRuleCore.OnLevelEnter();

        public void OnTick(TwilightTimer.ValidationContext ctx) => FoggyRuleCore.OnTick();

        public void OnLevelExit(TwilightTimer.ValidationContext ctx) => FoggyRuleCore.OnLevelExit();
    }
}
