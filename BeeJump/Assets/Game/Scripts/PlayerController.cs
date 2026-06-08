using System.Security.Cryptography;
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

    // ?? state ??????????????????????????????????????????????????????????????????
    private Rigidbody2D rb;
    private int direction = -1;     // starts moving left on first jump
    private bool canJump = true;
    private bool isGrounded;
    private bool isSliding;
    private bool wasGrounded;       // to detect the moment of landing
    private Vector2 slopeNormal = Vector2.up;
    private bool isStunned = false;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;       // we handle gravity manually
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        // ?? jump input (new Input System) ??????????????????????????????????????
        if (Keyboard.current.spaceKey.wasPressedThisFrame && canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;     // leave ground immediately
        }
    }

    void FixedUpdate()
    {
        wasGrounded = isGrounded;
        CheckGrounded();

        // Regain jump the moment player touches ground after a stun
        if (isGrounded && !canJump && !isStunned)
            StartCoroutine(StunRecovery());

        ApplyHorizontalMovement();
        ApplyGravity();
    }
    private IEnumerator StunRecovery()
    {
        isStunned = true;
        Debug.Log($"STATE: Stunned on ground — recovering for {groundedStunDuration}s");
        yield return new WaitForSeconds(groundedStunDuration);
        canJump = true;
        isStunned = false;
        Debug.Log("STATE: Jump restored");
    }
    // ?? ground detection ???????????????????????????????????????????????????????
    void CheckGrounded()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, groundLayer);

        if (hit != null)
        {
            RaycastHit2D ray = Physics2D.Raycast(
                groundCheck.position, Vector2.down, 0.3f, groundLayer);
            slopeNormal = ray.collider != null ? ray.normal : Vector2.up;

            float slopeAngle = Vector2.Angle(slopeNormal, Vector2.up);

            if (slopeAngle > slopeAngleThreshold)
            {
                isGrounded = false;     // slope = NOT grounded in your definition
                isSliding = true;
            }
            else
            {
                isGrounded = true;      // flat floor = truly grounded, player stops
                isSliding = false;
            }
        }
        else
        {
            isGrounded = false;
            isSliding = false;
        }
    }

    // ?? horizontal movement ????????????????????????????????????????????????????
    void ApplyHorizontalMovement()
    {
        if (isGrounded)
        {
            // Flat ground — completely stop, player is "stuck"
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            Debug.Log("STATE: Grounded (stuck)");
        }
        else if (isSliding)
        {
            Vector2 slopeDir = new Vector2(slopeNormal.y, -slopeNormal.x);
            if (slopeDir.y > 0f) slopeDir = -slopeDir;

            // Push player into the slope surface to prevent bounce/jitter
            Vector2 intoSlope = -slopeNormal * 2f;

            rb.linearVelocity = Vector2.Lerp(
                rb.linearVelocity,
                (slopeDir * moveSpeed) + intoSlope,
                slideFriction
            );
            Debug.Log($"STATE: Sliding down | vel={rb.linearVelocity}");
        }
        else
        {
            // Airborne — move horizontally at full speed (gliding)
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

            if (rb.linearVelocity.y < -0.1f && !canJump)
                Debug.Log("STATE: Falling (stunned, no jump)");
            else if (rb.linearVelocity.y < -0.1f)
                Debug.Log("STATE: Gliding down");
            else if (rb.linearVelocity.y > 0.1f)
                Debug.Log("STATE: Rising");
        }
    }

    // ?? gravity ????????????????????????????????????????????????????????????????
    void ApplyGravity()
    {
        if (isGrounded && rb.linearVelocity.y <= 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            return;
        }

        float grav = Physics2D.gravity.y * gravityScale;

        if (!canJump && rb.linearVelocity.y < 0f)
        {
            // Stunned fall — drops fast
            grav *= fallMultiplier;
        }
        else if (rb.linearVelocity.y < 0f && canJump)
        {
            // Gliding down — still floaty but controlled
            grav = Physics2D.gravity.y * glideGravityScale;
        }
        else if (rb.linearVelocity.y > 0f)
        {
            // Rising after jump — apply extra gravity to cut the floaty rise
            // This makes the jump feel snappy and punchy instead of balloonlike
            grav *= 1.8f;   // expose this as a variable if you want to tune it
        }

        rb.linearVelocity += Vector2.up * grav * Time.fixedDeltaTime;
    }

    // ?? wall collision ? flip direction ????????????????????????????????????????
    void OnCollisionEnter2D(Collision2D col)
    {
        foreach (ContactPoint2D contact in col.contacts)
        {
            float angle = Vector2.Angle(contact.normal, Vector2.up);

            // Wall hit ? flip direction (your existing logic)
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                direction *= -1;
                rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
                return;
            }

            // Slope hit ? kill all velocity, immediately push down the slope
            if (angle > slopeAngleThreshold && angle < 85f)
            {
                Vector2 slopeDir = new Vector2(contact.normal.y, -contact.normal.x);
                if (slopeDir.y > 0f) slopeDir = -slopeDir;   // always downhill

                direction = slopeDir.x > 0f ? 1 : -1;
                rb.linearVelocity = Vector2.zero;
                isSliding = true;
                return;
            }
        }
    }

    // ?? called by hazards ??????????????????????????????????????????????????????
    public void OnHitHazard()
    {
        canJump = false;
        isStunned = false;   // reset so StunRecovery can trigger fresh on next landing
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}