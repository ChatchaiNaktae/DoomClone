using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    private bool damagingPlayer;
    private PlayerHealth playerHealth;
    
    public int damageAmount = 10;
    public float timeBetweenDamage = 1.5f;

    private float damageCounter;
    
    void Start()
    {
        damageCounter = timeBetweenDamage;
        playerHealth = FindObjectOfType<PlayerHealth>();
    }
    
    void Update()
    {
        if (damagingPlayer)
        {
            if (damageCounter >= timeBetweenDamage)
            {
                // damage player
                playerHealth.DamagePlayer(damageAmount);
                
                // restart counter
                damageCounter = 0f;
            }
            
            // add to counter
            damageCounter += Time.deltaTime;
        }
        else
        {
            // keep damage counter reset
            damageCounter = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            damageCounter = timeBetweenDamage;
            damagingPlayer = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            damagingPlayer = false;
        }
    }
}
