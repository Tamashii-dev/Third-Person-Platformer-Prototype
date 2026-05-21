using UnityEngine;
using UnityEngine.InputSystem;

public class InputDebugProbe : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    private void Update()
    {
        if (playerInput == null) return;

        Vector2 rawMouseDelta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;

        Debug.Log(
            $"Scheme: {playerInput.currentControlScheme} | " +
            $"Paired Devices: {string.Join(", ", System.Array.ConvertAll(playerInput.devices.ToArray(), d => d.displayName))} | " +
            $"Mouse Delta: {rawMouseDelta}"
        );
    }
}