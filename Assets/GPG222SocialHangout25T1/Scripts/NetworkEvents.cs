using Networking.Packets;
using UnityEngine;

namespace Networking.Core
{
    public class NetworkEvents : MonoBehaviour
    {
        public delegate void ServerConnect();
        public ServerConnect ServerConnectEvent;
        
        public delegate void PlayerConnected(PlayerData player);
        public PlayerConnected PlayerConnectedEvent;
        
        public delegate void PositionPacketReceived(PositionPacket packet);
        public PositionPacketReceived PositionPacketReceivedEvent;
        
        public delegate void DestroyPacketReceived(DestroyPacket packet);
        public DestroyPacketReceived DestroyPacketReceivedEvent;

        public delegate void ChatMessageSent(PlayerData player, string message);
        public ChatMessageSent ChatMessageSentEvent;
        
        public delegate void ChatMessageReceived(PlayerData player, string message);
        public ChatMessageReceived ChatMessageReceivedEvent;

        public delegate void PlayerReadinessChanged(PlayerData player, bool isReady);
        public PlayerReadinessChanged PlayerReadinessChangedEvent;
    }
}