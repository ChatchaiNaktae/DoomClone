using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public int maxHealth;
    private int health;
    
    public int maxArmor;
    private int armor;
    
    private bool isDead = false;
    
    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        CanvasManager.Instance.UpdateHealth(health, maxHealth);
        CanvasManager.Instance.UpdateArmor(armor);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (isDead) return;
        
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
        
        if (armor > 0)
        {
            if (armor >= damage)
            {
                armor -= damage;
                AudioManager.instance.Play("PlayerDamaged");
            }
            else
            {
                int remainingDamage;
                remainingDamage = damage - armor;
                armor = 0;
                health -= remainingDamage;
                AudioManager.instance.Play("PlayerPain");
            }
        }
        else
        {
            health -= damage;
            AudioManager.instance.Play("PlayerPain");
        }
        
        if (health <= 0)
        {
            health = 0;
            isDead = true;
            StartCoroutine(PlayerDead());
        }
        
        CanvasManager.Instance.UpdateHealth(health, maxHealth);
        CanvasManager.Instance.UpdateArmor(armor);
    }
    
    private IEnumerator PlayerDead()
    {
        // handle player dead
        Debug.Log("Player is Died");
        
        AudioManager.instance.Play($"PlayerDeath{Random.Range(1, 3)}");
        
        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<MouseLook>().enabled = false;
        
        Gun gunScript = GetComponentInChildren<Gun>();
        if (gunScript != null) 
        {
            gunScript.enabled = false; 
            if (gunScript.gunAnimator != null)
            {
                gunScript.gunAnimator.SetTrigger("HideTrigger");
            }
        }
        
        Animator camAnim = GetComponent<PlayerMovement>().cameraAnimator;
        if (camAnim != null) camAnim.enabled = false;
        
        Transform camTransform = Camera.main.transform;
        float elapsed = 0f;
        float fallDuration = 0.5f;
        
        Vector3 startPos = camTransform.localPosition;
        Vector3 endPos = new Vector3(startPos.x, -0.6f, startPos.z);
        
        Quaternion startRot = camTransform.localRotation;
        Quaternion endRot = Quaternion.Euler(0f, 0f, 75f);
        
        while (elapsed < fallDuration)
        {
            camTransform.localPosition = Vector3.Lerp(startPos, endPos, elapsed / fallDuration);
            camTransform.localRotation = Quaternion.Lerp(startRot, endRot, elapsed / fallDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(5f - fallDuration);
        
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
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