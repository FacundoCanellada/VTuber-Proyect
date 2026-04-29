using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    public static InputReader Instance { get; private set; }
    private GameInput _input;

    public GameInput.UIActions UI => _input.UI;
    public GameInput.ExplorationActions Exploration => _input.Exploration;
    public GameInput.CombatActions Combat => _input.Combat;

    private void Awake()
    {
        if (Instance == null) { Instance = this; _input = new GameInput(); }
        else Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);
    }

    public void EnableUIMode()
    {
        _input.Exploration.Disable();
        _input.Combat.Disable();
        _input.UI.Enable();
    }

    public void EnableExplorationMode()
    {
        _input.UI.Disable();
        _input.Combat.Disable();
        _input.Exploration.Enable();
    }

    public void EnableSoulMode()
    {
        // Often UI is still enabled for pausing, but for Undertale combat 
        // we might want only Soul active or Soul + UI for menu transitions
        _input.Exploration.Disable();
        _input.UI.Disable(); // Disable UI navigation while dodging
        _input.Combat.Enable();
    }
}
