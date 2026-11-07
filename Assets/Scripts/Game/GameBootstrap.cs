using UnityEngine;

namespace SlipUp
{
    [DefaultExecutionOrder(-100)]
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private RunSession _session;
        [SerializeField] private AudioService _audio;
        [SerializeField] private MainMenuView _menu;
        [SerializeField] private HudView _hud;
        [SerializeField] private ResultView _result;

        private SaveService _save;
        private RunModel _model;

        private void Awake()
        {
            _save = new SaveService();
            _model = new RunModel();
            _audio.Initialize(_save);
            _menu.Initialize(_save, _audio);
            _hud.Initialize(_model);
            _result.Initialize(_model);
            _session.Initialize(_model, _save);
            _menu.PlayPressed += OnPlayPressed;
            _result.AgainPressed += OnAgainPressed;
            _result.MenuPressed += OnMenuPressed;
        }

        private void Start()
        {
            _session.OpenMenu();
        }

        private void OnDestroy()
        {
            if (_menu != null)
            {
                _menu.PlayPressed -= OnPlayPressed;
                _menu = null;
            }

            if (_result != null)
            {
                _result.AgainPressed -= OnAgainPressed;
                _result.MenuPressed -= OnMenuPressed;
                _result = null;
            }

            if (_model != null)
            {
                _model.Clear();
                _model = null;
            }

            _session = null;
            _audio = null;
            _hud = null;
            _save = null;
        }

        private void OnPlayPressed()
        {
            _session.Play();
        }

        private void OnAgainPressed()
        {
            _session.Play();
        }

        private void OnMenuPressed()
        {
            _session.OpenMenu();
        }
    }
}
