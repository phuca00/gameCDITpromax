using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class PlayerNetwork : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float speed = 4f;
    public float jumpingPower = 8f;
    public float wallSlidingSpeed = 2f;
    public Vector2 wallJumpPower = new Vector2(5f, 10f);
    public float wallJumpDuration = 0.2f;

    [Header("Components")]
    public Rigidbody2D rb;
    public Animator animator;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public Transform wallCheck;
    public LayerMask wallLayer;

    // --- BIẾN LOGIC NỘI BỘ ---
    private float horizontal;
    private bool isFacingRight = true; 
    private bool isWallSliding; 
    private bool isWallJumping;
    private float wallJumpDirection;
    private bool canDoubleJump;
    
    private bool isDoingDoubleJump; 
    
    private float coyoteTime = 0.15f;
    private float coyoteTimeCounter;
    private float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    private int lastSentState = -1;

    private static readonly int stateHash = Animator.StringToHash("state");

    // --- BIẾN MẠNG ---
    private NetworkVariable<int> netState = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netScaleX = new NetworkVariable<float>(1.4f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Score System")]
    public NetworkVariable<int> playerScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.simulated = true; 
        
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (IsOwner)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 3f; 
            rb.interpolation = RigidbodyInterpolation2D.Interpolate; 
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Kinematic; 
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero; 
            rb.interpolation = RigidbodyInterpolation2D.Interpolate; 
        }

        // Đăng ký sự kiện: Cứ mỗi khi đổi màn hình, gọi hàm OnSceneChange
        SceneManager.sceneLoaded += OnSceneChange;
    }

    public override void OnNetworkDespawn()
    {
        // Gỡ sự kiện khi nhân vật bị xóa để tránh lỗi bộ nhớ
        SceneManager.sceneLoaded -= OnSceneChange;
    }

    private void Update()
    {
        if (!IsOwner)
        {
            if (animator != null) animator.SetInteger(stateHash, netState.Value);
            transform.localScale = new Vector3(netScaleX.Value, 1.4f, 1.4f);
            return; 
        }

        // Nếu có script Pause làm khóa phím thì bỏ qua đoạn này nếu ông đang không dùng
        // if (Pause.inputLocked) { horizontal = 0; return; }
        horizontal = Input.GetAxisRaw("Horizontal");

        if (IsGrounded()) 
        {
            coyoteTimeCounter = coyoteTime;
            isDoingDoubleJump = false; 
        }
        else 
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f)
        {
            if (coyoteTimeCounter > 0f) 
            {
                jumpBufferCounter = 0f; 
                coyoteTimeCounter = 0f; 
                rb.velocity = new Vector2(rb.velocity.x, jumpingPower); 
                canDoubleJump = true;
                isDoingDoubleJump = false;
            }
            else if (isWallSliding) 
            {
                jumpBufferCounter = 0f; 
                isWallJumping = true;
                wallJumpDirection = isFacingRight ? -1f : 1f;
                rb.velocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
                Invoke(nameof(StopWallJumping), wallJumpDuration);
                isDoingDoubleJump = false; 
            }
            else if (canDoubleJump) 
            {
                jumpBufferCounter = 0f; 
                canDoubleJump = false;
                rb.velocity = new Vector2(rb.velocity.x, jumpingPower * 0.85f);
                isDoingDoubleJump = true; 
            }
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0.1f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            coyoteTimeCounter = 0f;
        }

        WallSlideLogic(); 
        Flip();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (!IsOwner || isWallJumping) return;
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
    }

    private void StopWallJumping() => isWallJumping = false;

    private void WallSlideLogic()
    {
        if (IsWalled() && !IsGrounded() && Mathf.Abs(horizontal) > 0.1f)
        {
            isWallSliding = true;
            isDoingDoubleJump = false; 
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void UpdateAnimation()
    {
        int s = 0; 
        
        if (isWallSliding) 
        {
            s = 5; 
        }
        else if (!IsGrounded()) 
        {
            if (isDoingDoubleJump) 
            {
                s = 4; 
                if (rb.velocity.y < 0f) isDoingDoubleJump = false;
            }
            else 
            {
                s = rb.velocity.y > 0.1f ? 2 : 3; 
            }
        }
        else 
        {
            s = Mathf.Abs(horizontal) > 0.1f ? 1 : 0; 
        }

        animator.SetInteger(stateHash, s);

        if (s != lastSentState)
        {
            lastSentState = s;
            UpdateAnimServerRpc(s);
        }
    }

    [ServerRpc] 
    void UpdateAnimServerRpc(int s) => netState.Value = s;

    private void Flip()
    {
        if (IsWalled() || isWallJumping) return; 

        if (horizontal > 0.1f && !isFacingRight) ExecuteFlip(true);
        else if (horizontal < -0.1f && isFacingRight) ExecuteFlip(false);
    }

    private void ExecuteFlip(bool right)
    {
        isFacingRight = right; 
        float newScale = right ? 1.4f : -1.4f;

        transform.localScale = new Vector3(newScale, 1.4f, 1.4f);
        UpdateScaleServerRpc(newScale);
    }

    [ServerRpc] 
    void UpdateScaleServerRpc(float s) => netScaleX.Value = s;

    private bool IsGrounded() => Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    private bool IsWalled() => Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);

    public void AddScore(int amount)
    {
        if (IsServer) playerScore.Value += amount;
    }

    // --- LOGIC TỰ ĐỘNG SANG MÀN MỚI ---
    void OnSceneChange(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name.ToLower().Contains("level"))
        {
            // 1. Mở khóa cho người chơi điều khiển lại
            this.enabled = true;
            if (rb != null) rb.simulated = true;

            // 2. Tự động chia vị trí (Chỉ Server mới dịch chuyển)
            if (IsServer)
            {
                int pointIndex = (int)OwnerClientId + 1; 
                string targetPointName = "point" + pointIndex;

                GameObject spawnPoint = GameObject.Find(targetPointName);
                Vector3 newPosition = Vector3.zero;
                
                if (spawnPoint != null)
                {
                    newPosition = spawnPoint.transform.position;
                    Debug.Log($"[Client {OwnerClientId}] Đang đáp xuống: {targetPointName}");
                }
                else
                {
                    // Lỗi kẹt tường hay do nó nhảy vào dòng Backup này (Tọa độ 0,0,0)
                    GameObject backupPoint = GameObject.Find("point1");
                    newPosition = (backupPoint != null) ? backupPoint.transform.position : Vector3.zero; 
                    Debug.LogWarning($"[Client {OwnerClientId}] Không thấy {targetPointName}, vứt tạm vào point1");
                }

                // ÉP TRỤC Z = 0 (Sửa lỗi nhân vật bị kẹt ra sau background 2D)
                newPosition.z = 0f;
                transform.position = newPosition;

                // BÓP PHANH QUÁN TÍNH: Xóa hết lực bay/rơi từ màn trước để không bị trôi xuyên tường
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
            }
        }
    }
}