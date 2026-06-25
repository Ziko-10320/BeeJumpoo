using UnityEngine;

public class BossDoorTrigger : MonoBehaviour
{
    [Header("Boss Setup")]
    public BossRoomManager bossRoomManager; // Links to your boss manager script

    [Header("Door Animation Setup")]
    public Animator doorAnimator;           // Drag your Door object here in Unity
    public string doorTriggerName = "Close"; // Type your Animator's trigger name here

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // If the door was already triggered, don't do anything else
        if (triggered) return;

        // Check if the object that stepped into the box is actually the player
        PlayerController player = other.GetComponent<PlayerController>();

        // If it's NOT the player (like an enemy or a stray projectile), stop right here
        if (player == null) return;

        // --- SUCCESS: The player has entered! ---
        triggered = true;

        // 1. Tell the boss manager to lock the room down and start the fight
        if (bossRoomManager != null)
        {
            bossRoomManager.StartBossFight();
        }
        else
        {
            Debug.LogError("Boss Room Manager is missing from the script slot!");
        }

        // 2. Tell the door's animator to play the closing animation
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(doorTriggerName);
        }
        else
        {
            Debug.LogWarning("Door Animator is missing from the script slot! The door won't visually close.");
        }
    }
}