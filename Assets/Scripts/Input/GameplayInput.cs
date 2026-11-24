using UnityEngine;

namespace SlipUp
{
    public sealed class GameplayInput
    {
        public Vector2 Move;
        public Vector2 LookStick;
        public bool JumpPressed;
        public bool DashPressed;

        public bool ConsumeJump()
        {
            bool pressed = JumpPressed;
            JumpPressed = false;
            return pressed;
        }

        public bool ConsumeDash()
        {
            bool pressed = DashPressed;
            DashPressed = false;
            return pressed;
        }
    }
}
