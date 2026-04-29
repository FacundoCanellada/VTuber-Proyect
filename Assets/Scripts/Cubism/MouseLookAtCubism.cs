using UnityEngine;
using UnityEngine.InputSystem;
using Live2D.Cubism.Framework.LookAt;

public class MouseLookAtCubism : MonoBehaviour, ICubismLookTarget
{
    public Vector3 GetPosition()
    {
        // Verificamos si hay un mouse conectado para evitar errores
        if (Mouse.current == null) return Vector3.zero;

        // Así se lee la posición del mouse en el nuevo sistema
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Convertimos a World Point (el proceso es igual, solo cambia la fuente)
        Vector3 screenPos = new Vector3(mousePosition.x, mousePosition.y, 10f);
        
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    public bool IsActive()
    {
        return true;
    }
}
