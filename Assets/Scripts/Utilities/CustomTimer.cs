using UnityEngine;
using System; // Required for Action

public class CustomTimer
{
    public float Interval { get; private set; }
    
    // Action acts exactly like Roblox's RBXScriptSignal. You can subscribe using +=
    public Action OnTick; 
    
    private float nextTick;
    private bool isRunning;

    public CustomTimer(float interval)
    {
        Interval = Mathf.Max(0f, interval);
    }

    public void Start()
    {
        if (isRunning) return;
        
        nextTick = Time.time + Interval;
        isRunning = true;
    }

    public void StartNow()
    {
        if (isRunning) return;
        
        // Invoke fires the signal immediately
        OnTick?.Invoke(); 
        Start();
    }

    public void Stop()
    {
        isRunning = false;
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    // Unlike Roblox's RunService, C# custom classes need to be manually ticked inside a MonoBehaviour's Update()
    public void Update()
    {
        if (!isRunning) return;

        if (Time.time >= nextTick)
        {
            nextTick = Time.time + Interval;
            OnTick?.Invoke();
        }
    }
}