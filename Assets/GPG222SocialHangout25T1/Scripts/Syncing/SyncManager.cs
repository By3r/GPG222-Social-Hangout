using System;
using Networking.Core.Lobby;
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
            DontDestroyOnLoad(this);
            
            _client = Client.Instance;

            _client.DestroyPacketReceivedEvent += DestroyFromNetwork;
        }

        private void OnDestroy()
        {
            _client.DestroyPacketReceivedEvent -= DestroyFromNetwork;
        }

        

        private void PositionPacketReceived(PositionPacket packet)
        {
            /*
            var ncs = FindObjectsByType<NetworkComponent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            if (ncs == null) return; // If there aren't any networked objects then stop
            
            foreach (NetworkComponent nc in ncs)
            {
                if (nc.OwnerID == packet.OwnerID && nc.GameObjectID == packet.ObjectID)
                {
                    Debug.Log($"ID: {packet.OwnerID}, {packet.ObjectID} | Pos: {packet.Position}");
                    nc.LastSyncedTransform.position = packet.Position;
                    nc.LastSyncedTransform.rotation = packet.Rotation;
                    nc.LastSyncedTransform = nc.transform;
                    break; // We break here cuz we found the object we want so no need to continue
                }
            }
            */
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
                    nc.gameObject.SetActive(false);
                    return; // We return after destroying the object cuz our job here is done
                }
            }
        }
    }
}