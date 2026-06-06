using System;

namespace Tartaria.Integration
{
    /// <summary>
    /// Princess Anastasia behavior modes
    /// </summary>
    public enum AnastasiaMode
    {
        Silent,
        ReactiveWhisper,
        Conversational,
        Invisible
    }

    /// <summary>
    /// Solidification effect phases
    /// </summary>
    public enum SolidificationPhase
    {
        NotTriggered,
        Glow,
        Shift,
        Solid,
        Return,
        Complete
    }
}
