using UnityEngine;

public class PlayerSenses : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Transform wallCheckPoint;

    [Header("Layers")]
    [SerializeField] private LayerMask terrainLayer;

    [Header("Ground Settings")]
    [SerializeField] private float groundCheckRadius = 0.25f;

    [Header("Ground Debug Readout")]
    [SerializeField] private bool isGrounded;

    [Header("Wall Debug Readouts")]
    [SerializeField] private bool touchingWallForward;
    [SerializeField] private bool touchingWallForwardRight;
    [SerializeField] private bool touchingWallRight;
    [SerializeField] private bool touchingWallBackRight;
    [SerializeField] private bool touchingWallBack;
    [SerializeField] private bool touchingWallBackLeft;
    [SerializeField] private bool touchingWallLeft;
    [SerializeField] private bool touchingWallForwardLeft;
    [SerializeField] private bool isTouchingAnyWall;

    public bool IsGrounded => isGrounded;

    public bool TouchingWallForward => touchingWallForward;
    public bool TouchingWallForwardRight => touchingWallForwardRight;
    public bool TouchingWallRight => touchingWallRight;
    public bool TouchingWallBackRight => touchingWallBackRight;
    public bool TouchingWallBack => touchingWallBack;
    public bool TouchingWallBackLeft => touchingWallBackLeft;
    public bool TouchingWallLeft => touchingWallLeft;
    public bool TouchingWallForwardLeft => touchingWallForwardLeft;
    public bool IsTouchingAnyWall => isTouchingAnyWall;

    public void RunChecks(PlayerProfileSO profile)
    {
        CheckGround(profile);
        CheckWalls(profile);
    }

    private void CheckGround(PlayerProfileSO profile)
    {
        if (groundCheckPoint == null)
        {
            isGrounded = false;
            return;
        }

        Vector3 origin = groundCheckPoint.position;
        Vector3 direction = Vector3.down;

        isGrounded = Physics.SphereCast(
            origin,
            groundCheckRadius,
            direction,
            out RaycastHit hit,
            profile.groundCheckDistance,
            terrainLayer,
            QueryTriggerInteraction.Ignore
        );

        Debug.DrawRay(
            origin,
            direction * profile.groundCheckDistance,
            isGrounded ? Color.green : Color.red
        );
    }

    private void CheckWalls(PlayerProfileSO profile)
    {
        if (wallCheckPoint == null)
        {
            touchingWallForward = false;
            touchingWallForwardRight = false;
            touchingWallRight = false;
            touchingWallBackRight = false;
            touchingWallBack = false;
            touchingWallBackLeft = false;
            touchingWallLeft = false;
            touchingWallForwardLeft = false;
            isTouchingAnyWall = false;
            return;
        }

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Vector3 forwardRight = (forward + right).normalized;
        Vector3 backRight = (-forward + right).normalized;
        Vector3 back = -forward;
        Vector3 backLeft = (-forward - right).normalized;
        Vector3 left = -right;
        Vector3 forwardLeft = (forward - right).normalized;

        touchingWallForward = CastWallRay(wallCheckPoint.position, forward, profile.wallCheckDistance);
        touchingWallForwardRight = CastWallRay(wallCheckPoint.position, forwardRight, profile.wallCheckDistance);
        touchingWallRight = CastWallRay(wallCheckPoint.position, right, profile.wallCheckDistance);
        touchingWallBackRight = CastWallRay(wallCheckPoint.position, backRight, profile.wallCheckDistance);
        touchingWallBack = CastWallRay(wallCheckPoint.position, back, profile.wallCheckDistance);
        touchingWallBackLeft = CastWallRay(wallCheckPoint.position, backLeft, profile.wallCheckDistance);
        touchingWallLeft = CastWallRay(wallCheckPoint.position, left, profile.wallCheckDistance);
        touchingWallForwardLeft = CastWallRay(wallCheckPoint.position, forwardLeft, profile.wallCheckDistance);

        isTouchingAnyWall =
            touchingWallForward ||
            touchingWallForwardRight ||
            touchingWallRight ||
            touchingWallBackRight ||
            touchingWallBack ||
            touchingWallBackLeft ||
            touchingWallLeft ||
            touchingWallForwardLeft;

        DrawWallDebugRay(wallCheckPoint.position, forward, profile.wallCheckDistance, touchingWallForward);
        DrawWallDebugRay(wallCheckPoint.position, forwardRight, profile.wallCheckDistance, touchingWallForwardRight);
        DrawWallDebugRay(wallCheckPoint.position, right, profile.wallCheckDistance, touchingWallRight);
        DrawWallDebugRay(wallCheckPoint.position, backRight, profile.wallCheckDistance, touchingWallBackRight);
        DrawWallDebugRay(wallCheckPoint.position, back, profile.wallCheckDistance, touchingWallBack);
        DrawWallDebugRay(wallCheckPoint.position, backLeft, profile.wallCheckDistance, touchingWallBackLeft);
        DrawWallDebugRay(wallCheckPoint.position, left, profile.wallCheckDistance, touchingWallLeft);
        DrawWallDebugRay(wallCheckPoint.position, forwardLeft, profile.wallCheckDistance, touchingWallForwardLeft);
    }

    private bool CastWallRay(Vector3 origin, Vector3 direction, float distance)
    {
        return Physics.Raycast(
            origin,
            direction,
            distance,
            terrainLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    private void DrawWallDebugRay(Vector3 origin, Vector3 direction, float distance, bool hitWall)
    {
        Debug.DrawRay(
            origin,
            direction * distance,
            hitWall ? Color.green : Color.yellow
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;

            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);

            Gizmos.DrawLine(
                groundCheckPoint.position,
                groundCheckPoint.position + (Vector3.down * 0.5f)
            );

            Gizmos.DrawWireSphere(
                groundCheckPoint.position + (Vector3.down * 0.5f),
                groundCheckRadius
            );
        }

        if (wallCheckPoint != null)
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            DrawWallGizmo(forward, touchingWallForward);
            DrawWallGizmo((forward + right).normalized, touchingWallForwardRight);
            DrawWallGizmo(right, touchingWallRight);
            DrawWallGizmo((-forward + right).normalized, touchingWallBackRight);
            DrawWallGizmo(-forward, touchingWallBack);
            DrawWallGizmo((-forward - right).normalized, touchingWallBackLeft);
            DrawWallGizmo(-right, touchingWallLeft);
            DrawWallGizmo((forward - right).normalized, touchingWallForwardLeft);
        }
    }

    private void DrawWallGizmo(Vector3 direction, bool isTouching)
    {
        Gizmos.color = isTouching ? Color.green : Color.yellow;
        Gizmos.DrawLine(
            wallCheckPoint.position,
            wallCheckPoint.position + (direction * 0.5f)
        );
    }
}