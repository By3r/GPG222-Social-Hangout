using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mustafa
{
#if MUSTAFA
    public class UIManager : MonoBehaviour
    {
        [SerializeField] GameObject welcomePanel;
        [SerializeField] TMP_InputField usernameInputField;
        [SerializeField] TMP_Dropdown colorDropdown;
        [SerializeField] Button connectToServerButton;

        [SerializeField] GameObject chatPanel;
        [SerializeField] TMP_Text chatText;
        [SerializeField] TMP_InputField chatInputField;
        [SerializeField] Button sendChatMessageButton;

        int selectedColorIndex = -1;

        void OnDestroy()
        {
            NetworkManager.instance.ServerConnectEvent -= OnServerConnect;

            //--- Message ---
            NetworkManager.instance.ChatMessageReceivedEvent -= OnChatMessageReceived;
            NetworkManager.instance.ChatMessageSentEvent -= OnChatMessageSent;

            //--- Color ---
            NetworkManager.instance.ColorSentEvent -= OnColorSent;
        }

        void Start()
        {
            NetworkManager.instance.ServerConnectEvent += OnServerConnect;

            //--- Message ---
            NetworkManager.instance.ChatMessageReceivedEvent += OnChatMessageReceived;
            NetworkManager.instance.ChatMessageSentEvent += OnChatMessageSent;

            //--- Color ---
            NetworkManager.instance.ColorSentEvent += OnColorSent;

            connectToServerButton.onClick.AddListener(() =>
            {
                NetworkManager.instance.ConnectToServer(usernameInputField.text);
            });

            sendChatMessageButton.onClick.AddListener(() =>
            {
                NetworkManager.instance.SendChatMessage(chatInputField.text);
            });

            colorDropdown.onValueChanged.AddListener(OnColorSelected);
        }

        void OnColorSelected(int colorIndex)
        {
            selectedColorIndex = colorIndex;
        }

        void OnServerConnect()
        {
            NetworkManager.instance.SendColor(selectedColorIndex);
        }

        void OnColorSent(PlayerData playerData, int color)
        {
            welcomePanel.SetActive(false);
            chatPanel.SetActive(true);
        }

        Color MapColorIndexToColor(int colorIndex)
        {
            switch (colorIndex)
            {
                case 0: //Red
                    return new Color(1, 0.25f, 0.25f, 1);

                case 1: //Dark Red
                    return new Color(1, 0.5f, 0.5f, 1);

                case 2: //Bright Red
                    return new Color(1, 0, 0, 1);

                case 3: //Slightly Darker Red
                    return new Color(1, 0.15f, 0.15f, 1);

                case 4: //Blue
                    return new Color(0, 0, 1, 1);

                case 5: //Dark Blue
                    return new Color(0.15f, 0.15f, 1, 1);

                case 6: //Even Darker Blue
                    return new Color(0.5f, 0.5f, 1, 1);

                case 7: //VOID (purple)
                    return new Color(0.619f, 0, 1, 1);

                case 8: //Spice girls
                    return new Color(1, 0, 0.68f, 1);

                case 9: //Barbie Pink
                    return new Color(1, 0.47f, 0.83f, 1);

                case 10: //NION GREEN
                    return new Color(0, 1, 0, 1);

                default:
                    return new Color(0, 0, 0, 1);
            }
        }

        void OnChatMessageReceived(PlayerData playerData, string message)
        {
            int colorIndex = -1;
            // bool yes = NetworkManager.instance.playerColors.TryGetValue(playerData, out colorIndex);

            if (yes)
            {
                chatText.color = MapColorIndexToColor(colorIndex);
            }

            chatText.text += $"{playerData.name}#{playerData.tag}: {message}\n";
        }

        void OnChatMessageSent(PlayerData playerData, string message)
        {
            int colorIndex = -1;
            bool yes = NetworkManager.instance.playerColors.TryGetValue(playerData, out colorIndex);

            if (yes)
            {
                chatText.color = MapColorIndexToColor(colorIndex);
            }

            chatText.text += $"{playerData.name}#{playerData.tag}: {message}\n";
        }
    }
#endif
}