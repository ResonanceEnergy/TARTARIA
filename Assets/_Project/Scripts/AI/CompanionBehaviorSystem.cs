namespace Tartaria.AI
{
    /// <summary>
    /// DEPRECATED: DOTS-era companion behavior system.
    /// Kept for compilation compatibility with CompanionManager (hybrid bridge code).
    /// New companion system uses MonoBehaviour controllers.
    /// </summary>
    public static class CompanionBehaviorSystem
    {
        public static void ApplyPhysicalTellForBeat(ref CompanionBehavior behavior, int beatType, int companionId)
        {
            // STUB: Apply physical animation tells based on narrative beat
            // TODO: Route to MonoBehaviour controller when hybrid bridge is removed
        }
    }
}
