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
        private Transform _syncTransform;

        private void OnEnable()
        {
            Client.Instance.PositionPacketReceivedEvent += PositionPacketReceivedEvent;
            _syncTransform = transform;
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
                    // TODO: Small optimisation is theoretically possible by only sending position packets if we have moved a sufficient distance
                    PositionPacket pp = new PositionPacket(Client.Instance.GetPlayerData(OwnerID),  GameObjectID, transform.position, transform.rotation);
                    Client.Instance.SendPositionPacket(pp);
                }

                if (_syncTransform != null)
                {
                    transform.position = Vector3.Lerp(transform.position, _syncTransform.position, Time.fixedDeltaTime * _packetFrequency);
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
                    _syncTransform.position = pp.Position;
                    _syncTransform.rotation = pp.Rotation;
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