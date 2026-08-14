using UnityEngine;

// Interface for all equippable weapons
public interface IWeapon
{
    void Attack();
    void Equip();
    int GetCurrentAmmo();
}