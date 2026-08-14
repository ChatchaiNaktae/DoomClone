using UnityEngine;

// Interface for objects the player can interact with (Doors, Switches, etc.)
public interface IInteractable
{
    void Interact();
    string GetInteractText();
}