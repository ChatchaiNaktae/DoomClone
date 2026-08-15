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
    
    [Header("Effects")]
    public GameObject bulletHolePrefab; 
    
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
            EnemyAwareness awareness = enemyCollider.GetComponent<EnemyAwareness>();
            if (awareness != null)
            {
                awareness.isAggro = true;
            }
        }
        
        AudioManager.instance.Play(currentGunData.shootSoundName);
        
        foreach (var enemy in enemyManager.enemiesInTrigger)
        {
            var dir = enemy.transform.position - transform.position;
            RaycastHit hit;
            
            if (Physics.Raycast(transform.position, dir, out hit, currentGunData.range, raycastLayerMask))
            {
                IDamageable target = hit.transform.GetComponent<IDamageable>();
                if (target != null)
                {
                    int minDmg = (int)currentGunData.smallDamage;
                    int maxDmg = (int)currentGunData.bigDamage;
                    float finalDamage = UnityEngine.Random.Range(minDmg, maxDmg + 1);
                    
                    target.TakeDamage(finalDamage);
                    
                    Debug.DrawRay(transform.position, dir, Color.green, 2f); 
                }
            }
        }
        
        Transform camTransform = Camera.main.transform; 
        RaycastHit wallHit;
        
        if (Physics.Raycast(camTransform.position, camTransform.forward, out wallHit, currentGunData.range, raycastLayerMask))
        {
            IDamageable targetHit = wallHit.transform.GetComponent<IDamageable>();
            Debug.Log("ยิงโดนวัตถุ: " + wallHit.transform.name + " | มี IDamageable ไหม: " + (targetHit != null));
            if (targetHit != null)
            {
                if (wallHit.transform.GetComponent<Enemy>() == null)
                {
                    int minDmg = (int)currentGunData.smallDamage;
                    int maxDmg = (int)currentGunData.bigDamage;
                    float finalDamage = UnityEngine.Random.Range(minDmg, maxDmg + 1);
                    
                    targetHit.TakeDamage(finalDamage);
                    Debug.Log("ทำดาเมจใส่ถัง: " + finalDamage);
                }
            }
            else
            {
                if (bulletHolePrefab != null)
                {
                    Vector3 spawnPos = wallHit.point + (wallHit.normal * 0.01f);
                    Quaternion spawnRot = Quaternion.LookRotation(-wallHit.normal);
                    
                    float randomRoll = UnityEngine.Random.Range(0f, 360f);
                    spawnRot *= Quaternion.Euler(0f, 0f, randomRoll);
                    
                    GameObject hole = Instantiate(bulletHolePrefab, spawnPos, spawnRot);
                    hole.transform.SetParent(wallHit.transform);
                    
                    Renderer wallRenderer = wallHit.transform.GetComponent<Renderer>();
                    if (wallRenderer != null)
                    {
                        Color wallColor = Color.white;
                        
                        if (wallRenderer.material.HasProperty("_BaseColor"))
                        {
                            wallColor = wallRenderer.material.GetColor("_BaseColor");
                        }
                        else if (wallRenderer.material.HasProperty("_Color"))
                        {
                            wallColor = wallRenderer.material.color;
                        }
                        
                        Renderer holeRenderer = hole.GetComponent<Renderer>();
                        if (holeRenderer != null)
                        {
                            if (holeRenderer.material.HasProperty("_BaseColor"))
                            {
                                holeRenderer.material.SetColor("_BaseColor", wallColor);
                            }
                            else if (holeRenderer.material.HasProperty("_Color"))
                            {
                                holeRenderer.material.color = wallColor;
                            }
                        }
                    }
                    
                    Destroy(hole, 10f);
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