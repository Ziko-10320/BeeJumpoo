// CameraFollow.cs (FINAL UPGRADE - DUAL INPUT PANNING)
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [Range(0.01f, 1.0f)]
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 baseOffset = new Vector3(0, 0, -10);

    [Header("Axis Locking")]
    [SerializeField] private bool lockXAxis = false;

    [Header("Lookahead Settings")]
    [Tooltip("Assign the Player object here so the camera can check if it's grappling.")]

    [Header("State Offsets")]
    [SerializeField] private float glideYOffset = -2f;     // how far down when gliding
    [SerializeField] private float fallYOffset = -4f;      // how far down when stunned falling
    [SerializeField] private float offsetSmoothSpeed = 3f; // how fast offset transitions

    private float currentYOffset = 0f;
    [SerializeField] private PlayerController player;



    // --- Private State Variables ---

    private float holdTimer = 0f;
    private Vector2 panDirection = Vector2.zero;
    

   
    void Start()
    {
    
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollow script has no target assigned!");
            return;
        }

        // --- 1. FIGURE OUT TARGET Y OFFSET BASED ON PLAYER STATE ---
        float targetYOffset = 0f;

        if (player != null)
        {
            if (!player.canJump && !player.isGrounded)
                targetYOffset = fallYOffset;       // stunned falling
            else if (player.canJump && !player.isGrounded
                     && player.rb.linearVelocity.y < 0f)
                targetYOffset = glideYOffset;      // gliding down
        }

        // Smoothly lerp toward the target offset
        currentYOffset = Mathf.Lerp(currentYOffset, targetYOffset,
                                    offsetSmoothSpeed * Time.deltaTime);

        // --- 2. CALCULATE DESIRED POSITION ---
        Vector3 desiredPosition = new Vector3(
            target.position.x + baseOffset.x,
            target.position.y + baseOffset.y + currentYOffset,
            baseOffset.z);

        // --- 3. APPLY X-AXIS LOCK (IF ENABLED) ---
        if (lockXAxis)
            desiredPosition.x = transform.position.x;

        // --- 4. SMOOTHLY MOVE THE CAMERA ---
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
    }

    // --- THIS IS THE NEW, UNIFIED FUNCTION ---


    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }


}
