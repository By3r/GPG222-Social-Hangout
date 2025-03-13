using TMPro;
using UnityEngine;
using Dana.Shared.Packets;  
using JW.Dana.BaseNetwork;

namespace Dana.ChatSystem
{
    public class ChatSystem : MonoBehaviour
    {
        [SerializeField] private TMP_InputField chatInput;
        [SerializeField] private TMP_Text chatLog;

        private void Start()
        {
            if (NetworkManager.instance != null)
            {
                NetworkManager.instance.Client.OnPacketReceived += OnPacketReceived;
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.instance != null)
            {
                NetworkManager.instance.Client.OnPacketReceived -= OnPacketReceived;
            }
        }

        public void OnSendButtonClicked()
        {
            if (NetworkManager.instance == null || NetworkManager.instance.playerData == null)
            {
                Debug.LogError("Player data/networkmanager isn't initialised");
                return;
            }

            string message = chatInput.text.Trim();
            if (string.IsNullOrEmpty(message))
                return;

            var playerData = NetworkManager.instance.playerData;

            ChatPacket chatPacket = new ChatPacket(playerData.Name, message, playerData.Color);
            NetworkManager.instance.Client.SendPacket(chatPacket);

            chatInput.text = "";
            chatInput.ActivateInputField();
        }

        private void OnPacketReceived(IPacket packet)
        {
            if (packet is ChatPacket chatPacket)
            {
                AppendMessage($"<color={chatPacket.senderColor}>{chatPacket.senderUsername}</color>: <color=white>{chatPacket.message}</color>");
            }
        }

        private void AppendMessage(string message)
        {
            chatLog.text += "\n" + message;
        }
    }
}
