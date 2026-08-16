using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ItemPickup : NetworkBehaviour
{
    [Header("Item Configuration")]
    public ItemData itemData; 
    
    private bool hasBeenPickedUp = false;
    
    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (hasBeenPickedUp) return;
        
        if (other.CompareTag("Player"))
        {
            if (itemData == null)
            {
                Debug.LogWarning("ItemData is missing on this pickup object!");
                return;
            }
            
            hasBeenPickedUp = true;
            
            switch (itemData.itemType)
            {
                case ItemType.Health:
                    other.GetComponent<PlayerHealth>()?.GiveHealth(itemData.amount, this.gameObject);
                    break;
                case ItemType.Armor:
                    other.GetComponent<PlayerHealth>()?.GiveArmor(itemData.amount, this.gameObject);
                    break;
                case ItemType.Ammo:
                    other.GetComponentInChildren<Gun>()?.GiveAmmo(itemData.amount, this.gameObject);
                    break;
                
                // Add logic for picking up a gun
                case ItemType.Gun:
                    if (itemData.gunData != null)
                    {
                        // Equip the new gun using the data stored in the item
                        other.GetComponentInChildren<Gun>()?.EquipGun(itemData.gunData);
                        DestroyPickup();
                    }
                    else
                    {
                        Debug.LogWarning("GunData is missing in the ItemData template!");
                    }
                    break;
            }
        }
    }
    
    private void DestroyPickup()
    {
        NetworkUtils.DespawnOrDestroy(gameObject);
    }
}