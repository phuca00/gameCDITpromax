using UnityEngine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    private float horizontal;

    [Header("Move & Jump")]
    public float speed = 4f;
    public float jumpingPower = 8f;

    [Header("Rigidbody & Checks")]
    public Rigidbody2D rb;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public Transform wallCheck;
    public LayerMask wallLayer;

    [Header("Ground Check Settings")]
    public float groundCheckRadius = 0.18f;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    private float coyoteTimeCounter = 0f;
    private float jumpBufferCounter = 0f;

    [Header("Jump Counters")]
    private bool canDoubleJump = false;
    private int wallJumpCount = 0;
    public int maxWallJumps = 2;

    [Header("Wall Sliding")]
    private bool isWallSliding = false;
    public float wallSlidingSpeed = 2f;

    [Header("Wall Jump")]
    private bool isWallJumping = false;
    public float wallJumpDirection;
    public float wallJumpDuration = 0.2f;
    public Vector2 wallJumpPower = new Vector2(5f, 10f);

    [Header("Animation")]
    public Animator animator;
    private static readonly int stateHash = Animator.StringToHash("state");

    private enum State { idle, running, jumping, falling, doubleJump, wallSlide }

    private bool isGroundedCached = false;

    // 🔥 NETCODE SYNC
    private NetworkVariable<int> netState = new NetworkVariable<int>();
    private NetworkVariable<float> netScaleX = new NetworkVariable<float>();

    private void Update()
    {
        Debug.Log($"--- Check Isowner: {IsOwner}");
        // 👉 PLAYER KHÁC → chỉ nhận data
        if (!IsOwner)
        {
            animator.SetInteger(stateHash, netState.Value);
            Debug.Log($"=== Check state: {stateHash}  {netState.Value} ===");

            Vector3 scale = transform.localScale;
            scale.x = netScaleX.Value;
            transform.localScale = scale;

            return;
        }

        // 👉 PLAYER LOCAL
        if (Pause.inputLocked)
        {
            Debug.Log($"Check inputLocked: {Pause.inputLocked}");
            horizontal = 0;
            return;
        }

        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.55f);

        TryConsumeJumpBuffer();

        WallSlide();
        Flip();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        CheckGround();

        if (!isWallJumping)
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);

        if (isGroundedCached)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.fixedDeltaTime;
    }

    private void CheckGround()
    {
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (grounded && !isGroundedCached)
        {
            canDoubleJump = true;
            wallJumpCount = 0;
        }

        isGroundedCached = grounded;
    }

    private bool IsGrounded()
    {
        return isGroundedCached || coyoteTimeCounter > 0f;
    }

    private bool IsWalled()
    {
        Collider2D hit = Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
        if(hit != null)
        {
            Debug.Log("Wall detected: " + hit.name);
            return true;
        }
        else
        {
            Debug.Log("No wall detected at: " + wallCheck.position);
            return false;
        }
    }

    private void TryConsumeJumpBuffer()
    {
        if (jumpBufferCounter > 0f)
        {
            if (IsGrounded())
            {
                DoJump();
                jumpBufferCounter = 0f;
                return;
            }

            if (IsWalled() && wallJumpCount < maxWallJumps)
            {
                wallJumpCount++;
                StartWallJump();
                jumpBufferCounter = 0f;
                return;
            }

            if (canDoubleJump && !IsGrounded())
            {
                canDoubleJump = false;
                rb.velocity = new Vector2(rb.velocity.x, jumpingPower * 0.9f);
                AudioManager.instance.PlayJump();
                jumpBufferCounter = 0f;
                return;
            }
        }
    }

    private void DoJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        AudioManager.instance.PlayJump();
        coyoteTimeCounter = 0f;
    }

    private void WallSlide()
    {
        bool touchingWall = IsWalled();
        Debug.Log($"--- Check wall sliding: {IsWalled()}");
        bool pushingTowardsWall =
            (horizontal > 0 && transform.localScale.x > 0 && touchingWall) ||
            (horizontal < 0 && transform.localScale.x < 0 && touchingWall);

        if (touchingWall && !IsGrounded() && (Mathf.Abs(horizontal) > 0f || pushingTowardsWall))
        {
            isWallSliding = true;

            rb.velocity = new Vector2(rb.velocity.x,
                Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void StartWallJump()
    {
        isWallSliding = false;
        isWallJumping = true;

        wallJumpDirection = -transform.localScale.x;

        rb.velocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);

        transform.localScale = new Vector3(wallJumpDirection, 1, 1);

        Invoke(nameof(StopWallJump), wallJumpDuration);
    }

    private void StopWallJump()
    {
        isWallJumping = false;
    }

    private void Flip()
    {
        if (isWallJumping) return;

        if (horizontal > 0) transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
        if (horizontal < 0) transform.localScale = new Vector3(-1.4f, 1.4f, 1.4f);
    }

    private void UpdateAnimation()
    {
        if (!IsOwner) return;

        State state;

        if (IsGrounded())
        {
            Debug.Log("--Check: is grounded");
            state = Mathf.Abs(horizontal) > 0.1f ? State.running : State.idle;

            animator.SetInteger(stateHash, (int)state);
            Debug.Log($"=== Check state: {stateHash}  ===");

            SendAnimServerRpc((int)state, transform.localScale.x);
            return;
        }

        if (isWallSliding)
            state = State.wallSlide;
        else if (rb.velocity.y > 0.1f)
            state = canDoubleJump ? State.jumping : State.doubleJump;
        else if (rb.velocity.y < -0.1f)
            state = State.falling;
        else
            state = State.idle;

        animator.SetInteger(stateHash, (int)state);
        Debug.Log($"=== Check state: {stateHash}  ===");

        SendAnimServerRpc((int)state, transform.localScale.x);
    }

    [ServerRpc]
    void SendAnimServerRpc(int state, float scaleX)
    {
        netState.Value = state;
        netScaleX.Value = scaleX;
    }
}