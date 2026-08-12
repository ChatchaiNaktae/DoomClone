using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float range = 20f;
    public float verticalRange = 20f;
    public float gunShotRadius = 20f;
    
    public float bigDamage = 2f;
    public float smallDamage = 1f;
    
    public float fireRate = 1f;
    private float nextTimeFire;

    public int maxAmmo;
    private int ammo = 10;
    
    public LayerMask raycastLayerMask;
    public LayerMask enemyLayerMask;
    
    private BoxCollider gunTrigger;
    public EnemyManager enemyManager;
    
    void Start()
    {
        gunTrigger = GetComponent<BoxCollider>();
        gunTrigger.isTrigger = true; 
        gunTrigger.size = new Vector3(1, verticalRange, range);
        gunTrigger.center = new Vector3(0, 0, range * 0.5f);
        
        CanvasManager.Instance.UpdateAmmo(ammo);
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time > nextTimeFire && ammo > 0)
        {
            Fire();
        }
    }
    
    void Fire()
    {
        // Log a message to the console to confirm the left click works
        Debug.Log("Gun Fired!"); 
        
        // simulate gun shot radius

        Collider[] enemyColliders = Physics.OverlapSphere(transform.position, gunShotRadius, enemyLayerMask);
        
        // alert any enemy in earshot
        foreach (var enemyCollider in enemyColliders)
        {
            enemyCollider.GetComponent<EnemyAwareness>().isAggro = true;
        }
        
        // play test audio
        AudioManager.instance.Play("Shoot");
        
        // loop to find and damage enemies inside the trigger zone
        foreach (var enemy in enemyManager.enemiesInTrigger)
        {
            // get direction to enemy
            var dir = enemy.transform.position - transform.position;
            
            RaycastHit hit;
            if (Physics.Raycast(transform.position, dir, out hit, range * 1.5f, raycastLayerMask))
            {
                if (hit.transform == enemy.transform)
                {
                    // range check
                    float dist = Vector3.Distance(enemy.transform.position, transform.position);
                    
                    if (dist > range * 0.5f)
                    {
                        // damage enemy small
                        enemy.TakeDamage(smallDamage);
                        Debug.Log("Hit enemy from afar! Small damage applied.");
                    }
                    else
                    {
                        // damage enemy big
                        enemy.TakeDamage(bigDamage);
                        Debug.Log("Hit enemy close up! Big damage applied.");
                    }
                    
                    // Draw a green line in the Scene view for 2 seconds to show bullet path
                    Debug.DrawRay(transform.position, dir, Color.green, 2f); 
                }
            }
        }
        
        //  reset timer for the next shot
        nextTimeFire = Time.time + fireRate;
        
        // fire ammo
        ammo--;
        CanvasManager.Instance.UpdateAmmo(ammo);
    }

    public void GiveAmmo(int amount, GameObject pickup)
    {
        if (ammo < maxAmmo)
        {
            ammo += amount;
            Destroy(pickup);
        }

        if (ammo > maxAmmo)
        {
            ammo = maxAmmo;
        }
        
        CanvasManager.Instance.UpdateAmmo(ammo);
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