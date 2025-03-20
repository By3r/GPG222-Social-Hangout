using Networking.Core;
using TMPro;
using UnityEngine;

namespace Networking.UI
{
    public class UIManager : MonoBehaviour
    {
        private Client _client;
        [Header("Main Menu Screen")]
        // Username input
        [SerializeField] private TMP_InputField inputField;
        private int _duckChosen = -1;
        public delegate int DuckSelected(int duck);
        public DuckSelected OnDuckSelected;

        // Start is called before the first frame update
        void Start()
        {
            _client = Client.Instance;
        }
        
        public void OnConnectClicked()
        {
            // Get the username
            string _username = inputField.text.Trim();
            if (string.IsNullOrEmpty(_username))
            {
                Debug.LogError("Please enter a username");
                return;
            }

            // Get the chosen duck
            if (_duckChosen == -1)
            {
                Debug.LogError("Please choose a duck");
                return;
            }
            
            // Making the new player
            PlayerData playerData = new PlayerData(_duckChosen, _username);
            _client.JoinLobby(playerData);
        }

        public void OnDuckClicked(int duckNumber) // TODO: Check Duck Availability before allowing it to be chosen
        {
            Debug.LogError($"Duck {duckNumber}");
            bool isDuckAvailable = true;
            for (int i = 0; i < _client.PlayersInLobby.Count; i++)
            {
                if (duckNumber == _client.PlayersInLobby[i].DuckID)
                {
                    isDuckAvailable = false;
                    break;
                }
            }

            if (isDuckAvailable)
            {
                _duckChosen = duckNumber;
            }
            Debug.LogError(_duckChosen);
            
        }

        public void OnQuitClicked()
        {
            Application.Quit();
        }
    }
}
