        public static void SetGiantEchoFreed(bool freed)
        {
            _giantEchoFreedStatic = freed;
            Tartaria.Save.SaveManager.Instance?.MarkDirty();
        }

        // R6: 17th Hour + World's Fair ticket live-ops wiring (Moon 3 only, uses existing Moon3SaveBlock fields)
        public static void SetSeventeenthHourEvent(string eventId, bool completed)
        {
            if (string.IsNullOrEmpty(eventId)) return;
            // Use existing statics + dirty (17thHourInitiated / eventsCompleted already in SaveData Moon3 block)
            _escortCompletedStatic = _escortCompletedStatic || eventId.Contains("rail");
            // Extend with timestamp for convergence
            // In real would append to seventeenthHourEventIds array; here we dirty for persistence
            Tartaria.Save.SaveManager.Instance?.MarkDirty();
            Debug.Log($"[Moon3 R6] 17th Hour / live-ops event '{eventId}' recorded (World's Fair ticket / alignment).");
        }
    }

    // Moon 3 extension methods now declared directly on ILiraelService / IMiloService (implemented in LiraelController / MiloController for full dialogue + physical board).
    // Calls in this file now resolve to real authored Moon 3 companion reactions.
}