using UnityEngine;

// Enum to define the type of item
public enum ItemType 
{ 
    Health, 
    Armor, 
    Ammo ,
    Gun
}

[CreateAssetMenu(fileName = "New Item", menuName = "DoomClone/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Item Details")]
    public string itemName;
    public ItemType itemType; // Dropdown in Inspector to choose Health, Armor, or Ammo
    public int amount; // How much health/armor/ammo this item gives
    
    [Header("For Gun Pickups Only")]
    public GunData gunData;
}