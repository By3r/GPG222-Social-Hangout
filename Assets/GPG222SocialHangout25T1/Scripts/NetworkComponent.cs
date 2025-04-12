using System;
using Networking.Core.Syncing;
using Networking.Packets;
using TMPro;
using UnityEngine;

namespace Networking.Core
{
    public class NetworkComponent : MonoBehaviour
    {
        public string GameObjectID { get; private set; }
        public int OwnerID { get; private set; }

        [SerializeField] private float _packetFrequency = .2f;
        private float _packetTimer = 0f;

        private void OnEnable()
        {
            Client.Instance.PositionPacketReceivedEvent += PositionPacketReceivedEvent;
        }

        private void OnDisable()
        {
            Client.Instance.PositionPacketReceivedEvent -= PositionPacketReceivedEvent;
        }

        private void FixedUpdate()
        {
            if (Client.Instance.PlayersInLobby.Count > 1)
            {
                if (OwnerID != Client.Instance.PlayerData.DuckID) return;
                
                _packetTimer += Time.fixedDeltaTime;
                if (_packetTimer >= _packetFrequency)
                {
                    _packetTimer = 0f;
                    
                    PositionPacket pp = new PositionPacket(Client.Instance.GetPlayerData(OwnerID),  GameObjectID, transform.position, transform.rotation);
                    Client.Instance.SendPositionPacket(pp);
                }
            }
        }

        private void PositionPacketReceivedEvent(PositionPacket pp)
        {
            Debug.LogError($"PP received from {pp.OwnerID} for {pp.ObjectID}");
            if (pp.OwnerID == OwnerID)
            {
                if (GameObjectID == pp.ObjectID)
                {
                    Debug.LogError($"Position set for {gameObject.name}");
                    transform.position = pp.Position;
                    transform.rotation = pp.Rotation;
                }
            }
        }

        public void SetObjectData(string objectID, int ownerID)
        {
            GameObjectID = objectID;
            OwnerID = ownerID;
        }
    }
}