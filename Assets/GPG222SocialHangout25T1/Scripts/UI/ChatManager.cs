using System;
using System.Collections.Generic;
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
        
        [Header("UI Elements")]
        [SerializeField] private Button _chatButton;
        [SerializeField] private TMP_Text _chatBoxText;
        [SerializeField] private TMP_InputField _chatInput;
        [SerializeField] private List<Color> _chatColors;
        private void Start()
        {
            _client = Client.Instance;
            
            _client.ChatMessageSentEvent += OnChatMessageSend;
            _client.ChatMessageReceivedEvent += OnChatMessageReceived;
            
            _chatButton.onClick.AddListener((() =>
            {
                _client.SendChatMessage(_chatInput.text);
            }));
        }

        private void OnDestroy()
        {
            _client.ChatMessageSentEvent -= OnChatMessageSend;
            _client.ChatMessageReceivedEvent -= OnChatMessageReceived;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return)) // Send chat message when you hit enter
            {
                _client.SendChatMessage(_chatInput.text);
            }
        }

        /// <summary>
        /// Sends a MessagePacket with the message in the input field. Adds the text in the input field to the message text without new line
        /// </summary>
        private void OnChatMessageSend(PlayerData playerData, string message)
        {
            // Adds the message we sent
            _chatBoxText.text += FormatMessage(playerData, message);
            // _chatBoxText.text += $"{playerData.Username} sent: {message}\n";
        }

        private void OnChatMessageReceived(PlayerData player, string message)
        {
            // TODO: Add color to messages based on DuckID
            _chatBoxText.text += FormatMessage(player, message);
        }

        private string FormatMessage(PlayerData player, string message)
        {
            // TODO: Add color from Duck ID
            Color color = _chatColors[player.DuckID];
            string formattedMessage = $"<{color}>{player.Username}#{player.DuckID}<{color}>: {message}\n";
            return formattedMessage;
        }
    }
}