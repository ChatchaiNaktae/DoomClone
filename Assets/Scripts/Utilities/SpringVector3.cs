using UnityEngine;

public class SpringVector3
{
    public Vector3 Current;
    public Vector3 Target;
    public Vector3 Velocity;
    
    public float SmoothTime;
    public float MaxSpeed;

    // Constructor to initialize the spring
    public SpringVector3(Vector3 initial, float smoothTime, float maxSpeed = Mathf.Infinity)
    {
        Current = initial;
        Target = initial;
        Velocity = Vector3.zero;
        SmoothTime = smoothTime;
        MaxSpeed = maxSpeed;
    }

    // Updates the spring. Must be called in Update() or LateUpdate()
    public Vector3 Update(float deltaTime)
    {
        // Unity has a built-in SmoothDamp function that acts exactly like Roblox's TweenService:SmoothDamp
        Current = Vector3.SmoothDamp(Current, Target, ref Velocity, SmoothTime, MaxSpeed, deltaTime);
        return Current;
    }

    // Adds a sudden force to the spring (Great for Recoil)
    public void Impulse(Vector3 velocity)
    {
        Velocity += velocity;
    }

    // Resets the spring to a specific value immediately
    public void Reset(Vector3 value)
    {
        Current = value;
        Target = value;
        Velocity = Vector3.zero;
    }
}