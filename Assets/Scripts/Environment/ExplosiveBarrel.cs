using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// Barrel uses both IDamageable and IExplosive interfaces
public class ExplosiveBarrel : NetworkBehaviour, IDamageable, IExplosive
{
    public float health = 20f;
    public float explosionRadius = 4f;
    public float explosionDamage = 128f;
    public GameObject explosionEffect;
    
    private Animator barrelAnimator;
    private Collider barrelCollider;
    private bool hasExploded = false;
    
    void Start()
    {
        barrelAnimator = GetComponentInChildren<Animator>();
        barrelCollider = GetComponent<Collider>();
    }
    
    public void TakeDamage(float damage)
    {
        if (hasExploded) return;
        
        RequestDamageServerRpc(damage);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestDamageServerRpc(float damage)
    {
        if (hasExploded) return;
        
        health -= damage;
        Debug.Log("Barrel took " + damage + " damage. Current Health: " + health);
        
        if (health <= 0)
        {
            Explode();
        }
    }
    
    // Implemented from IExplosive
    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        
        Debug.Log("Barrel EXPLODED on Server!");
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            IDamageable damageableObj = hit.GetComponent<IDamageable>();
            if (damageableObj == null)
            {
                damageableObj = hit.GetComponentInParent<IDamageable>();
            }
            
            if (damageableObj != null && hit.gameObject != this.gameObject) 
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                float damagePercent = 1f - Mathf.Clamp01(distance / explosionRadius);
                float finalCalculatedDamage = explosionDamage * damagePercent;
                
                damageableObj.TakeDamage(finalCalculatedDamage);
                
                Debug.Log($"ทำดาเมจใส่ {hit.name}: {finalCalculatedDamage:F1} (ระยะห่าง {distance:F1}m)");
            }
        }
        
        ExplodeClientRpc();
    }
    
    [ClientRpc]
    private void ExplodeClientRpc()
    {
        hasExploded = true;
        
        if (explosionEffect != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0f, 1f, 0f);
            GameObject spawnedEffect = Instantiate(explosionEffect, spawnPos, Quaternion.identity);
            Destroy(spawnedEffect, 0.15f);
        }
        
        if (barrelAnimator != null)
        {
            barrelAnimator.SetTrigger("ExplodeTrigger");
        }
        
        if (barrelCollider != null)
        {
            barrelCollider.enabled = false;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}