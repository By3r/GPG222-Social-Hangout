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

        public TMP_Text OwnerText;
        public TMP_Text ObjectIDText;

        [SerializeField] private float _packetFrequency = 2f;
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
            Debug.LogError("PP received");
            if (pp.OwnerID == OwnerID)
            {
                if (GameObjectID == pp.ObjectID)
                {
                    transform.position = pp.Position;
                    transform.rotation = pp.Rotation;
                    
                    if (OwnerText != null)
                    {
                        OwnerText.text = pp.Position.ToString();
                    }
                    if (ObjectIDText != null)
                    {
                        ObjectIDText.text = pp.Rotation.ToString();
                    }
                }
            }
        }

        public void SetObjectData(string objectID, int ownerID)
        {
            GameObjectID = objectID;
            OwnerID = ownerID;

            if (OwnerText != null)
            {
                OwnerText.text = ownerID.ToString();
            }
            if (ObjectIDText != null)
            {
                ObjectIDText.text = objectID;
            }
        }
    }
}