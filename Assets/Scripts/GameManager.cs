using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /* ---------- Singleton boilerplate ---------- */
    public static GameManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    /* ---------- State ---------- */
    [Header("Player Stats")]
    [SerializeField] int maxHealth = 3;
    public int CurrentHealth { get; private set; }
    public int Lives { get; private set; } = 3;           // example extra state
    public int Coins { get; private set; }
  
    
    /* ---------- Public API ---------- */
    public void ResetRun()
    {
        CurrentHealth = maxHealth;
        Lives = 3;
        Coins = 0;
    }

    public void DamagePlayer(int dmg = 1)
    {
        if (CurrentHealth <= 0) return;                    // already dead

        CurrentHealth = Mathf.Max(CurrentHealth - dmg, 0);

        if (CurrentHealth == 0)
        {
            Lives--;

            if (Lives > 0)
                CurrentHealth = maxHealth;                 // respawn
        }
    }

    public void HealPlayer(int amount = 1)
    {
        if (CurrentHealth == maxHealth) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
    }

    /* ---------- Example initialisation ---------- */
    void Start() => ResetRun();
}
