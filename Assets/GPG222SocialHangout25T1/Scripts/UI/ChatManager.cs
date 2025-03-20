using System;
using Networking.Core;
using Networking.Packets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Networking.UI
{
    public class ChatManager : MonoBehaviour
    {
        private Client _client;
        [SerializeField] private Button _chatButton;
        [SerializeField] private TMP_Text _chatBoxText;
        [SerializeField] private TMP_InputField _chatInput;

        private void Start()
        {
            _client = Client.Instance;
            
            _client.ChatMessageSentEvent += OnChatMessageSend;
            _client.ChatMessageReceivedEvent += OnChatMessageReceived;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return)) // Send chat message when you hit enter
            {
                OnChatMessageSend(_client.PlayerData, _chatInput.text.Trim());
            }
        }

        public void OnChatButtonClicked()
        {
            OnChatMessageSend(_client.PlayerData, _chatInput.text.Trim());
        }

        /// <summary>
        /// Sends a MessagePacket with the message in the input field. Adds the text in the input field to the message text without new line
        /// </summary>
        private void OnChatMessageSend(PlayerData playerData, string message)
        {
            _chatInput.text = ""; // Reset the input box

            // Adds message with a leading new line if not the first line
            _chatBoxText.text += FormatMessage(_client.PlayerData, message);
        }

        private void OnChatMessageReceived(PlayerData player, string message)
        {
            _chatBoxText.text += FormatMessage(player, message);
        }

        private string FormatMessage(PlayerData player, string message)
        {
            string formattedMessage = $"{player.Username}#{player.DuckID}: {message}\n";
            return formattedMessage;
        }
    }
}