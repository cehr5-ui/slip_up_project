using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlipUp
{
    [DefaultExecutionOrder(-50)]
    public class MobileControlsView : MonoBehaviour
    {
        private const float JoystickSize = 220f;
        private const float HandleSize = 90f;
        private const float JoystickRange = 70f;
        private const float SidePadding = 48f;
        private const float BottomPadding = 36f;
        private const float ButtonSize = 92f;

        private GameplayInput _input;
        private VirtualJoystick _move;
        private VirtualJoystick _look;
        private Sprite _circle;

        internal void Initialize(GameplayInput input)
        {
            _input = input;
            Build();
        }

        private void OnDestroy()
        {
            _input = null;
            _move = null;
            _look = null;

            if (_circle != null)
            {
                Destroy(_circle.texture);
                Destroy(_circle);
                _circle = null;
            }
        }

        private void Update()
        {
            if (_input == null)
            {
                return;
            }

            if (_move != null)
            {
                _input.Move = _move.Value;
            }

            if (_look != null)
            {
                _input.LookStick = _look.Value;
            }
        }

        private void Build()
        {
            if (_move != null)
            {
                return;
            }

            _circle = CreateCircleSprite();
            _move = CreateJoystick("MoveJoystick", new Vector2(0f, 0f), new Vector2(SidePadding, BottomPadding));
            _look = CreateJoystick("CameraJoystick", new Vector2(1f, 0f), new Vector2(-SidePadding, BottomPadding));
            CreateActionButton(
                "JumpButton",
                "Прыжок",
                new Vector2(-SidePadding - ButtonSize - 16f, BottomPadding + JoystickSize + 12f),
                OnJumpPressed);
            CreateActionButton(
                "DashButton",
                "Рывок",
                new Vector2(-SidePadding, BottomPadding + JoystickSize + 12f),
                OnDashPressed);
        }

        private VirtualJoystick CreateJoystick(string name, Vector2 anchor, Vector2 offset)
        {
            RectTransform root = CreateRect(name, transform, anchor, offset, new Vector2(JoystickSize, JoystickSize));
            Image background = root.gameObject.AddComponent<Image>();
            background.sprite = _circle;
            background.color = new Color(1f, 1f, 1f, 0.22f);
            background.raycastTarget = true;

            RectTransform handle = CreateRect("Handle", root, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(HandleSize, HandleSize));
            handle.anchorMin = new Vector2(0.5f, 0.5f);
            handle.anchorMax = new Vector2(0.5f, 0.5f);
            Image handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.sprite = _circle;
            handleImage.color = new Color(1f, 1f, 1f, 0.7f);
            handleImage.raycastTarget = false;

            VirtualJoystick joystick = root.gameObject.AddComponent<VirtualJoystick>();
            joystick.Bind(root, handle, JoystickRange);
            return joystick;
        }

        private void CreateActionButton(string name, string label, Vector2 offset, Action pressed)
        {
            RectTransform root = CreateRect(name, transform, new Vector2(1f, 0f), offset, new Vector2(ButtonSize, ButtonSize));
            Image image = root.gameObject.AddComponent<Image>();
            image.sprite = _circle;
            image.color = new Color(1f, 1f, 1f, 0.28f);
            image.raycastTarget = true;

            HoldButton button = root.gameObject.AddComponent<HoldButton>();
            button.Pressed += pressed;

            GameObject textObject = new GameObject("Label", typeof(RectTransform));
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.SetParent(root, false);
            Stretch(textRect);
            Text text = textObject.AddComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 22;
            text.raycastTarget = false;
        }

        private void OnJumpPressed()
        {
            if (_input == null)
            {
                return;
            }

            _input.JumpPressed = true;
        }

        private void OnDashPressed()
        {
            if (_input == null)
            {
                return;
            }

            _input.DashPressed = true;
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchor, Vector2 offset, Vector2 size)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = offset;
            rect.sizeDelta = size;
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Sprite CreateCircleSprite()
        {
            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[size * size];
            float radius = size * 0.5f - 1f;
            float radiusSqr = radius * radius;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - size * 0.5f;
                    float dy = y + 0.5f - size * 0.5f;
                    bool inside = dx * dx + dy * dy <= radiusSqr;
                    pixels[y * size + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 64f);
        }

        private sealed class HoldButton : MonoBehaviour, IPointerDownHandler
        {
            internal event Action Pressed;

            public void OnPointerDown(PointerEventData eventData)
            {
                Pressed?.Invoke();
            }

            private void OnDestroy()
            {
                Pressed = null;
            }
        }
    }
}
