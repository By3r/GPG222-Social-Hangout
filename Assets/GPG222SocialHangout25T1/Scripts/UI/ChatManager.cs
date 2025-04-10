using System;
using System.Collections.Generic;
using Networking.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Networking.UI
{
    public class ChatManager : MonoBehaviour
    {
        private Client _client;

        [Header("UI Elements")]
        [SerializeField] private GameObject chatBox;
        [SerializeField] private GameObject chatIcon;
        [SerializeField] private Button chatButton;
        [SerializeField] private TMP_Text chatBoxText;
        [SerializeField] private TMP_InputField chatInput;
        [SerializeField] private List<Color> chatColors;
        [SerializeField] private List<string> colorCodes;

        private bool _wantsToMessage = false;
        private void Start()
        {
            _client = Client.Instance;

            _client.ChatMessageSentEvent += OnChatMessageSend;
            _client.ChatMessageReceivedEvent += OnChatMessageReceived;

            chatButton.onClick.AddListener((() =>
            {
                _client.SendChatMessage(chatInput.text);
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
                _client.SendChatMessage(chatInput.text);
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleChatBox();
            }
        }

        public void ToggleChatBox()
        {
            chatBox.SetActive(_wantsToMessage);
            chatIcon.SetActive(!_wantsToMessage);
            _wantsToMessage = !_wantsToMessage;
            print($"{_wantsToMessage} = chatBox and {!_wantsToMessage} is chatIcon");
        }

        /// <summary>
        /// Sends a MessagePacket with the message in the input field. Adds the text in the input field to the message text without new line
        /// </summary>
        private void OnChatMessageSend(PlayerData playerData, string message)
        {
            // Adds the message we sent
            chatBoxText.text += FormatMessage(playerData, message);
            // _chatBoxText.text += $"{playerData.Username} sent: {message}\n";
        }

        private void OnChatMessageReceived(PlayerData player, string message)
        {
            // TODO: Add color to messages based on DuckID
            chatBoxText.text += FormatMessage(player, message);
        }

        private string FormatMessage(PlayerData player, string message)
        {
            // TODO: Add color from Duck ID
            string color = colorCodes[player.DuckID];
            string formattedMessage = $"<color=#{color}>{player.Username}#{player.DuckID}</color>: {message}\n";
            return formattedMessage;
        }
    }
}