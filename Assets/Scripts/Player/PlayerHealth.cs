using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour, IDamageable
{
    public int maxHealth;
    private int health;
    
    public int maxArmor;
    private int armor;
    
    private bool isDead = false;
    
    // Save the spawn location and initial view for respawning.
    private Vector3 initialSpawnPosition;
    private Quaternion initialSpawnRotation;
    private Vector3 initialCamLocalPosition;
    private Quaternion initialCamLocalRotation;
    
    // Save initial weapon local transform
    private Vector3 initialGunLocalPosition;
    private Quaternion initialGunLocalRotation;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Record initial occurrence location
        initialSpawnPosition = transform.position;
        initialSpawnRotation = transform.rotation;
        
        Transform camTransform = GetComponentInChildren<Camera>()?.transform;
        if (camTransform != null)
        {
            initialCamLocalPosition = camTransform.localPosition;
            initialCamLocalRotation = camTransform.localRotation;
        }
        
        Gun gunScript = GetComponentInChildren<Gun>();
        if (gunScript != null)
        {
            initialGunLocalPosition = gunScript.transform.localPosition;
            initialGunLocalRotation = gunScript.transform.localRotation;
        }
        
        health = maxHealth;
        
        if (IsOwner && CanvasManager.Instance != null)
        {
            CanvasManager.Instance.UpdateHealth(health, maxHealth);
            CanvasManager.Instance.UpdateArmor(armor);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        // Moved most initial setup to OnNetworkSpawn to support Netcode.
    }
    
    // Update is called once per frame
    void Update()
    {
        if (isDead) return;
        
        // Process only on the machine that owns the character.
        if (!IsOwner) return;
        
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            DamagePlayer(30);
            Debug.Log("Player is Damaged");
        }
    }
    
    public void TakeDamage(float damage)
    {
        DamagePlayer(Mathf.RoundToInt(damage));
    }
    
    public void DamagePlayer(int damage)
    {
        if (isDead) return;
        
        // Send a blood reduction request to the server.
        RequestDamageServerRpc(damage);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestDamageServerRpc(int damage)
    {
        if (isDead) return;
        
        string soundToPlay = "";
        
        if (armor > 0)
        {
            if (armor >= damage)
            {
                armor -= damage;
                soundToPlay = "PlayerDamaged";
            }
            else
            {
                int remainingDamage;
                remainingDamage = damage - armor;
                armor = 0;
                health -= remainingDamage;
                soundToPlay = "PlayerPain";
            }
        }
        else
        {
            health -= damage;
            soundToPlay = "PlayerPain";
        }
        
        if (health <= 0)
        {
            health = 0;
            isDead = true;
            TriggerDeathClientRpc();
        }
        
        // Sync blood values, armor, and sound to the client.
        SyncStatsClientRpc(health, armor, soundToPlay);
    }
    
    [ClientRpc]
    private void SyncStatsClientRpc(int syncHealth, int syncArmor, string soundName)
    {
        health = syncHealth;
        armor = syncArmor;
        
        if (!string.IsNullOrEmpty(soundName) && AudioManager.instance != null)
        {
            AudioManager.instance.Play(soundName);
        }
        
        if (IsOwner && CanvasManager.Instance != null)
        {
            CanvasManager.Instance.UpdateHealth(health, maxHealth);
            CanvasManager.Instance.UpdateArmor(armor);
        }
    }
    
    [ClientRpc]
    private void TriggerDeathClientRpc()
    {
        isDead = true;
        StartCoroutine(PlayerDead());
    }
    
    private IEnumerator PlayerDead()
    {
        // handle player dead
        Debug.Log("Player is Died");
        
        AudioManager.instance.Play($"PlayerDeath{Random.Range(1, 3)}");
        
        // Shut down the control system.
        PlayerMovement movement = GetComponent<PlayerMovement>();
        MouseLook mouseLook = GetComponent<MouseLook>();
        CharacterController controller = GetComponent<CharacterController>();
        
        if (movement != null) movement.enabled = false;
        if (mouseLook != null) mouseLook.enabled = false;
        if (controller != null) controller.enabled = false;
        
        Gun gunScript = GetComponentInChildren<Gun>();
        if (gunScript != null) 
        {
            gunScript.enabled = false; 
            if (gunScript.gunAnimator != null)
            {
                gunScript.gunAnimator.SetTrigger("HideTrigger");
            }
        }
        
        Animator camAnim = movement != null ? movement.cameraAnimator : null;
        if (camAnim != null) camAnim.enabled = false;
        
        // Use your character's camera instead of `Camera.main` to avoid accessing a teammate's camera.
        Camera playerCamera = GetComponentInChildren<Camera>();
        Transform camTransform = playerCamera != null ? playerCamera.transform : transform;
        
        float elapsed = 0f;
        float fallDuration = 0.5f;
        
        Vector3 startPos = camTransform.localPosition;
        Vector3 endPos = new Vector3(startPos.x, -0.6f, startPos.z);
        
        Quaternion startRot = camTransform.localRotation;
        Quaternion endRot = Quaternion.Euler(0f, 0f, 75f);
        
        // The camera-falling-to-the-ground animation plays only on the character owner's screen.
        if (IsOwner)
        {
            while (elapsed < fallDuration)
            {
                camTransform.localPosition = Vector3.Lerp(startPos, endPos, elapsed / fallDuration);
                camTransform.localRotation = Quaternion.Lerp(startRot, endRot, elapsed / fallDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        
        yield return new WaitForSeconds(5f - fallDuration);
        
        // In the multiplayer system: The character respawns at the spawn point instead of reloading the scene.
        RespawnPlayer(camTransform, camAnim);
    }
    
    private void RespawnPlayer(Transform camTransform, Animator camAnim)
    {
        isDead = false;
        
        // Restore position and camera angle.
        if (IsOwner && camTransform != null)
        {
            camTransform.localPosition = initialCamLocalPosition;
            camTransform.localRotation = initialCamLocalRotation;
        }
        
        if (camAnim != null) camAnim.enabled = true;
        
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;
        
        // Warp back to the starting spawn point.
        transform.position = initialSpawnPosition;
        transform.rotation = initialSpawnRotation;
        
        // Restore weapon transform and reset animator state
        Gun gunScript = GetComponentInChildren<Gun>();
        if (gunScript != null)
        {
            gunScript.transform.localPosition = initialGunLocalPosition;
            gunScript.transform.localRotation = initialGunLocalRotation;
            
            if (gunScript.gunAnimator != null)
            {
                gunScript.gunAnimator.Rebind();
                gunScript.gunAnimator.Update(0f);
            }
        }
        
        // Reactivate the control system exclusively for our own use.
        if (IsOwner)
        {
            PlayerMovement movement = GetComponent<PlayerMovement>();
            MouseLook mouseLook = GetComponent<MouseLook>();
            
            if (movement != null) movement.enabled = true;
            if (mouseLook != null) mouseLook.enabled = true;
            if (controller != null) controller.enabled = true; // เปิด controller กลับมาหลังจากย้ายตำแหน่งแล้ว
            
            if (gunScript != null) gunScript.enabled = true;
        }
        
        // Set the server to restore full health.
        if (IsServer)
        {
            health = maxHealth;
            armor = 0;
            SyncStatsClientRpc(health, armor, "");
        }
    }
    
    public void GiveHealth(int amount, GameObject pickup)
    {
        if (isDead) return;
        
        if (health < maxHealth)
        {
            health += amount;
            Destroy(pickup);
        }
        
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        
        CanvasManager.Instance.UpdateHealth(health, maxHealth);
    }
    
    public void GiveArmor(int amount, GameObject pickup)
    {
        if (isDead) return;
        
        if (armor < maxArmor)
        {
            armor += amount;
            Destroy(pickup);
        }
        
        if (armor > maxArmor)
        {
            armor = maxArmor;
        }
        
        CanvasManager.Instance.UpdateArmor(armor);
    }
}