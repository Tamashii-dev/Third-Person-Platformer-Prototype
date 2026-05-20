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

    [Header("Velocity Readout")]
    [SerializeField] private Vector3 currentHorizontalVelocity;
    [SerializeField] private float verticalVelocity;

    [SerializeField] private Vector2 lookInput;
    [SerializeField] private ThirdPersonCamera followCamera;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (senses == null) senses = GetComponent<PlayerSenses>();

        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    private void Update()
    {
        if (profile == null || senses == null) return;

        senses.RunChecks(profile);

        UpdateState();

        if (jumpPressed)
        {
            TryJump();
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
        }
    }

    private void HandleIdle()
    {
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

        if (senses.IsGrounded)
        {
            verticalVelocity = profile.jumpForce;
            currentState = PlayerTraversalState.Jump;
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

        cameraForward.y = 0f;
        cameraRight.y = 0f;

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