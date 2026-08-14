using UnityEngine;

// This allows us to right-click in Unity to create a new Gun Data file
[CreateAssetMenu(fileName = "New Gun", menuName = "DoomClone/Gun Data")]
public class GunData : ScriptableObject
{
    [Header("Basic Info")]
    public string gunName;
    
    [Header("Shooting Stats")]
    public float fireRate = 0.5f;
    public int maxAmmo = 10;
    public float range = 20f;
    public float gunShotRadius = 20f;
    
    [Header("Damage")]
    public float bigDamage = 2f;
    public float smallDamage = 1f;

    [Header("Audio")]
    public string shootSoundName = "Shoot"; // Matches the name in AudioManager
}