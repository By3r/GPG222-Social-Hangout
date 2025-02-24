using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moustaffa
{
    public class NetworkEvents : MonoBehaviour
    {
        public delegate void ServerConnect();
        public ServerConnect ServerConnectEvent;

        public delegate void ChatMessageReceived(string message);
        public ChatMessageReceived ChatMessageReceivedEvent;

        public delegate void ChatMessageSent(string message);
        public ChatMessageSent ChatMessageSentEvent;
    } 
}