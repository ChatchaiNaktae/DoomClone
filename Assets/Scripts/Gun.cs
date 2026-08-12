using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float range = 20f;
    public float verticalRange = 20f;
    public float fireRate;
    public float bigDamage = 2f;
    public float smallDamage = 1f;

    private float nextTimeFire;
    private BoxCollider gunTrigger;

    public LayerMask raycastLayerMask;
    public EnemyManager enemyManager;
    
    void Start()
    {
        gunTrigger = GetComponent<BoxCollider>();
        gunTrigger.size = new Vector3(1, verticalRange, range);
        gunTrigger.center = new Vector3(0, 0, range * 0.5f);
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time > nextTimeFire)
        {
            Fire();
        }
    }

    void Fire()
    {
        // damage enemies
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
                    }
                    else
                    {
                        // damage enemy big
                        enemy.TakeDamage(bigDamage);
                    }
                    
                    Debug.DrawRay(transform.position, dir, Color.green);
                    Debug.Break();
                }
            }
        }
        
        //  reset timer
        nextTimeFire = Time.time + fireRate;
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
