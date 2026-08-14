using UnityEngine;

// Barrel uses both IDamageable and IExplosive interfaces
public class ExplosiveBarrel : MonoBehaviour, IDamageable, IExplosive
{
    public float health = 10f;
    public float explosionRadius = 5f;
    public float explosionDamage = 50f;
    public GameObject explosionEffect;
    
    // Implemented from IDamageable
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Explode();
        }
    }

    // Implemented from IExplosive
    public void Explode()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            IDamageable damageableObj = hit.GetComponent<IDamageable>();
            if (damageableObj != null && hit.gameObject != this.gameObject) 
            {
                damageableObj.TakeDamage(explosionDamage);
            }
        }
        
        Destroy(gameObject);
    }
}