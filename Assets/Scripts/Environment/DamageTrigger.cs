using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    public int damageAmount = 10;
    public float timeBetweenDamage = 1.5f;
    
    private float damageCounter;
    private List<PlayerHealth> playersInTrigger = new List<PlayerHealth>();
    private Collider triggerCollider;
    
    void Start()
    {
        damageCounter = timeBetweenDamage;
        triggerCollider = GetComponent<Collider>();
    }
    
    void Update()
    {
        for (int i = playersInTrigger.Count - 1; i >= 0; i--)
        {
            PlayerHealth player = playersInTrigger[i];
            
            if (player == null)
            {
                playersInTrigger.RemoveAt(i);
                continue;
            }
            
            if (triggerCollider != null)
            {
                Vector3 closestPoint = triggerCollider.ClosestPoint(player.transform.position);
                float distanceToTrigger = Vector3.Distance(closestPoint, player.transform.position);
                if (distanceToTrigger > 1.5f)
                {
                    playersInTrigger.RemoveAt(i);
                }
            }
        }
        
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