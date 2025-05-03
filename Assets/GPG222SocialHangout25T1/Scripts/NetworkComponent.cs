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
        public bool IsPhysics = false;
        private Rigidbody _rb;

        [SerializeField] private float _packetFrequency = .2f;
        private float _packetTimer = 0f;
        private Transform _syncTransform;

        private void OnEnable()
        {
            Client.Instance.PositionPacketReceivedEvent += PositionPacketReceivedEvent;
            _syncTransform = transform;
            _rb = GetComponent<Rigidbody>();
        }

        private void OnDisable()
        {
            Client.Instance.PositionPacketReceivedEvent -= PositionPacketReceivedEvent;
        }

        private void Update()
        {
            if (Client.Instance.PlayersInLobby.Count > 1) // We are not the only client in the server
            {
                if (OwnerID != Client.Instance.PlayerData.DuckID) // This is not our object so it should have its transform synced
                {
                    // If we have a transform to sync to, then lerp to it
                    if (_syncTransform != null)
                    {
                        if (IsPhysics)
                        {
                            _rb.velocity = Vector3.Lerp(_rb.velocity, _syncTransform.position, _packetFrequency);
                            _rb.angularVelocity = Vector3.Lerp(_rb.angularVelocity, _syncTransform.rotation.eulerAngles, _packetFrequency);
                        }
                        else
                        {
                            transform.position = Vector3.Lerp(transform.position, _syncTransform.position, _packetFrequency);
                            transform.rotation = Quaternion.Lerp(transform.rotation, _syncTransform.rotation, _packetFrequency);
                        }
                    }
                    
                    return; // We only want to continue if this is our object
                }
                
                // We own this object so we send packets at a set frequency
                _packetTimer += Time.deltaTime;
                if (_packetTimer >= _packetFrequency)
                {
                    _packetTimer = 0f;
                    // TODO: Small optimisation is theoretically possible by only sending position packets if we have moved a sufficient distance
                    if (IsPhysics)
                    {
                        Vector3 av = _rb.angularVelocity;
                        Quaternion rotation = new Quaternion(av.x, av.y, av.z, 0);
                        PositionPacket pp = new PositionPacket(Client.Instance.GetPlayerData(OwnerID),  GameObjectID, _rb.velocity, rotation, true);
                        Client.Instance.SendPositionPacket(pp);
                    }
                    else
                    {
                        PositionPacket pp = new PositionPacket(Client.Instance.GetPlayerData(OwnerID),  GameObjectID, transform.position, transform.rotation);
                        Client.Instance.SendPositionPacket(pp);
                    }
                    
                }
                
                
            }
        }

        private void PositionPacketReceivedEvent(PositionPacket pp)
        {
            if (pp.OwnerID == OwnerID)
            {
                if (GameObjectID == pp.ObjectID)
                {
                    IsPhysics = pp.IsPhysics;
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