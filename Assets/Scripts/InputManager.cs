using UnityEngine;
using UnityEngine.InputSystem;
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public bool StopMovementPressed { get; private set; }

    [Header("Mouse")]
    public Vector3 MouseWorldPosition { get; private set; }
    public Ray MouseRay { get; private set; }
    public RaycastHit MouseHit { get; private set; }
    public bool HasMouseHit { get; private set; }

    public bool RightClickPressed { get; private set; }
    public bool LeftClickPressed { get; private set; }

    [Header("Raycast")]
    [SerializeField] private LayerMask mouseRaycastLayers;

    private Camera mainCamera;

    private void Awake()
    {
        // Make sure there is only one InputManager.
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        mainCamera = Camera.main;
    }

    private void Update()
    {
        UpdateMouseInput();
        UpdateMouseWorldPosition();
    }

    private void UpdateMouseInput()
    {
        // Reset these each frame.
        RightClickPressed = false;
        LeftClickPressed = false;

        if(Mouse.current == null) return;

        RightClickPressed = Mouse.current.rightButton.wasPressedThisFrame;
        LeftClickPressed = Mouse.current.leftButton.wasPressedThisFrame;

        StopMovementPressed = Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame;
    }

    private void UpdateMouseWorldPosition()
    {
        if(mainCamera == null)
        {
            mainCamera = Camera.main;

            if(mainCamera == null) return;
        }

        Vector2 mouseScreenPosition = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

        MouseRay = mainCamera.ScreenPointToRay(mouseScreenPosition);

        HasMouseHit = Physics.Raycast(
            MouseRay,
            out RaycastHit hit,
            Mathf.Infinity,
            mouseRaycastLayers
        );

        if(HasMouseHit)
        {
            MouseHit = hit;
            MouseWorldPosition = hit.point;
        }
    }
}
