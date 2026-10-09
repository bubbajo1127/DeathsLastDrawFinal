using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Hovl
{
    public class HS_DemoLasers : MonoBehaviour
    {
        public GameObject FirePoint;
        public Camera Cam;
        public float MaxLength;
        public GameObject[] Prefabs;

        private Ray RayMouse;
        private Vector3 direction;
        private Quaternion rotation;

        [Header("GUI")]
        private float windowDpi;

        private int Prefab;
        private GameObject Instance;

        private Component LaserScript;

        private float buttonSaver = 0f;

        void Start()
        {
            if (Screen.dpi < 1)
                windowDpi = 1;
            else if (Screen.dpi < 200)
                windowDpi = 1;
            else
                windowDpi = Screen.dpi / 200f;

            Counter(0);
        }

        void Update()
        {
            bool firePressed = false;
            bool fireReleased = false;

            bool moveLeft = false;
            bool moveRight = false;

            Vector2 mousePosition = Vector2.zero;
            bool hasMousePosition = false;


#if ENABLE_INPUT_SYSTEM
            // NEW INPUT SYSTEM
            if (Mouse.current != null)
            {
                firePressed |= Mouse.current.leftButton.wasPressedThisFrame;
                fireReleased |= Mouse.current.leftButton.wasReleasedThisFrame;

                mousePosition = Mouse.current.position.ReadValue();
                hasMousePosition = true;
            }

            if (Keyboard.current != null)
            {
                moveLeft |=
                    Keyboard.current.aKey.isPressed ||
                    Keyboard.current.leftArrowKey.isPressed;

                moveRight |=
                    Keyboard.current.dKey.isPressed ||
                    Keyboard.current.rightArrowKey.isPressed;
            }
#endif


#if ENABLE_LEGACY_INPUT_MANAGER
            // ============================
            // OLD INPUT SYSTEM
            // ============================

            firePressed |= Input.GetMouseButtonDown(0);
            fireReleased |= Input.GetMouseButtonUp(0);

            moveLeft |=
                Input.GetKey(KeyCode.A) ||
                Input.GetKey(KeyCode.LeftArrow) ||
                Input.GetAxisRaw("Horizontal") < 0f;

            moveRight |=
                Input.GetKey(KeyCode.D) ||
                Input.GetKey(KeyCode.RightArrow) ||
                Input.GetAxisRaw("Horizontal") > 0f;

            // Use legacy mouse position only if new Input System
            // hasn't already supplied it.
            if (!hasMousePosition)
            {
                mousePosition = Input.mousePosition;
                hasMousePosition = true;
            }
#endif


            // ENABLE LASER

            if (firePressed)
            {
                if (Instance != null)
                    Destroy(Instance);

                Instance = Instantiate(
                    Prefabs[Prefab],
                    FirePoint.transform.position,
                    FirePoint.transform.rotation
                );

                Instance.transform.parent = transform;

                // Try to find scripts by name without compile dependency
                LaserScript = Instance.GetComponent("HS_Laser");
            }

            // DISABLE LASER

            if (fireReleased)
            {
                if (LaserScript != null)
                {
                    LaserScript.SendMessage(
                        "DisablePrepare",
                        SendMessageOptions.DontRequireReceiver
                    );
                }

                if (Instance != null)
                    Destroy(Instance, 1f);
            }

            // CHANGE LASER

            if (moveLeft && buttonSaver >= 0.4f)
            {
                buttonSaver = 0f;
                Counter(-1);
            }

            if (moveRight && buttonSaver >= 0.4f)
            {
                buttonSaver = 0f;
                Counter(+1);
            }

            buttonSaver += Time.deltaTime;

            // AIM TO MOUSE

            if (Cam != null && hasMousePosition)
            {
                RaycastHit hit;

                RayMouse = Cam.ScreenPointToRay(mousePosition);

                if (Physics.Raycast(
                    RayMouse.origin,
                    RayMouse.direction,
                    out hit,
                    MaxLength))
                {
                    RotateToMouseDirection(gameObject, hit.point);
                }
                else
                {
                    Vector3 pos = RayMouse.GetPoint(MaxLength);
                    RotateToMouseDirection(gameObject, pos);
                }
            }
            else if (Cam == null)
            {
                Debug.Log("No camera");
            }
        }

        void OnGUI()
        {
            GUI.Label(
                new Rect(
                    10 * windowDpi,
                    5 * windowDpi,
                    400 * windowDpi,
                    20 * windowDpi
                ),
                "Use the keyboard buttons A/<- and D/-> to change lasers!"
            );

            GUI.Label(
                new Rect(
                    10 * windowDpi,
                    20 * windowDpi,
                    400 * windowDpi,
                    20 * windowDpi
                ),
                "Use left mouse button for shooting!"
            );
        }


        void Counter(int count)
        {
            Prefab += count;

            if (Prefab > Prefabs.Length - 1)
                Prefab = 0;
            else if (Prefab < 0)
                Prefab = Prefabs.Length - 1;
        }


        void RotateToMouseDirection(GameObject obj, Vector3 destination)
        {
            direction = destination - obj.transform.position;
            rotation = Quaternion.LookRotation(direction);

            obj.transform.localRotation =
                Quaternion.Lerp(obj.transform.rotation, rotation, 1f);
        }
    }
}
