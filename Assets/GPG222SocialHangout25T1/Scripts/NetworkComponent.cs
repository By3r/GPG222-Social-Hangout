using System;
using Networking.Core.Syncing;
using Networking.Core.WaterSimulation;
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

        private void Update()
        {
            if (Client.Instance.PlayersInLobby.Count > 1) // We are not the only client in the server
            {
                // we don't want to send  updates in the lobby scene if we are a duck (bouyancy components are present)
                if (Client.Instance.SceneIndex == 1) // We are in the lobby scene
                {
                    // We don't continue if we have a bouyancy controller, ie. we are a player
                    BouyancyObject bouyancyObject = GetComponent<BouyancyObject>();
                    if (bouyancyObject != null)
                    {
                        return;
                    }
                }
                
                if (OwnerID != Client.Instance.PlayerData.DuckID) // This is not our object so it should have its transform synced
                {
                    // If we have a transform to sync to, then lerp to it
                    if (_syncTransform != null)
                    {
                        transform.position = Vector3.Lerp(transform.position, _syncTransform.position,  _packetFrequency);
                        transform.rotation = Quaternion.Lerp(transform.rotation, _syncTransform.rotation, _packetFrequency);
                    }
                    
                    return; // We only want to continue if this is our object
                }
                
                // We own this object so we send packets at a set frequency
                _packetTimer += Time.deltaTime;
                if (_packetTimer >= _packetFrequency)
                {
                    _packetTimer = 0f;
                    // TODO: Small optimisation is theoretically possible by only sending position packets if we have moved a sufficient distance
                    PositionPacket pp = new PositionPacket(Client.Instance.GetPlayerData(OwnerID),  GameObjectID, transform.position, transform.rotation);
                    Client.Instance.SendPositionPacket(pp);
                }
                
                
            }
        }

        private void PositionPacketReceivedEvent(PositionPacket pp)
        {
            if (pp.OwnerID == OwnerID)
            {
                if (GameObjectID == pp.ObjectID)
                {
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