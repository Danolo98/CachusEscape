using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Cachu.Core
{
    public class InputReader : MonoBehaviour
    {
        public static InputReader I { get; private set; }
        public bool Pressed { get; private set; }
        public bool Held { get; private set; }
        public bool Released { get; private set; }

        void Awake()
        {
            if (I != null) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            bool any = false;

#if ENABLE_INPUT_SYSTEM
            any = Keyboard.current?.spaceKey?.isPressed == true
                   || Gamepad.current?.buttonSouth?.isPressed == true
                   || Mouse.current?.leftButton?.isPressed == true
                   || (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed);
#endif

            // fallback clásico si el nuevo no responde
            any = any || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);

            bool prev = Held;
            Held = any;
            Pressed = Held && !prev;
            Released = !Held && prev;

            if (Pressed)
                Debug.Log("🟢 Barra espaciadora detectada");
        }
    }
}
