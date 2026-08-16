using UnityEngine;

public static class GameConfig
{
    // Server Configuration
    public static int maxPlayers = 4;
    public static bool friendlyFire = false;
    public static float monsterDamageMultiplier = 1.0f;
    public static float monsterHealthMultiplier = 1.0f;
    public static string serverName = "Doom Squad Room";
    
    public static void ResetToDefault()
    {
        maxPlayers = 4;
        friendlyFire = false;
        monsterDamageMultiplier = 1.0f;
        monsterHealthMultiplier = 1.0f;
        serverName = "Doom Squad Room";
    }
}