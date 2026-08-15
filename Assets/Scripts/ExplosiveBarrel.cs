using UnityEngine;

// Barrel uses both IDamageable and IExplosive interfaces
public class ExplosiveBarrel : MonoBehaviour, IDamageable, IExplosive
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
        
        Debug.Log("Barrel EXPLODED!");
        
        if (explosionEffect != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0f, 1f, 0f);
            GameObject spawnedEffect = Instantiate(explosionEffect, spawnPos, Quaternion.identity);
            Destroy(spawnedEffect, 0.15f);
        }
        else
        {
            Debug.LogWarning("Explosion Effect Prefab is missing in Inspector!");
        }
        
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
        
        if (barrelAnimator != null)
        {
            barrelAnimator.SetTrigger("ExplodeTrigger");
        }
        else
        {
            Debug.LogWarning("Animator not found on Barrel or its children!");
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