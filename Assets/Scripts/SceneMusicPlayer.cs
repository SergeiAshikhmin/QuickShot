using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class SceneMusicPlayer : MonoBehaviour
{
    [System.Serializable]
    public class SceneTrackGroup
    {
        public string sceneName;
        public List<AudioClip> tracks;
    }

    [Header("Scene-Based Music Sets")]
    public List<SceneTrackGroup> sceneTracks = new();

    [Header("Options")]
    public bool shuffle = false;

    private AudioSource audioSource;
    private List<AudioClip> currentQueue = new();
    private int currentIndex = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        string currentScene = SceneManager.GetActiveScene().name;
        SceneTrackGroup group = sceneTracks.Find(g => g.sceneName == currentScene);

        if (group == null || group.tracks == null || group.tracks.Count == 0)
        {
            Debug.LogWarning($"No tracks assigned for scene '{currentScene}'.");
            return;
        }

        BuildQueue(group.tracks);
        PlayCurrent();
    }

    void Update()
    {
        if (!audioSource.isPlaying && currentQueue.Count > 0)
        {
            NextTrack();
        }
    }

    void BuildQueue(List<AudioClip> sourceTracks)
    {
        currentQueue = new List<AudioClip>(sourceTracks);

        if (shuffle)
        {
            for (int i = 0; i < currentQueue.Count; i++)
            {
                int rand = Random.Range(i, currentQueue.Count);
                (currentQueue[i], currentQueue[rand]) = (currentQueue[rand], currentQueue[i]);
            }
        }

        currentIndex = 0;
    }

    void PlayCurrent()
    {
        if (currentQueue.Count == 0) return;

        audioSource.clip = currentQueue[currentIndex];
        audioSource.Play();
    }

    void NextTrack()
    {
        currentIndex = (currentIndex + 1) % currentQueue.Count;
        PlayCurrent();
    }
}
