using UnityEngine;

namespace SlipUp
{
    public class ScreenService : MonoBehaviour
    {
        public enum ScreenId
        {
            None = 0,
            Menu = 1,
            Hud = 2,
            Result = 3,
        }

        [SerializeField] private GameObject _menu;
        [SerializeField] private GameObject _hud;
        [SerializeField] private GameObject _result;

        internal void Show(ScreenId screenId)
        {
            _menu.SetActive(screenId == ScreenId.Menu);
            _hud.SetActive(screenId == ScreenId.Hud);
            _result.SetActive(screenId == ScreenId.Result);
        }
    }
}
