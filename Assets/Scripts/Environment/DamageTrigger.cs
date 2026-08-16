using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    public int damageAmount = 10;
    public float timeBetweenDamage = 1.5f;
    
    private float damageCounter;
    private List<PlayerHealth> playersInTrigger = new List<PlayerHealth>();
    
    void Start()
    {
        damageCounter = timeBetweenDamage;
    }
    
    void Update()
    {
        playersInTrigger.RemoveAll(p => p == null);
        
        if (playersInTrigger.Count > 0)
        {
            if (damageCounter >= timeBetweenDamage)
            {
                for (int i = 0; i < playersInTrigger.Count; i++)
                {
                    if (playersInTrigger[i] != null)
                    {
                        playersInTrigger[i].DamagePlayer(damageAmount);
                    }
                }
                
                damageCounter = 0f;
            }
            
            damageCounter += Time.deltaTime;
        }
        else
        {
            damageCounter = 0f;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health == null)
            {
                health = other.GetComponentInParent<PlayerHealth>();
            }
            
            if (health != null && !playersInTrigger.Contains(health))
            {
                playersInTrigger.Add(health);
                damageCounter = timeBetweenDamage;
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health == null)
            {
                health = other.GetComponentInParent<PlayerHealth>();
            }
            
            if (health != null && playersInTrigger.Contains(health))
            {
                playersInTrigger.Remove(health);
            }
        }
    }
}