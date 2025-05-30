using System;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    /* ---------- State ---------- */
    [Header("Player Stats")]
    [SerializeField] int maxHealth = 3;
    public int CurrentHealth { get; private set; }
    public int Lives { get; private set; } = 3;           // example extra state
    public int Coins { get; private set; }
    
    // Events
    public static event Action OnPlayerDeath;
  
    
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
        
        Debug.Log($"Player health: {CurrentHealth}");

        if (CurrentHealth == 0)
        {
           // game over
           OnPlayerDeath?.Invoke();
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetRun();
    }
    
    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /* ---------- Example initialisation ---------- */
    void Start() => ResetRun();
}
