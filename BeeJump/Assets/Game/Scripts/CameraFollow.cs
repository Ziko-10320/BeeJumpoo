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

        // --- 1. HANDLE ALL CAMERA PANNING LOGIC ---


        // --- 2. CALCULATE DESIRED POSITION ---
        Vector3 desiredPosition = new Vector3(
      target.position.x + baseOffset.x,
      target.position.y + baseOffset.y,
      baseOffset.z);
        // --- 3. APPLY Y-AXIS LOCK (IF ENABLED) ---
        if (lockXAxis)
        {
            desiredPosition.x = transform.position.x;
        }

        // --- 4. SMOOTHLY MOVE THE CAMERA ---
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // --- 5. APPLY THE FINAL POSITION ---
        transform.position = smoothedPosition;
    }

    // --- THIS IS THE NEW, UNIFIED FUNCTION ---
   

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }


}
