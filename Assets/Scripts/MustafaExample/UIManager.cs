using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Moustaffa;

namespace Moustaffa
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] GameObject welcomePanel;
        [SerializeField] Button connectToServerButton;

        [SerializeField] GameObject chatPanel;
        [SerializeField] TMP_Text chatText;
        [SerializeField] TMP_InputField chatInputField;
        [SerializeField] Button sendChatMessageButton;

        void OnDestroy()
        {
            NetworkManager.instance.ServerConnectEvent -= OnServerConnect;
            NetworkManager.instance.ChatMessageReceivedEvent -= OnChatMessageReceived;
            NetworkManager.instance.ChatMessageSentEvent -= OnChatMessageSent;
        }

        void Start()
        {
            NetworkManager.instance.ServerConnectEvent += OnServerConnect;
            NetworkManager.instance.ChatMessageReceivedEvent += OnChatMessageReceived;
            NetworkManager.instance.ChatMessageSentEvent += OnChatMessageSent;

            connectToServerButton.onClick.AddListener(() =>
            {
                NetworkManager.instance.ConnectToServer();
            });

            sendChatMessageButton.onClick.AddListener(() =>
            {
                NetworkManager.instance.SendChatMessage(chatInputField.text);
            });
        }

        void OnServerConnect()
        {
            welcomePanel.SetActive(false);
            chatPanel.SetActive(true);
        }

        void OnChatMessageReceived(string message)
        {
            chatText.text += $"{message}\n";
        }

        void OnChatMessageSent(string message)
        {
            chatText.text += $"{message}\n";
        }
    }
}