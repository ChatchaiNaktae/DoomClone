using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Gun : NetworkBehaviour
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
    
    private Camera plrCamera;
    
    void Start()
    {
        gunTrigger = GetComponent<BoxCollider>();
        if (gunTrigger != null)
        {
            gunTrigger.isTrigger = true; 
        }
        
        if (enemyManager == null)
        {
            enemyManager = FindObjectOfType<EnemyManager>();
        }
        
        plrCamera = GetComponentInParent<Camera>();
        if (plrCamera == null)
        {
            plrCamera = transform.root.GetComponentInChildren<Camera>();
        }
        
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
        
        if (gunTrigger != null)
        {
            gunTrigger.size = new Vector3(1, 20f, currentGunData.range);
            gunTrigger.center = new Vector3(0, 0, currentGunData.range * 0.5f);
        }
        
        currentAmmo = currentGunData.maxAmmo;
        if (IsOwner && CanvasManager.Instance != null)
        {
            CanvasManager.Instance.UpdateAmmo(currentAmmo);
        }
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
            if (fireCooldownTimer != null) fireCooldownTimer.Stop();
        };
    }
    
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        
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
            if (fireCooldownTimer != null) fireCooldownTimer.Start();
        }
        
        if (recoilSpring != null)
        {
            Vector3 currentRecoil = recoilSpring.Update(Time.deltaTime);
            transform.localPosition = originalLocalPosition + currentRecoil;
        }
    }
    
    void Fire()
    {
        Debug.Log("Fired " + currentGunData.gunName + "!"); 
        
        if (gunAnimator != null)
        {
            gunAnimator.SetTrigger("ShootTrigger");
        }
        
        if (recoilSpring != null)
        {
            recoilSpring.Impulse(new Vector3(0, recoilUp, -recoilKickback));
        }
        
        Collider[] enemyColliders = Physics.OverlapSphere(transform.position, currentGunData.gunShotRadius, enemyLayerMask);
        
        foreach (var enemyCollider in enemyColliders)
        {
            if (enemyCollider != null)
            {
                EnemyAwareness awareness = enemyCollider.GetComponent<EnemyAwareness>();
                if (awareness != null)
                {
                    awareness.isAggro = true;
                }
            }
        }
        
        PlayShootEffectsServerRpc(currentGunData.shootSoundName);
        
        if (enemyManager != null && enemyManager.enemiesInTrigger != null)
        {
            foreach (var enemy in enemyManager.enemiesInTrigger)
            {
                if (enemy == null) continue;
                
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
        }
        
        Transform camTransform = plrCamera != null ? plrCamera.transform : transform;
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
                SpawnBulletHole(wallHit.point, wallHit.normal, wallHit.transform);
                SpawnBulletHoleServerRpc(wallHit.point, wallHit.normal);
            }
        }
        
        currentAmmo--;
        if (IsOwner && CanvasManager.Instance != null)
        {
            CanvasManager.Instance.UpdateAmmo(currentAmmo);
        }
    }
    
    [ServerRpc]
    private void PlayShootEffectsServerRpc(string soundName)
    {
        PlayShootEffectsClientRpc(soundName);
    }
    
    [ClientRpc]
    private void PlayShootEffectsClientRpc(string soundName)
    {
        if (AudioManager.instance != null && !string.IsNullOrEmpty(soundName))
        {
            AudioManager.instance.Play(soundName);
        }
    }
    
    [ServerRpc]
    private void SpawnBulletHoleServerRpc(Vector3 point, Vector3 normal)
    {
        SpawnBulletHoleClientRpc(point, normal);
    }
    
    [ClientRpc]
    private void SpawnBulletHoleClientRpc(Vector3 point, Vector3 normal)
    {
        if (IsOwner) return; // เจ้าของเครื่องสร้างไปแล้วตอนกดยิง ไม่ต้องสร้างซ้ำ
        SpawnBulletHole(point, normal, null);
    }
    
    private void SpawnBulletHole(Vector3 point, Vector3 normal, Transform parentTransform)
    {
        if (bulletHolePrefab != null)
        {
            Vector3 spawnPos = point + (normal * 0.01f);
            Quaternion spawnRot = Quaternion.LookRotation(-normal);
            
            float randomRoll = UnityEngine.Random.Range(0f, 360f);
            spawnRot *= Quaternion.Euler(0f, 0f, randomRoll);
            
            GameObject hole = Instantiate(bulletHolePrefab, spawnPos, spawnRot);
            if (parentTransform != null)
            {
                hole.transform.SetParent(parentTransform);
            }
            
            Destroy(hole, 10f);
        }
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
        if (IsOwner && CanvasManager.Instance != null)
        {
            CanvasManager.Instance.UpdateAmmo(currentAmmo);
        }
        
        isReloading = false;
        Debug.Log("Reload Complete!");
    }
    
    public void GiveAmmo(int amount, GameObject pickup)
    {
        if (currentGunData == null) return;
        
        if (currentAmmo < currentGunData.maxAmmo)
        {
            currentAmmo += amount;
            if (IsServer)
            {
                NetworkObject netObj = pickup.GetComponent<NetworkObject>();
                if (netObj != null && netObj.IsSpawned) netObj.Despawn(true);
                else Destroy(pickup);
            }
        }
        
        if (currentAmmo > currentGunData.maxAmmo)
        {
            currentAmmo = currentGunData.maxAmmo;
        }
        
        if (IsOwner && CanvasManager.Instance != null)
        {
            CanvasManager.Instance.UpdateAmmo(currentAmmo);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (enemyManager == null)
        {
            enemyManager = FindObjectOfType<EnemyManager>();
        }
        
        if (enemyManager != null)
        {
            Enemy enemy = other.transform.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemyManager.AddEnemy(enemy);
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (enemyManager == null)
        {
            enemyManager = FindObjectOfType<EnemyManager>();
        }
        
        if (enemyManager != null)
        {
            Enemy enemy = other.transform.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemyManager.RemoveEnemy(enemy);
            }
        }
    }
}