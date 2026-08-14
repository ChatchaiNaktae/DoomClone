using UnityEngine;

// Interface for anything that can take damage (Enemies, Barrels, Glass, etc.)
public interface IDamageable
{
    void TakeDamage(float damage);
}