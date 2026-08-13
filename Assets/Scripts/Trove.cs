using System;
using System.Collections.Generic;
using UnityEngine;

public class Trove
{
    private class TrackedObject
    {
        public object Target;
        public Action CleanupAction;
    }

    private List<TrackedObject> _objects = new List<TrackedObject>();
    private bool _cleaning = false;

    // We use a single Add method to mimic Lua's dynamic typing behavior!
    public T Add<T>(T obj)
    {
        if (_cleaning) throw new Exception("Cannot call Trove.Add() while cleaning");

        Action cleanupAction = null;

        // Check the type of the object and assign the correct cleanup method automatically
        if (obj is Action action)
        {
            cleanupAction = action; // If it's a function
        }
        else if (obj is UnityEngine.Object unityObj)
        {
            // If it's a Unity Object (GameObject, Component)
            cleanupAction = () => { if (unityObj != null) UnityEngine.Object.Destroy(unityObj); };
        }
        else if (obj is IDisposable disposable)
        {
            // If it's a C# disposable object (like our CustomTimer)
            cleanupAction = () => disposable?.Dispose();
        }
        else
        {
            Debug.LogWarning($"Trove: Added object of type {typeof(T)} but don't know how to clean it up automatically.");
        }

        _objects.Add(new TrackedObject 
        { 
            Target = obj, 
            CleanupAction = cleanupAction 
        });

        return obj;
    }

    // Allows adding an object with a custom cleanup function (like the Lua version)
    public T Add<T>(T obj, Action customCleanup)
    {
        if (_cleaning) throw new Exception("Cannot call Trove.Add() while cleaning");

        _objects.Add(new TrackedObject 
        { 
            Target = obj, 
            CleanupAction = customCleanup 
        });

        return obj;
    }

    public bool Remove(object obj)
    {
        if (_cleaning) throw new Exception("Cannot call Trove.Remove() while cleaning");
        return FindAndRemoveFromObjects(obj, true);
    }

    public bool Pop(object obj)
    {
        if (_cleaning) throw new Exception("Cannot call Trove.Pop() while cleaning");
        return FindAndRemoveFromObjects(obj, false);
    }

    public void Clean()
    {
        if (_cleaning) return;
        
        _cleaning = true;

        // Loop backwards so removing items during iteration doesn't break the loop
        for (int i = _objects.Count - 1; i >= 0; i--)
        {
            try
            {
                _objects[i].CleanupAction?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error cleaning up object in Trove: {e.Message}");
            }
        }

        _objects.Clear();
        _cleaning = false;
    }

    public void Destroy()
    {
        Clean();
    }

    private bool FindAndRemoveFromObjects(object obj, bool cleanup)
    {
        for (int i = 0; i < _objects.Count; i++)
        {
            if (_objects[i].Target == obj)
            {
                var cleanupAction = _objects[i].CleanupAction;
                _objects.RemoveAt(i);
                
                if (cleanup)
                {
                    cleanupAction?.Invoke();
                }
                
                return true;
            }
        }
        return false;
    }
}