using Networking.Packets;
using UnityEngine;

namespace Networking.Core
{
    public class NetworkEvents : MonoBehaviour
    {
        public delegate void ServerConnect();
        public ServerConnect ServerConnectEvent;
        
        public delegate void ClientJoinedLobby();
        public ClientJoinedLobby LobbyJoinEvent;
        
        public delegate void PacketReceived(BasePacket packet);
        public PacketReceived PacketReceivedEvent;

        public delegate void ChatMessageSent(PlayerData player, string message);
        public ChatMessageSent ChatMessageSentEvent;
        
        public delegate void ChatMessageReceived(PlayerData player, string message);
        public ChatMessageReceived ChatMessageReceivedEvent;
    }
}