using TMPro;
using UnityEngine;
using JW.Dana.BaseNetwork;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Dana.UI
{
    public class ButtonsUIManager : MonoBehaviour
    {
        #region Variables
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_Text feedbackMessages;
        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private Button[] characterButtons;

        private int selectedCharacter = -1;
        private HashSet<int> unavailableCharacters = new();
        #endregion

        private void Start()
        {
            NetworkManager.instance.OnCharacterAvailabilityReceived += UpdateCharacterAvailability;
        }

        #region Public Functions
        public void OnConnectButtonClicked()
        {
            string username = usernameInput.text.Trim();

            if (string.IsNullOrEmpty(username))
            {
                feedbackMessages.text = "Username cant be empty";
                return;
            }

            if (selectedCharacter == -1)
            {
                feedbackMessages.text = "You must select a character before connecting";
                return;
            }

            Debug.Log($"Requesting character selection for {username} with Character ID: {selectedCharacter}");
            loadingPanel.SetActive(true);

            NetworkManager.instance.RequestCharacterSelection(username, selectedCharacter);
        }

        public void SelectCharacter(int characterID)
        {
            if (unavailableCharacters.Contains(characterID))
            {
                feedbackMessages.text = $"Character {characterID} is already taken!";
                return;
            }

            selectedCharacter = characterID;
            Debug.Log($"Selected Character: {characterID}");
        }
        #endregion

        #region Private Functions
        private void UpdateCharacterAvailability(int characterID, bool isTaken)
        {
            if (isTaken)
            {
                unavailableCharacters.Add(characterID);
                characterButtons[characterID].interactable = false;
            }
        }

        public void ToggleCreditsPanel()
        {
            creditsPanel.SetActive(!creditsPanel.activeSelf);
        }

        public void Exit()
        {
            Application.Quit();
        }
        #endregion
    }
}
