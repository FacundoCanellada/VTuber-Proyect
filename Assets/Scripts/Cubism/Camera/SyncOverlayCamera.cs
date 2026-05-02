using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SyncOverlayCamera : MonoBehaviour
{
    [Tooltip("La cámara principal que está siendo controlada por Cinemachine")]
    public Camera mainCamera; 
    
    private Camera overlayCamera;

    void Awake()
    {
        overlayCamera = GetComponent<Camera>();
        
        if (mainCamera == null && transform.parent != null)
        {
            mainCamera = transform.parent.GetComponent<Camera>();
        }
    }

    void LateUpdate()
    {
        if (mainCamera != null && overlayCamera.orthographicSize != mainCamera.orthographicSize)
        {
            overlayCamera.orthographicSize = mainCamera.orthographicSize;
        }
    }
}