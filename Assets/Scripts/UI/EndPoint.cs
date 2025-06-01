using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


[RequireComponent(typeof(Animator))]
public class EndPoint : MonoBehaviour
{
    [Header("Level Complete UI")]
    public GameObject levelCompleteUI; // Assign in Inspector

    [Header("Grow Animation")]
    public float growDuration = 0.5f;
    public Vector3 finalScale = Vector3.one;

    private bool triggered = false;
    private Animator anim;

    void Awake()
    {
        transform.localScale = Vector3.zero;
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        StartCoroutine(GrowEffect());
    }

    IEnumerator GrowEffect()
    {
        float t = 0f;
        Vector3 startScale = Vector3.zero;

        while (t < growDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, finalScale, t / growDuration);
            yield return null;
        }

        transform.localScale = finalScale;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            ShowLevelCompleteUI();
        }
    }

    void ShowLevelCompleteUI()
    {
        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(true);
            Time.timeScale = 0f;

            var pauseManager = FindObjectOfType<PauseMenuManager>();
            if (pauseManager != null)
                pauseManager.enabled = false;
        }
        else
        {
            Debug.LogWarning("LevelCompleteUI is not assigned!");
        }
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
