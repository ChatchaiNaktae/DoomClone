using UnityEngine;
using Unity.Netcode;

public class ExplosiveBarrel : NetworkBehaviour, IDamageable, IExplosive
{
    public float health = 20f;
    public float explosionRadius = 4f;
    public float explosionDamage = 128f;
    public GameObject explosionEffect;
    
    private Animator barrelAnimator;
    private Collider barrelCollider;
    
    // Synced explosion state for all clients and late joiners
    public NetworkVariable<bool> hasExploded = new NetworkVariable<bool>(
        false, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        barrelAnimator = GetComponentInChildren<Animator>();
        barrelCollider = GetComponent<Collider>();
        
        // Listen for explosion state changes
        hasExploded.OnValueChanged += OnExplosionStateChanged;
        
        // If late-joining client enters and barrel already exploded
        if (hasExploded.Value)
        {
            ApplyExplodedState(false);
        }
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        hasExploded.OnValueChanged -= OnExplosionStateChanged;
    }
    
    public void TakeDamage(float damage)
    {
        if (hasExploded.Value) return;
        
        RequestDamageServerRpc(damage);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestDamageServerRpc(float damage)
    {
        if (hasExploded.Value) return;
        
        health -= damage;
        
        if (health <= 0)
        {
            Explode();
        }
    }
    
    public void Explode()
    {
        if (hasExploded.Value) return;
        hasExploded.Value = true; // Server updates NetworkVariable
        
        // Server calculates area damage
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            IDamageable damageableObj = hit.GetComponent<IDamageable>() ?? hit.GetComponentInParent<IDamageable>();
            
            if (damageableObj != null && hit.gameObject != this.gameObject) 
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                float damagePercent = Mathf.Clamp01(1f - (distance / explosionRadius));
                float finalCalculatedDamage = explosionDamage * damagePercent;
                
                damageableObj.TakeDamage(finalCalculatedDamage);
            }
        }
    }
    
    private void OnExplosionStateChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            ApplyExplodedState(true);
        }
    }
    
    private void ApplyExplodedState(bool spawnFx)
    {
        if (spawnFx && explosionEffect != null)
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