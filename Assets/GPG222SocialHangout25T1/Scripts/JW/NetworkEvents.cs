using System;
using UnityEngine;

namespace JW.Dana.BaseNetwork
{
    public class NetworkEvents : MonoBehaviour
    {
        public delegate void ServerConnect();
        public ServerConnect ServerConnectEvent;

        public delegate void ClientConnected();
        public ClientConnected OnNewClientConnect;

        public event Action<int, bool> OnCharacterAvailabilityReceived;
        protected void InvokeCharacterAvailabilityEvent(int characterID, bool isTaken)
        {
            OnCharacterAvailabilityReceived?.Invoke(characterID, isTaken);
        }
    }
}
