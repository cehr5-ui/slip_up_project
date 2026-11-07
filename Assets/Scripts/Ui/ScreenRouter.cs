using UnityEngine;

namespace SlipUp
{
    public class ScreenRouter : MonoBehaviour
    {
        [SerializeField] private GameObject _menu;
        [SerializeField] private GameObject _hud;
        [SerializeField] private GameObject _result;

        internal void Show(RunPhase phase)
        {
            bool isMenu = phase == RunPhase.Menu;
            bool isResult = phase == RunPhase.Result;
            _menu.SetActive(isMenu);
            _hud.SetActive(isMenu == false && isResult == false);
            _result.SetActive(isResult);
        }

        private void OnDestroy()
        {
            _menu = null;
            _hud = null;
            _result = null;
        }
    }
}
