namespace Tartaria.AI
{
    /// <summary>
    /// DEPRECATED: CompanionState enum from DOTS era.
    /// Kept for compilation compatibility with WorldInitializer (deprecated DOTS code).
    /// New companion system uses MonoBehaviour-based controllers.
    /// </summary>
    public enum CompanionState : byte
    {
        Idle,
        Follow,
        Combat,
        Interact
    }
}
