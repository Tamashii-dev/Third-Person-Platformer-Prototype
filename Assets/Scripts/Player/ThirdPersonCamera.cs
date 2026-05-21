using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Settings")]
    [SerializeField] private float distance = 4f;
    [SerializeField] private float height = 1.5f;
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 60f;

    private Vector2 lookInput;
    private float yaw;
    private float pitch = 10f;

    private void Start()
    {
        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }
    }

    public void SetLookInput(Vector2 newLookInput)
    {
        lookInput = newLookInput;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        yaw += lookInput.x * rotationSpeed * Time.deltaTime;
        pitch -= lookInput.y * rotationSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 targetPosition = target.position + Vector3.up * height;
        Vector3 cameraOffset = rotation * new Vector3(0f, 0f, -distance);

        transform.position = targetPosition + cameraOffset;
        transform.LookAt(targetPosition);
    }
}