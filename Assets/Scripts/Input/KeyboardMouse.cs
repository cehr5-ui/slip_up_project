using UnityEngine;
using UnityEngine.InputSystem;

namespace SlipUp
{
    internal static class KeyboardMouse
    {
        private const float MouseAxisScale = 0.1f;

        internal static bool WasPressed(Key key)
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard[key].wasPressedThisFrame;
        }

        internal static Vector2 PlanarAxes()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            float x = 0f;
            float y = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                x -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                x += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                y -= 1f;
            }

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                y += 1f;
            }

            return new Vector2(x, y);
        }

        internal static Vector2 MouseAxis()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return Vector2.zero;
            }

            return mouse.delta.ReadValue() * MouseAxisScale;
        }

        internal static bool HasTouch()
        {
            Touchscreen touchscreen = Touchscreen.current;
            return touchscreen != null && touchscreen.primaryTouch.press.isPressed;
        }
    }
}
