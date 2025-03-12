using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using JW;
public class ButtonsUIManager : MonoBehaviour
{
    #region Variables
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private GameObject optionsPanel;
    #endregion

    public void Play()
    {
        SceneManager.LoadScene(1);
    }

    public void OnConnectButtonClicked()
    {
        string username = usernameInput.text.Trim(); 

        if (string.IsNullOrEmpty(username))
        {
            Debug.LogError("Username cannot be empty!"); // will change this to be player feedback later
            return;
        }

        NetworkManager.instance.ConnectToServer(username);

    }
    public void ToggleOptionsPanel()
    {
        optionsPanel.SetActive(!optionsPanel.activeSelf);
    }

    public void Exit()
    {
        Application.Quit();
    }

    private GamelobbySceneLoader()
    {
        SceneManager.LoadScene(1);
    }
}
