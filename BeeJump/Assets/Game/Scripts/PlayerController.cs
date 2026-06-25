using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    [Header("Glide")]
    public float glideGravityScale = 0.3f;
    [Header("Jump")]
    public float jumpForce = 12f;
    [Header("Stun")]
    public float groundedStunDuration = 0.5f;
    [Header("Gravity")]
    public float gravityScale = 1.2f;
    public float fallMultiplier = 1.5f;

    [Header("Slope")]
    public float slideFriction = 0.2f;
    public float slopeAngleThreshold = 15f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;
    private bool isOnSlopeTrigger = false;
    // ?? state ??????????????????????????????????????????????????????????????????
    public Rigidbody2D rb;
    private int direction = -1;     // starts moving left on first jump
    public bool canJump = true;
    public bool isGrounded;
    public bool isSliding;
    private bool wasGrounded;       // to detect the moment of landing
    private Vector2 slopeNormal = Vector2.up;
    private bool isStunned = false;
    private bool isJumping = false;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;       // we handle gravity manually
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            Debug.Log($"JUMP PRESSED — canJump={canJump} isGrounded={isGrounded} isStunned={isStunned}");

        if (Keyboard.current.spaceKey.wasPressedThisFrame && canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
            ExitSlope();
        }
    }

    void FixedUpdate()
    {
        isJumping = false;
        bool wasGroundedThisFrame = Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, groundLayer) != null
            && !isOnSlopeTrigger;

        bool justLanded = !isGrounded && wasGroundedThisFrame;

        isGrounded = wasGroundedThisFrame;
        isSliding = isOnSlopeTrigger;

        // Only trigger recovery the exact frame player lands
        if (justLanded && !canJump && !isStunned)
            StartCoroutine(StunRecovery());
        else if (justLanded && isStunned)
            StartCoroutine(StunRecovery());

        ApplyHorizontalMovement();
        ApplyGravity();
    }
    private IEnumerator StunRecovery()
    {
        yield return new WaitForSeconds(groundedStunDuration);
        canJump = true;
        isStunned = false;
    }
    // ?? ground detection ???????????????????????????????????????????????????????
    void CheckGrounded()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, groundLayer);

        isGrounded = hit != null && !isOnSlopeTrigger;
        isSliding = isOnSlopeTrigger;
        if (isSliding && !isStunned)
            canJump = true;
    }

    // ?? horizontal movement ????????????????????????????????????????????????????
    void ApplyHorizontalMovement()
    {
        if (isSliding)
        {
            rb.linearVelocity = slopeNormal * moveSpeed;
            Debug.Log($"STATE: Sliding | dir={slopeNormal}");
            return;
        }

        if (isGrounded && rb.linearVelocity.y <= 0.1f)
        {
            // Only kill horizontal — never touch Y so jump velocity survives
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            Debug.Log("STATE: Grounded");
            return;
        }

        // Airborne
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (rb.linearVelocity.y < -0.1f && !canJump)
            Debug.Log("STATE: Falling (stunned)");
        else if (rb.linearVelocity.y < -0.1f)
            Debug.Log("STATE: Gliding");
        else if (rb.linearVelocity.y > 0.1f)
            Debug.Log("STATE: Rising");
    }

    // ?? gravity ????????????????????????????????????????????????????????????????
    void ApplyGravity()
    {
        if (isSliding) return;

        // Only zero velocity when truly sitting still on ground
        if (isGrounded && rb.linearVelocity.y <= 0f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float grav = Physics2D.gravity.y * gravityScale;

        if (!canJump && rb.linearVelocity.y < 0f)
            grav *= fallMultiplier;
        else if (canJump && rb.linearVelocity.y < 0f)
            grav = Physics2D.gravity.y * glideGravityScale;
        else if (rb.linearVelocity.y > 0f)
            grav *= 1.8f;

        rb.linearVelocity += Vector2.up * grav * Time.fixedDeltaTime;
    }

    // ?? wall collision ? flip direction ????????????????????????????????????????
    void OnCollisionEnter2D(Collision2D col)
    {
        foreach (ContactPoint2D contact in col.contacts)
        {
            float angle = Mathf.Abs(col.transform.eulerAngles.z);
            if (angle > 180f) angle = 360f - angle;

            if (Mathf.Abs(contact.normal.x) > 0.7f)
            {
                // Wall hit — flip and immediately push away from wall
                direction *= -1;
                // Force velocity away from wall, preserve Y completely
                rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
                if (contact.normal.y < -0.7f)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                    return;
                }
                return;
            }
        }
    }
    void OnCollisionStay2D(Collision2D col)
    {
        foreach (ContactPoint2D contact in col.contacts)
        {
            // Wall — push away
            if (Mathf.Abs(contact.normal.x) > 0.7f)
            {
                direction = contact.normal.x > 0f ? 1 : -1;
                rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
                return;
            }

            // Ceiling hit — kill upward velocity immediately so gravity pulls him down
            if (contact.normal.y < -0.7f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                return;
            }
        }
    }
    public void EnterSlope(Vector2 slideDirection)
    {
        isOnSlopeTrigger = true;
        slopeNormal = slideDirection.normalized;
        rb.linearVelocity = Vector2.zero;
        direction = slideDirection.x > 0f ? 1 : -1;

        // Never restore jump through slope if stunned
        if (!isStunned)
            canJump = true;
    }


    public void ExitSlope()
    {
        isOnSlopeTrigger = false;
        slopeNormal = Vector2.up;
    }
    // ?? called by hazards ??????????????????????????????????????????????????????
    public void OnHitHazard()
    {
        canJump = false;
        isStunned = true;
        StopAllCoroutines();
        BossRoomManager boss = FindObjectOfType<BossRoomManager>();
        if (boss != null)
        {
            boss.PlayerDied();
            return;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}