using UnityEngine;

namespace Mustafa
{
#if MUSTAFA
    public class NetworkEvents : MonoBehaviour
    {
        public delegate void ServerConnect();
        public ServerConnect ServerConnectEvent;

        //---- Message ----
        public delegate void ChatMessageReceived(PlayerData playerData, string message);
        public ChatMessageReceived ChatMessageReceivedEvent;

        public delegate void ChatMessageSent(PlayerData playerData, string message);
        public ChatMessageSent ChatMessageSentEvent;

        //---- color ----
        public delegate void ColorSent(PlayerData playerData, int colorIndex);
        public ColorSent ColorSentEvent;

        public delegate void ColorReceived(PlayerData playerData, int colorIndex);
        public ColorReceived ColorReceivedEvent;

    }
#endif
}
