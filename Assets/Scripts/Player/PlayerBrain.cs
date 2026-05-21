using UnityEngine;
using UnityEngine.InputSystem; // Will need to install input system from Package Manager

public class PlayerBrain : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PlayerProfileSO profile;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private PlayerSenses senses;

    [Header("State")]
    [SerializeField] private PlayerTraversalState currentState = PlayerTraversalState.Idle;

    [Header("Input Readout")]
    [SerializeField] private Vector2 moveInput;
    [SerializeField] private bool jumpPressed;
    [SerializeField] private bool canDoubleJump;

    [Header("Velocity Readout")]
    [SerializeField] private Vector3 currentHorizontalVelocity;
    [SerializeField] private float verticalVelocity;

    [SerializeField] private Vector2 lookInput;
    [SerializeField] private ThirdPersonCamera followCamera;
    [SerializeField] private bool isBouncing;
    [SerializeField] public float jumpSpeed;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector3 dashDirection;
   
    
    public void OnDash(InputValue value)
    {
        if  (value.isPressed && dashCooldownTimer <= 0f)
        {
            StartDash();
        }
    }
   
    private void StartDash()
    {
        
        verticalVelocity = 0f;
        isDashing = true;
        dashTimer = dashTime;
        dashCooldownTimer = dashCooldown;

        Vector3 moveDir = GetCameraRelativeMoveDirection();

        dashDirection = moveDir.sqrMagnitude > 0.01f
         ? moveDir
        : transform.forward;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("JumpPad"))
        {
            isBouncing = true;
        }
        
        if (collision.gameObject.CompareTag("box"))
        {
            
            rb.velocity = new Vector3(rb.velocity.x, 10f, rb.velocity.z);
            currentState = PlayerTraversalState.Jump;
      
        }
    }
   
    

    private void HandleJumpPad()
    {
        verticalVelocity = jumpSpeed;

        isBouncing = false;

        currentState = PlayerTraversalState.Jump;
    }
    
    
    
    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (senses == null) senses = GetComponent<PlayerSenses>();

        if (rb != null)
        {
            rb.freezeRotation = true;
        }

        isBouncing = false;
    }

    private void Update()
    {
        if (profile == null || senses == null) return;

        senses.RunChecks(profile);

        
        
        if (senses.IsGrounded)
        {
            canDoubleJump = true;
        }
    
        UpdateState();

        if (jumpPressed)
        {
            TryJump();
        }
            if (dashCooldownTimer > 0f)
        dashCooldownTimer -= Time.deltaTime;

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (profile == null || rb == null) return;

        HandleMovement();
        ApplyFinalVelocity();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpPressed = true;
        }
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    private void LateUpdate()
    {
        if (followCamera != null)
        {
            followCamera.SetLookInput(lookInput);
        }
    }

   private void UpdateState()
    {
        if (isDashing)
        return;
    
        if (isBouncing)
        {
            currentState = PlayerTraversalState.JumpPad;
            return;
        }

        if (!senses.IsGrounded || verticalVelocity > 0.01f)
        {
            currentState = PlayerTraversalState.Jump;
            return;
        }

        if (moveInput.sqrMagnitude > 0.01f)
        {
            currentState = PlayerTraversalState.Walk;
        }
        else
        {
            currentState = PlayerTraversalState.Idle;
        }
    }

    private void HandleMovement()
    {
        
       if (isDashing)
       {
        currentState = PlayerTraversalState.Jump; // or create Dash state

        currentHorizontalVelocity = dashDirection * dashSpeed;
        return;
       }
    
    
        switch (currentState)
        {
            case PlayerTraversalState.Idle:
                HandleIdle();
                break;

            case PlayerTraversalState.Walk:
                HandleWalk();
                break;

            case PlayerTraversalState.Jump:
                HandleJump();
                break;

            case PlayerTraversalState.JumpPad:
                HandleJumpPad();
                break;

           


          
            // Add a new case for each new state, and then run a method for that sate (probably all your old jump pad code can go in that state)
        }
    }

   

    private void HandleIdle()
    {
        if (isDashing) return;
        currentHorizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity,
            Vector3.zero,
            profile.groundDeceleration * Time.fixedDeltaTime
        );

        if (senses.IsGrounded)
        {
            verticalVelocity = -2f;
        }
    }

    private void HandleWalk()
    {
        if (isDashing) return;
        Vector3 moveDirection = GetCameraRelativeMoveDirection();
        Vector3 targetVelocity = moveDirection * profile.walkSpeed;

        currentHorizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity,
            targetVelocity,
            profile.groundAcceleration * Time.fixedDeltaTime
        );

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            transform.forward = moveDirection;
        }

        if (senses.IsGrounded)
        {
            verticalVelocity = -2f;
        }
    }
    
    private void HandleJump()
    {
        if (isDashing) return;
        Vector3 moveDirection = GetCameraRelativeMoveDirection();
        Vector3 targetVelocity = moveDirection * profile.maxAirSpeed;

        currentHorizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity,
            targetVelocity,
            profile.airAcceleration * Time.fixedDeltaTime
        );

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            transform.forward = moveDirection;
        }

        ApplyGravity();
    }

        private void TryJump()
    {
        if (!jumpPressed) return;

        // First jump (ground)
        if (senses.IsGrounded)
        {
            verticalVelocity = profile.jumpForce;
            currentState = PlayerTraversalState.Jump;
            canDoubleJump = true;
        }
        // Second jump (air)
        else if (canDoubleJump)
        {
            verticalVelocity = profile.jumpForce;
            currentState = PlayerTraversalState.Jump;
            canDoubleJump = false;
        }

            jumpPressed = false;
    }

    private Vector3 GetCameraRelativeMoveDirection()
    {
        if (cameraTransform == null)
        {
            return new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        }

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

            cameraForward =
        Vector3.ProjectOnPlane(cameraForward, transform.up);

            cameraRight =
        Vector3.ProjectOnPlane(cameraRight, transform.up);

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * moveInput.y) + (cameraRight * moveInput.x);

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        return moveDirection;
    }

    private void ApplyGravity()
    {
        if (isDashing) return;
        verticalVelocity += profile.gravity * Time.fixedDeltaTime;
        verticalVelocity = Mathf.Max(verticalVelocity, profile.maxFallSpeed);
    }

    private void ApplyFinalVelocity()
    {
        if (senses.IsGrounded && verticalVelocity < 0f && currentState != PlayerTraversalState.Jump)
        {
            verticalVelocity = -2f;
        }

        Vector3 finalVelocity = currentHorizontalVelocity;
        finalVelocity.y = verticalVelocity;

        rb.velocity = finalVelocity;
    }
}