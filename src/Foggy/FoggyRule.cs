using HSRTimer;

namespace Foggy
{
    /// <summary>
    /// The "Foggy" tag rule: with the tag enabled in the timer's settings, every
    /// level the run enters is played at maximum fog density, and the game's
    /// default ','/'.' fog keys are disabled for as long as the override is in
    /// scope (level play, the loading phases between levels, and retry reloads).
    /// The rule raises no validity of its own — it changes the level instead of
    /// judging it; the timer's engine invokes it only while the tag is enabled.
    /// </summary>
    public sealed class FoggyRule : ITagRule
    {
        /// <summary>The stable tag id users toggle in the settings panel.</summary>
        public const string TagId = "Foggy";

        public string Id => TagId;

        // No localization key: the settings panel falls back to the raw id.
        public string DisplayNameKey => null;

        public void OnLevelEnter(ValidationContext ctx) => FogOverride.Activate();

        // CaveRender.OnPreCull re-derives RenderSettings.fogDensity from the
        // multiplier every frame, and the pause menu resets the multiplier to 1 —
        // re-pin it so the fog stays at maximum for the whole level.
        public void OnTick(ValidationContext ctx) => FogOverride.Enforce();

        // Intentionally no restore here: the load into the run's next level (or a
        // retry's reload) is still inside the override's scope. Leaving the run
        // for a menu/lobby is handled by FogOverride.Poll in the plugin's Update.
        public void OnLevelExit(ValidationContext ctx)
        {
        }
    }
}
