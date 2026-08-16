using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
    /// <summary>
    /// Maps a value from one range [inMin, inMax] to another range [outMin, outMax].
    /// </summary>
    public static float Remap(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
    }
    
    /// <summary>
    /// Maps a value from one range [inMin, inMax] to another range [outMin, outMax].
    /// </summary>
    public static float CalculateLinearFalloff(Vector3 center, Vector3 target, float maxRadius)
    {
        float distance = Vector3.Distance(center, target);
        return Mathf.Clamp01(1f - (distance / maxRadius));
    }
    
    /// <summary>
    /// Converts a signed angle (-180 to 180 degrees) into an 8-way sprite rotation index (0 to 7).
    /// </summary>
    public static int Get8WayAngleIndex(float angle)
    {
        // Front
        if (angle > -22.5f && angle < 22.6f)
            return 0;
        // Front-Right / Diagonal
        if (angle >= 22.5f && angle < 67.5f)
            return 7;
        // Right
        if (angle >= 67.5f && angle < 112.5f)
            return 6;
        // Back-Right
        if (angle >= 112.5f && angle < 157.5f)
            return 5;
        
        // Back
        if (angle <= -157.5f || angle >= 157.5f)
            return 4;
        // Back-Left
        if (angle >= -157.4f && angle < -112.5f)
            return 3;
        // Left
        if (angle >= -112.5f && angle < -67.5f) 
            return 2;
        // Front-Left
        if (angle >= -67.5f && angle <= -22.5f)
            return 1;
        
        return 0;
    }
}