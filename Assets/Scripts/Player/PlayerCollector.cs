using System.Collections;
using UnityEngine;

namespace SlipUp
{
    public class PlayerCollector : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
        [SerializeField] private PlayerAudio _audio;
        [SerializeField] private float _flashDuration = 0.15f;

        private RunModel _model;
        private Material[] _materials;
        private Color[] _originalColors;
        private WaitForSeconds _flashDelay;
        private Coroutine _flash;
        private bool _isCollecting = true;

        internal void Initialize(RunModel model)
        {
            _model = model;
        }

        private void Awake()
        {
            _materials = _skinnedMeshRenderer.materials;
            _originalColors = new Color[_materials.Length];

            for (int i = 0; i < _materials.Length; i++)
            {
                _originalColors[i] = GetMaterialColor(_materials[i]);
            }

            _flashDelay = new WaitForSeconds(_flashDuration);
        }

        private void OnDestroy()
        {
            _model = null;
            _flash = null;
            _flashDelay = null;

            for (int i = 0; i < _materials.Length; i++)
            {
                Destroy(_materials[i]);
                _materials[i] = null;
            }

            _materials = null;
            _originalColors = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollecting == false)
            {
                return;
            }

            if (other.TryGetComponent(out Coin coin) == false)
            {
                return;
            }

            if (coin.TryCollect() == false)
            {
                return;
            }

            _model.AddScore();
            _audio.PlayCollect();
            Flash();
        }

        internal void PauseCollection()
        {
            _isCollecting = false;
        }

        internal void ResumeCollection()
        {
            _isCollecting = true;
        }

        private static Color GetMaterialColor(Material material)
        {
            if (material.HasProperty(BaseColorId))
            {
                return material.GetColor(BaseColorId);
            }

            return material.color;
        }

        private static void SetMaterialColor(Material material, Color color)
        {
            if (material.HasProperty(BaseColorId))
            {
                material.SetColor(BaseColorId, color);
            }

            material.color = color;
        }

        private void Flash()
        {
            StopFlash();
            _flash = StartCoroutine(PlayFlash());
        }

        private void StopFlash()
        {
            if (_flash != null)
            {
                StopCoroutine(_flash);
                _flash = null;
            }

            RestoreColors();
        }

        private IEnumerator PlayFlash()
        {
            SetColors(Color.white);
            yield return _flashDelay;
            RestoreColors();
            _flash = null;
        }

        private void SetColors(Color color)
        {
            for (int i = 0; i < _materials.Length; i++)
            {
                SetMaterialColor(_materials[i], color);
            }
        }

        private void RestoreColors()
        {
            for (int i = 0; i < _materials.Length; i++)
            {
                SetMaterialColor(_materials[i], _originalColors[i]);
            }
        }
    }
}
