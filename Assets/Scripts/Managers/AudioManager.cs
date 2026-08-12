using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FootstepGroup
{
    public string tag;
    public AudioClip[] footstepClips;
}
public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public static AudioManager instance;
    
    [Header("Footstep Settings")]
    public FootstepGroup[] footstepGroups;
    
    private AudioSource footstepSource;
    
    void Awake() {
        if (instance == null){
            instance = this;
        }else {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
        
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
        
        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.playOnAwake = false;
    }
    
    private void Start() {
        // Play("BGMusic");
    }
    
    public void Play (string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null){
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }

    public void PlayFootstep(string surfaceTag)
    {
        FootstepGroup group = Array.Find(footstepGroups, g => g.tag == surfaceTag);
        if (group != null && group.footstepClips.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, group.footstepClips.Length);
            AudioClip clipToPlay = group.footstepClips[randomIndex];
            footstepSource.pitch = UnityEngine.Random.Range(0.85f, 1.15f); 
            footstepSource.PlayOneShot(clipToPlay);
        }
    }
}