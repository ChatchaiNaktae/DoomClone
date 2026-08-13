using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasManager : MonoBehaviour
{
    public TextMeshProUGUI health;
    public TextMeshProUGUI armor;
    public TextMeshProUGUI ammo;
    
    [Header("Health Bar Settings")]
    public Image healthBarImage;
    public float tweenSpeed = 5f;
    private float targetHealthFill = 1f;
    
    public Image healthIndicator;
    
    public Sprite health1; // healthy
    public Sprite health2;
    public Sprite health3;
    public Sprite health4; // dead
    
    private static CanvasManager _instance;
    public static CanvasManager Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Update()
    {
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = Mathf.Lerp(healthBarImage.fillAmount, targetHealthFill, tweenSpeed * Time.deltaTime);
        }
    }

    public void UpdateHealth(int healthValue, int maxHealthValue)
    {
        health.text = healthValue.ToString();
        targetHealthFill = (float)healthValue / (float)maxHealthValue;
    }

    public void UpdateArmor(int armorValue)
    {
        armor.text = armorValue.ToString() + "%";
    }

    public void UpdateAmmo(int ammoValue)
    {
        
    }
    
    public void UpdateHealthIndicator(int healthValue)
    {
        
    }
}