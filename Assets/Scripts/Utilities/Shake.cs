using UnityEngine;

public class Shake
{
    public float Amplitude = 1f;
    public float Frequency = 1f;
    public float FadeInTime = 1f;
    public float FadeOutTime = 1f;
    public float SustainTime = 0f;
    public bool Sustain = false;
    
    public Vector3 PositionInfluence = Vector3.one;
    public Vector3 RotationInfluence = Vector3.one;

    private float timeOffset;
    private float startTime;
    private bool isRunning;

    public Shake()
    {
        // Random offset so multiple shakes don't look identical
        timeOffset = Random.Range(-10000f, 10000f);
    }

    public void Start()
    {
        startTime = Time.time;
        isRunning = true;
    }

    public void Stop()
    {
        isRunning = false;
    }

    public void StopSustain()
    {
        Sustain = false;
        SustainTime = (Time.time - startTime) - FadeInTime;
    }

    public bool IsShaking()
    {
        return isRunning;
    }

    // Updates and outputs the current shake vectors. Must be called in Update()
    public void Update(out Vector3 positionOffset, out Vector3 rotationOffset, out bool isDone)
    {
        isDone = false;
        
        if (!isRunning)
        {
            positionOffset = Vector3.zero;
            rotationOffset = Vector3.zero;
            isDone = true;
            return;
        }

        float now = Time.time;
        float dur = now - startTime;
        float noiseInput = (now + timeOffset) / Frequency;

        float multiplierFadeIn = 1f;
        float multiplierFadeOut = 1f;

        // Calculate Fade In
        if (dur < FadeInTime)
        {
            multiplierFadeIn = dur / FadeInTime;
        }

        // Calculate Fade Out
        if (!Sustain && dur > FadeInTime + SustainTime)
        {
            if (FadeOutTime <= 0f)
            {
                isDone = true;
            }
            else
            {
                multiplierFadeOut = 1f - (dur - FadeInTime - SustainTime) / FadeOutTime;
                if (dur >= FadeInTime + SustainTime + FadeOutTime)
                {
                    isDone = true;
                }
            }
        }

        // Helper function to map Unity's PerlinNoise (0 to 1) to Roblox's math.noise (-0.5 to 0.5)
        float GetNoise(float x, float y)
        {
            return (Mathf.PerlinNoise(x, y) * 2f - 1f) / 2f;
        }

        // Generate the base offset using 3D noise approximations
        Vector3 offset = new Vector3(
            GetNoise(noiseInput, 0f),
            GetNoise(0f, noiseInput),
            GetNoise(noiseInput, noiseInput)
        ) * Amplitude * Mathf.Min(multiplierFadeIn, multiplierFadeOut);

        if (isDone)
        {
            Stop();
        }

        // Apply influences
        positionOffset = Vector3.Scale(PositionInfluence, offset);
        rotationOffset = Vector3.Scale(RotationInfluence, offset);
    }
}