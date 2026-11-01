using UnityEngine;
using UnityEngine.SceneManagement;

namespace SlipUp
{
    public class SceneRestarter : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) == false)
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}
