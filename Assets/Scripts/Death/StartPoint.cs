using UnityEngine;

public class StartPoint : Checkpoint
{
    void Start()
    {
        // If no checkpoint has been activated yet, become the active checkpoint
        if (Checkpoint.activeCheckpoint == null)
        {
            SetAsActive();

            // If no player exists in the scene, spawn one at this StartPoint
            if (FindObjectOfType<AdvancedPlayerMovement>() == null)
            {
                SpawnPlayer();
            }
        }
    }
}
