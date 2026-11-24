using UnityEngine;

namespace SlipUp
{
    [DefaultExecutionOrder(-20)]
    public class CharacterSkin : MonoBehaviour
    {
        private static readonly Color[] SkinColors =
        {
            new Color(1f, 0.15f, 0.12f, 1f),
            new Color(0.2f, 0.55f, 1f, 1f),
            new Color(0.2f, 0.85f, 0.4f, 1f),
            new Color(1f, 0.82f, 0.2f, 1f),
        };

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static int SelectedIndex;

        [SerializeField] private SkinnedMeshRenderer _renderer;
        [SerializeField] private PlayerCollector _collector;

        private Material[] _shared;
        private Material[] _skins;
        private bool _isReady;

        internal SkinnedMeshRenderer Renderer => _renderer;

        private void Awake()
        {
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
            }

            if (_collector == null)
            {
                _collector = GetComponent<PlayerCollector>();
            }

            BuildSkins();
            ApplySelected();
            _isReady = true;
        }

        private void OnDestroy()
        {
            DestroySkins();
            _renderer = null;
            _collector = null;
        }

        internal static void Cycle()
        {
            SelectedIndex++;

            if (SelectedIndex >= SkinColors.Length)
            {
                SelectedIndex = 0;
            }

            CharacterSkin[] skins = FindObjectsByType<CharacterSkin>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < skins.Length; i++)
            {
                skins[i].ApplySelected();
            }
        }

        private void BuildSkins()
        {
            _shared = _renderer.sharedMaterials;
            _skins = new Material[SkinColors.Length];

            Material source = _shared[0];

            for (int i = 0; i < SkinColors.Length; i++)
            {
                Material skin = new Material(source);
                SetColor(skin, SkinColors[i]);
                _skins[i] = skin;
            }
        }

        private void ApplySelected()
        {
            if (_skins == null)
            {
                return;
            }

            Material[] materials = new Material[_shared.Length];

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = _skins[SelectedIndex];
            }

            _renderer.sharedMaterials = materials;

            if (_isReady && _collector != null)
            {
                _collector.RefreshColors();
            }
        }

        private void DestroySkins()
        {
            if (_skins == null)
            {
                return;
            }

            for (int i = 0; i < _skins.Length; i++)
            {
                Destroy(_skins[i]);
                _skins[i] = null;
            }

            _skins = null;
            _shared = null;
        }

        private static void SetColor(Material material, Color color)
        {
            if (material.HasProperty(BaseColorId))
            {
                material.SetColor(BaseColorId, color);
            }

            material.color = color;
        }
    }
}
