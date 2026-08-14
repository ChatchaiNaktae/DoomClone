using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Weapon Data")]
    public GunData currentGunData; // The ScriptableObject that holds all gun stats
    
    private CustomTimer fireCooldownTimer;
    private bool canFire = true;
    private int currentAmmo; // Current ammo needs to be tracked separately from maxAmmo
    
    public LayerMask raycastLayerMask;
    public LayerMask enemyLayerMask;
    
    private BoxCollider gunTrigger;
    public EnemyManager enemyManager;
    
    [Header("Recoil Settings")]
    public float recoilKickback = 0.8f;
    public float recoilUp = 0.3f;
    public float smoothTime = 0.2f;
    
    private SpringVector3 recoilSpring;
    private Vector3 originalLocalPosition;
    
    void Start()
    {
        gunTrigger = GetComponent<BoxCollider>();
        gunTrigger.isTrigger = true; 
        
        // Equip the gun and load data at the start of the game
        if (currentGunData != null)
        {
            EquipGun(currentGunData);
        }
        
        originalLocalPosition = transform.localPosition;
        recoilSpring = new SpringVector3(Vector3.zero, smoothTime);
    }
    
    // Call this function whenever you want to swap to a different gun!
    public void EquipGun(GunData newGun)
    {
        currentGunData = newGun;
        
        // Update the trigger size based on the new gun's range
        gunTrigger.size = new Vector3(1, 20f, currentGunData.range);
        gunTrigger.center = new Vector3(0, 0, currentGunData.range * 0.5f);
        
        // Reset ammo when equipping a new gun
        currentAmmo = currentGunData.maxAmmo;
        CanvasManager.Instance.UpdateAmmo(currentAmmo);
        
        // Reset and update the fire rate timer
        if (fireCooldownTimer != null)
        {
            fireCooldownTimer.Stop();
        }
        
        fireCooldownTimer = new CustomTimer(currentGunData.fireRate);
        fireCooldownTimer.OnTick += () => {
            canFire = true;
            fireCooldownTimer.Stop();
        };
    }
    
    void Update()
    {
        if (fireCooldownTimer != null)
        {
            fireCooldownTimer.Update();
        }
        
        // Check if player can fire and has ammo
        if (Input.GetMouseButtonDown(0) && canFire && currentAmmo > 0 && currentGunData != null)
        {
            Fire();
            
            canFire = false;
            fireCooldownTimer.Start();
        }
        
        // Apply recoil offset from the spring module to the local position
        Vector3 currentRecoil = recoilSpring.Update(Time.deltaTime);
        transform.localPosition = originalLocalPosition + currentRecoil;
    }
    
    void Fire()
    {
        Debug.Log("Fired " + currentGunData.gunName + "!"); 
        
        // Push gun backward (Z-axis) and upward (Y-axis)
        recoilSpring.Impulse(new Vector3(0, recoilUp, -recoilKickback));
        
        // simulate gun shot radius using data from ScriptableObject
        Collider[] enemyColliders = Physics.OverlapSphere(transform.position, currentGunData.gunShotRadius, enemyLayerMask);
        
        foreach (var enemyCollider in enemyColliders)
        {
            enemyCollider.GetComponent<EnemyAwareness>().isAggro = true;
        }
        
        // Play the specific sound for this gun
        AudioManager.instance.Play(currentGunData.shootSoundName);
        
        foreach (var enemy in enemyManager.enemiesInTrigger)
        {
            var dir = enemy.transform.position - transform.position;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, dir, out hit, currentGunData.range * 1.5f, raycastLayerMask))
            {
                if (hit.transform == enemy.transform)
                {
                    float dist = Vector3.Distance(enemy.transform.position, transform.position);
                    if (dist > currentGunData.range * 0.5f)
                    {
                        enemy.TakeDamage(currentGunData.smallDamage);
                    }
                    else
                    {
                        enemy.TakeDamage(currentGunData.bigDamage);
                    }
                    
                    Debug.DrawRay(transform.position, dir, Color.green, 2f); 
                }
            }
        }
        
        currentAmmo--;
        CanvasManager.Instance.UpdateAmmo(currentAmmo);
    }
    
    public void GiveAmmo(int amount, GameObject pickup)
    {
        if (currentGunData == null) return;
        
        if (currentAmmo < currentGunData.maxAmmo)
        {
            currentAmmo += amount;
            Destroy(pickup);
        }
        
        if (currentAmmo > currentGunData.maxAmmo)
        {
            currentAmmo = currentGunData.maxAmmo;
        }
        
        CanvasManager.Instance.UpdateAmmo(currentAmmo);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.transform.GetComponent<Enemy>();
        if (enemy)
        {
            enemyManager.AddEnemy(enemy);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.transform.GetComponent<Enemy>();
        if (enemy)
        {
            enemyManager.RemoveEnemy(enemy);
        }
    }
}