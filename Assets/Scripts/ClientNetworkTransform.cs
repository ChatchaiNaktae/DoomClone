using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

// This script allows the local client/owner to synchronize its position and rotation directly
[DisallowMultipleComponent]
public class ClientNetworkTransform : NetworkTransform
{
    // Override authority check: return false means Client/Owner has authority
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}