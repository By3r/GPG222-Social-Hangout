using System.Linq;
using TMPro;
using UnityEngine;
using JW.Dana.BaseNetwork;
namespace Dana.ChatSystem
{
    /// <summary>
    /// 
    /// </summary>
    /// 
    public class ChatSystem : MonoBehaviour
    {
        [SerializeField] private TMP_InputField chatInput;
        [SerializeField] private TMP_Text chatLog;

        private void Start()
        {
            if (NetworkManager.instance != null)
            {
                NetworkManager.instance.Client.OnMessageReceived += OnMessageReceived;
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.instance != null)
            {
                NetworkManager.instance.Client.OnMessageReceived -= OnMessageReceived;
            }
        }

        #region Public Functions
        /// <summary>
        /// 
        /// </summary>
        public void OnSendButtonClicked()
        {
            string message = chatInput.text;
            if (string.IsNullOrEmpty(message))
                return;

            var playerData = NetworkManager.instance.playerData;
            string chatMessage = $"CHAT:{playerData.Name}:{playerData.Tag}:{playerData.Color}:{message}";

            NetworkManager.instance.Client.Send(chatMessage, System.Text.Encoding.Unicode);
            chatInput.text = "";
        }
        #endregion


        #region Private Functions
        private void OnMessageReceived(string rawMessage)
        {
            // I will be removing the chat colour appearing next to the player.
            string[] parts = rawMessage.Split(':');
            if (parts.Length < 2)
                return;

            string prefix = parts[0];
            if (prefix == "JOIN" && parts.Length >= 4)
            {
                string username = parts[1];
                string tag = parts[2];
                string color = parts[3];
                string formatted = $"<color={color}>{username}#{tag}</color> joined the lobby.";
                AppendMessage(formatted);
            }
            else if (prefix == "CHAT" && parts.Length >= 5)
            {
                string username = parts[1];
                string tag = parts[2];
                string color = parts[3];

                // Format used: String "color = colourname username, /color"
                string chatText = string.Join(":", parts.Skip(4).ToArray());
                string formatted = $"<color={color}>{username}#{tag}</color>: <color=white>{chatText}</color>";
                AppendMessage(formatted);
            }
            else
            {
                AppendMessage(rawMessage);
            }
        }

        private void AppendMessage(string message)
        {
            chatLog.text += "\n" + message;
        }
        #endregion
    }
}