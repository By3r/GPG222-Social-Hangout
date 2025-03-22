using System;
using Networking.Packets;
using UnityEngine;

namespace Networking.Core.Syncing
{
    public class SyncManager : MonoBehaviour
    {
        Client _client;

        [SerializeField] private int _syncFrequency;
        private int _syncCounter;

        private void Start()
        {
            _client = Client.Instance;

            _client.PositionPacketReceivedEvent += PositionPacketReceived;
            _client.DestroyPacketReceivedEvent += DestroyFromNetwork;
        }

        private void OnDestroy()
        {
            _client.PositionPacketReceivedEvent -= PositionPacketReceived;
            _client.DestroyPacketReceivedEvent -= DestroyFromNetwork;
        }

        private void FixedUpdate()
        {
            _syncCounter++;
            if (_syncCounter == _syncFrequency)
            {
                var ncs = FindObjectsByType<NetworkComponent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None); // Get all networked GameObject in the scene
                
                if (ncs == null) return; // If there aren't any networked objects then stop
                
                foreach (NetworkComponent nc in ncs) // For each object, send the position and rotation of our owned objects
                {
                    if (nc.OwnerID != _client.PlayerData.DuckID) continue;
                    
                    PositionPacket pp  = new PositionPacket(_client.PlayerData, nc.GameObjectID, nc.transform.position, nc.transform.rotation);
                    _client.SendPositionPacket(pp);
                }
            }
        }

        private void PositionPacketReceived(PositionPacket packet)
        {
            var ncs = FindObjectsByType<NetworkComponent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            foreach (NetworkComponent nc in ncs)
            {
                if (nc.OwnerID == packet.OwnerID && nc.GameObjectID == packet.ObjectID)
                {
                    nc.transform.position = Vector3.Lerp(nc.transform.position, packet.Position, Time.deltaTime);
                    nc.transform.rotation = Quaternion.Lerp(nc.transform.rotation, packet.Rotation, Time.deltaTime);
                    break; // We break here cuz we found the object we want so no need to continue
                }
            }
        }

        public static void DestroyOverNetwork(GameObject gameObject)
        {
            NetworkComponent nc = gameObject.GetComponent<NetworkComponent>();
            if (nc == null) return; // The object isn't networked so we don't bother sending

            DestroyPacket dp = new DestroyPacket(Client.Instance.PlayerData, nc.GameObjectID);
            Client.Instance.SendDestroyPacket(dp);
        }

        public void DestroyFromNetwork(DestroyPacket packet)
        {
            NetworkComponent[] ncs = FindObjectsOfType<NetworkComponent>();

            foreach (NetworkComponent nc in ncs)
            {
                if (nc.OwnerID == packet.OwnerID && nc.GameObjectID == packet.ObjectID)
                {
                    Destroy(nc.gameObject);
                    return; // We return after destroying the object cuz our job here is done
                }
            }
        }
    }
}