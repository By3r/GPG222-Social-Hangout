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
        public int _duckChosen = -1;

        // Start is called before the first frame update
        void Start()
        {
            _client = Client.Instance;
            
            _connectButton.onClick.AddListener((() =>
            {
                OnConnectClicked(inputField.text, _duckChosen);
            }));
        }
        
        public void OnConnectClicked(string username, int duckChosen)
        {
            // Get the username
            if (string.IsNullOrEmpty(username))
            {
                Debug.LogError("Please enter a username");
                _feedbackText.text = "Please enter a username";
                return;
            }

            // Get the chosen duck
            if (duckChosen == -1)
            {
                Debug.LogError("Please choose a duck");
                _feedbackText.text = "Please choose a duck";
                return;
            }
            _duckChosen = duckChosen;
            
            // Making the new player
            if (addressField.text != "")
            {
                _client.ConnectToServer(addressField.text, _duckChosen, username);
            }
            else
            {
                _client.ConnectToServer("127.0.0.1", _duckChosen, username);
            }
        }

        public void OnDuckClicked(int duckNumber) // TODO: Check Duck Availability before allowing it to be chosen
        {
            _feedbackText.text = "Duck chosen: " + duckNumber;
            bool isDuckAvailable = true;
            for (int i = 0; i < _client.PlayersInLobby.Count; i++)
            {
                if (duckNumber == _client.PlayersInLobby[i].DuckID)
                {
                    isDuckAvailable = false;
                    _feedbackText.text = $"Duck {duckNumber} is not available";
                    break;
                }
            }

            if (isDuckAvailable)
            {
                _duckChosen = duckNumber;
            }
            
        }

        public void OnQuitClicked()
        {
            Application.Quit();
        }
    }
}
