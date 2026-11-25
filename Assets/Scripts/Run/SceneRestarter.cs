using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SlipUp
{
    public class SceneRestarter : MonoBehaviour
    {
        private void Update()
        {
            if (KeyboardMouse.WasPressed(Key.R) == false)
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}
