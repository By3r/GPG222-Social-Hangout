using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtonFunctions : MonoBehaviour
{
        #region Variables
        [SerializeField] private GameObject optionsPanel;
        #endregion

        public void Play()
        {
            SceneManager.LoadScene(1);
        }

        public void ToggleOptionsPanel()
        {
            optionsPanel.SetActive(!optionsPanel.activeSelf);
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
