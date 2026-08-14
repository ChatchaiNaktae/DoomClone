using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Weapon Data")]
    public GunData currentGunData; 
    
    private CustomTimer fireCooldownTimer;
    private bool canFire = true;
    private int currentAmmo; 
    
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
    
    [Header("Animation")]
    public Animator gunAnimator; 
    
    [Header("Reload Settings")]
    private bool isReloading = false;
    
    void Start()
    {
        gunTrigger = GetComponent<BoxCollider>();
        gunTrigger.isTrigger = true; 
        
        if (gunAnimator == null)
        {
            gunAnimator = GetComponentInChildren<Animator>();
        }
        
        if (currentGunData != null)
        {
            EquipGun(currentGunData);
        }
        
        originalLocalPosition = transform.localPosition;
        recoilSpring = new SpringVector3(Vector3.zero, smoothTime);
    }
    
    public void EquipGun(GunData newGun)
    {
        currentGunData = newGun;
        
        gunTrigger.size = new Vector3(1, 20f, currentGunData.range);
        gunTrigger.center = new Vector3(0, 0, currentGunData.range * 0.5f);
        
        currentAmmo = currentGunData.maxAmmo;
        CanvasManager.Instance.UpdateAmmo(currentAmmo);
        isReloading = false;
        
        if (gunAnimator != null && currentGunData.gunAnimatorController != null)
        {
            gunAnimator.runtimeAnimatorController = currentGunData.gunAnimatorController;
        }
        
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
        
        if (!isReloading && currentGunData != null)
        {
            if (currentAmmo <= 0 || (Input.GetKeyDown(KeyCode.R) && currentAmmo < currentGunData.maxAmmo))
            {
                StartCoroutine(ReloadRoutine());
            }
        }
        
        if (!isReloading && Input.GetMouseButtonDown(0) && canFire && currentAmmo > 0 && currentGunData != null)
        {
            Fire();
            
            canFire = false;
            fireCooldownTimer.Start();
        }
        
        Vector3 currentRecoil = recoilSpring.Update(Time.deltaTime);
        transform.localPosition = originalLocalPosition + currentRecoil;
    }
    
    void Fire()
    {
        Debug.Log("Fired " + currentGunData.gunName + "!"); 
        
        if (gunAnimator != null)
        {
            gunAnimator.SetTrigger("ShootTrigger");
        }
        
        recoilSpring.Impulse(new Vector3(0, recoilUp, -recoilKickback));
        
        Collider[] enemyColliders = Physics.OverlapSphere(transform.position, currentGunData.gunShotRadius, enemyLayerMask);
        
        foreach (var enemyCollider in enemyColliders)
        {
            enemyCollider.GetComponent<EnemyAwareness>().isAggro = true;
        }
        
        AudioManager.instance.Play(currentGunData.shootSoundName);
        
        foreach (var enemy in enemyManager.enemiesInTrigger)
        {
            var dir = enemy.transform.position - transform.position;
            RaycastHit hit;
            
            if (Physics.Raycast(transform.position, dir, out hit, currentGunData.range * 1.5f, raycastLayerMask))
            {
                IDamageable target = hit.transform.GetComponent<IDamageable>();
                if (target != null)
                {
                    float dist = Vector3.Distance(hit.transform.position, transform.position);
                    if (dist > currentGunData.range * 0.5f)
                    {
                        target.TakeDamage(currentGunData.smallDamage);
                    }
                    else
                    {
                        target.TakeDamage(currentGunData.bigDamage);
                    }
                    
                    Debug.DrawRay(transform.position, dir, Color.green, 2f); 
                }
            }
        }
        
        currentAmmo--;
        CanvasManager.Instance.UpdateAmmo(currentAmmo);
    }
    
    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        Debug.Log("Reloading...");
        
        if (gunAnimator != null)
        {
            gunAnimator.SetTrigger("ReloadTrigger");
        }
        
        yield return new WaitForSeconds(currentGunData.reloadTime);
        
        currentAmmo = currentGunData.maxAmmo;
        CanvasManager.Instance.UpdateAmmo(currentAmmo);
        
        isReloading = false;
        Debug.Log("Reload Complete!");
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