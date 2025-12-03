using UnityEngine;

public static class PlayerDeathTracker
{
    // Total times the player has died since the game started
    public static int DeathCount { get; private set; } = 0;

    public static void RegisterDeath()
    {
        DeathCount++;
        // Optional debug:
        // Debug.Log($"PlayerDeathTracker: DeathCount = {DeathCount}");
    }

    // Optional helper, if you ever want to reset from somewhere
    public static void ResetDeaths()
    {
        DeathCount = 0;
    }
}
