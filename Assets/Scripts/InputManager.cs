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

    public bool Ability1Pressed { get; private set; }
    public bool Ability2Pressed { get; private set; }

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
        RightClickPressed = false;
        LeftClickPressed = false;
        Ability1Pressed = false;
        Ability2Pressed = false;
        StopMovementPressed = false;

        if (Mouse.current != null)
        {
            RightClickPressed = Mouse.current.rightButton.wasPressedThisFrame;
            LeftClickPressed = Mouse.current.leftButton.wasPressedThisFrame;
        }

        if (Keyboard.current != null)
        {
            Ability1Pressed = Keyboard.current.qKey.wasPressedThisFrame;
            Ability2Pressed = Keyboard.current.wKey.wasPressedThisFrame;
            StopMovementPressed = Keyboard.current.sKey.wasPressedThisFrame;
        }
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

    public bool TryGetMousePositionOnPlane(float height, out Vector3 worldPosition)
    {
        Plane plane = new Plane(Vector3.up, new Vector3(0f, height, 0f));

        if (plane.Raycast(MouseRay, out float distance))
        {
            worldPosition = MouseRay.GetPoint(distance);
            return true;
        }

        worldPosition = Vector3.zero;
        return false;
    }
}
