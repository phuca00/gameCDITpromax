using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    private float horizontal;

    [Header("Move & Jump")]
    public float speed = 4f;
    public float jumpingPower = 8f;

    [Header("Refs")]
    public Rigidbody2D rb;
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    [Header("Check Settings")]
    public float groundCheckRadius = 0.18f;
    public float wallCheckRadius = 0.2f;

    [Header("Jump Settings")]
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    private float coyoteCounter;
    private float jumpBufferCounter;

    private bool canDoubleJump;

    [Header("Wall")]
    public float wallSlideSpeed = 2f;
    private bool isWallSliding;

    [Header("Wall Jump")]
    public Vector2 wallJumpPower = new Vector2(5f, 10f);
    private bool isWallJumping;

    [Header("Animation")]
    public Animator animator;
    private static readonly int stateHash = Animator.StringToHash("state");

    enum State { idle, run, jump, fall, wall }

    // NETCODE
    private NetworkVariable<int> netState = new NetworkVariable<int>();
    private NetworkVariable<float> netScale = new NetworkVariable<float>();

    private void Update()
    {
        Debug.Log($"---Check Isowner: {IsOwner}");
        // 👉 CLIENT KHÁC
        if (!IsOwner)
        {
            animator.SetInteger(stateHash, netState.Value);

            Vector3 scale = transform.localScale;
            scale.x = netScale.Value;
            transform.localScale = scale;
            return;
        }

        // 👉 LOCAL PLAYER
        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        HandleJump();
        Debug.Log("---HandleWallSlide---");
        HandleWallSlide();
        Flip();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        bool grounded = IsGrounded();

        if (grounded)
        {
            coyoteCounter = coyoteTime;
            canDoubleJump = true;
        }
        else
        {
            coyoteCounter -= Time.fixedDeltaTime;
        }

        if (!isWallJumping)
        {
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        }
    }

    // ---------------- GROUND ----------------
    bool IsGrounded()
    {
        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    // ---------------- WALL ----------------
    bool IsWalled()
    {
        return Physics2D.OverlapCircle(
            wallCheck.position,
            wallCheckRadius,
            wallLayer
        );
    }

    // ---------------- JUMP ----------------
    void HandleJump()
    {
        if (jumpBufferCounter <= 0) return;

        // nhảy đất
        if (coyoteCounter > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            jumpBufferCounter = 0;
            return;
        }

        // double jump
        if (canDoubleJump)
        {
            canDoubleJump = false;
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            jumpBufferCounter = 0;
            return;
        }

        // wall jump
        if (IsWalled() && !IsGrounded())
        {
            isWallJumping = true;

            float dir = -transform.localScale.x;

            rb.velocity = new Vector2(dir * wallJumpPower.x, wallJumpPower.y);

            Invoke(nameof(StopWallJump), 0.2f);

            jumpBufferCounter = 0;
        }
    }

    void StopWallJump()
    {
        isWallJumping = false;
    }

    // ---------------- WALL SLIDE ----------------
    void HandleWallSlide()
    {
        Debug.Log("---Check hanldewallslide---");
        if (IsWalled() && !IsGrounded() && horizontal != 0)
        {
            isWallSliding = true;

            rb.velocity = new Vector2(
                rb.velocity.x,
                Mathf.Clamp(rb.velocity.y, -wallSlideSpeed, float.MaxValue)
            );
        }
        else
        {
            isWallSliding = false;
        }
    }

    // ---------------- FLIP ----------------
    void Flip()
    {
        if (horizontal > 0)
            transform.localScale = new Vector3(1, 1, 1);

        if (horizontal < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    // ---------------- ANIMATION ----------------
    void UpdateAnimation()
    {
        if (!IsOwner) return;

        State state;

        if (IsGrounded())
        {
            state = horizontal == 0 ? State.idle : State.run;
        }
        else if (isWallSliding)
        {
            state = State.wall;
        }
        else if (rb.velocity.y > 0)
        {
            state = State.jump;
        }
        else
        {
            state = State.fall;
        }

        animator.SetInteger(stateHash, (int)state);

        SendAnimServerRpc((int)state, transform.localScale.x);
    }

    [ServerRpc]
    void SendAnimServerRpc(int state, float scale)
    {
        netState.Value = state;
        netScale.Value = scale;
    }
}