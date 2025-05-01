using Networking.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Networking.UI
{
    public class UIManager : MonoBehaviour
    {
        private Client _client;
        [Header("Main Menu Screen")]
        // Username input
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_InputField addressField;
        [SerializeField] private Button _connectButton;
        [SerializeField] private TMP_Text _feedbackText;
        [SerializeField] private GameObject duckSelectionPanel;

        public int _duckChosen = -1;

        // Start is called before the first frame update
        void Start()
        {
            _client = Client.Instance;

            _connectButton.onClick.AddListener(() =>
            {
                string username = inputField.text;
                if (string.IsNullOrEmpty(username))
                {
                    _feedbackText.text = "Please enter a username";
                    return;
                }

                string address = string.IsNullOrEmpty(addressField.text) ? "127.0.0.1" : addressField.text;
                _client.ConnectToServer(address, username);
            });
        }

        public void OnDuckClicked(int duckNumber)
        {
            _feedbackText.text = $"Duck chosen: {duckNumber}";

            bool isDuckAvailable = !_client.PlayersInLobby.Exists(p => p.DuckID == duckNumber);

            if (isDuckAvailable)
            {
                _duckChosen = duckNumber;
            }
            else
            {
                _feedbackText.text = $"Duck {duckNumber} is not available";
            }
        }

        public void OnConfirmDuckClicked()
        {
            if (_duckChosen == -1)
            {
                _feedbackText.text = "Please choose a duck before confirming!";
                return;
            }

            _client.ConfirmDuckSelection(_duckChosen);
        }


        public void OnQuitClicked()
        {
            Application.Quit();
        }
        public void EnableDuckSelection()
        {
            duckSelectionPanel.SetActive(true);
        }
    }
}
