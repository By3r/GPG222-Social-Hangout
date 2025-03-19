using System.Collections;
using System.Collections.Generic;
using Networking.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Networking.UI
{
    public class UIManager : MonoBehaviour
    {
        private Client _client;
        
        [SerializeField] private TMP_InputField inputField;
        
        // Buttons
        [SerializeField] private Button[] _duckButtons = new Button[4];
        [SerializeField] private Button _connectButton;
        
        private int _duckChosen = -1;
        // Start is called before the first frame update
        void Start()
        {
            _connectButton.onClick.AddListener(OnConnectClicked);
            _client = Client.Instance;

            for (int i = 0; i < _duckButtons.Length; i++)
            {
                _duckButtons[i].onClick.AddListener(OnDuckClicked(i));
            }
        }

        private void OnConnectClicked()
        {
            string _username = inputField.text.Trim();
            if (string.IsNullOrEmpty(_username))
            {
                Debug.LogError("Please enter a username");
            }

            if (_duckChosen == -1)
            {
                Debug.LogError("Please choose a duck");
            }
            
            PlayerData playerData = new PlayerData(_duckChosen, _username);
            _client.JoinLobby(playerData);
        }

        private UnityAction OnDuckClicked(int duckNumber) // TODO: Check Duck Availability before allowing it to be chosen
        {
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


            return null;
        }
    }
}
