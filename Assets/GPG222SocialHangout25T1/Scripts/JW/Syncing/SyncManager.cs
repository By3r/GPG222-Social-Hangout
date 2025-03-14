using System;
using System.Collections.Generic;
using Dana.JW.Client;
using Dana.Shared.Packets;
using JW.Dana.BaseNetwork;
using JW.Shared.Packets;
using UnityEngine;

namespace JW.Syncing
{
    public class SyncManager : MonoBehaviour
    {
        [SerializeField] private int syncFrameFrequency = 10;
        private int syncFrameCounter = 0;
        [SerializeField] private NetworkManager networkManager;
        private List<ObjectSyncer> syncObjectIDs = new();
        
        public static SyncManager Instance;

        private void Awake()
        {
            // Set up and maintain Singleton SyncManager
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
            
            networkManager = NetworkManager.instance; // Get the network manager so we can send packets

            NetworkManager.instance.Client.OnPacketReceived += OnPacketReceived; // Subsribe to the delegate so we can recieve sync packets
        }

        private void OnDestroy()
        {
            NetworkManager.instance.Client.OnPacketReceived -= OnPacketReceived;
        }

        private void FixedUpdate()
        {
            syncFrameCounter++;
            if (syncFrameCounter < syncFrameFrequency) return; // Will enter when it is time to send the packet with syncing data
            syncFrameCounter = 0;
                
            // Get the positions and rotations of objects to be synced
            Vector3[] positions = new Vector3[syncObjectIDs.Count];
            Vector3[] rotations = new Vector3[syncObjectIDs.Count];
            int[] ownerIDs = new int[syncObjectIDs.Count];
            int[] IDs = new int[syncObjectIDs.Count];

            for (int i = 0; i < syncObjectIDs.Count; i++)
            {
                positions[i] = syncObjectIDs[i].transform.position;
                rotations[i] = syncObjectIDs[i].transform.rotation.eulerAngles;
                ownerIDs[i] = syncObjectIDs[i].OwnerID;
                IDs[i] = syncObjectIDs[i].SyncID;
            }
                
            // Make the FloatX packets from these positions and rotations
            var floatXs = new FloatX[syncObjectIDs.Count];
            for (int i = 0; i < syncObjectIDs.Count; i++)
            {
                // Set up the array
                float[] data = new float[6];
                data[0] = positions[i].x;
                data[1] = positions[i].y;
                data[2] = positions[i].z;
                data[3] = rotations[i].x;
                data[4] = rotations[i].y;
                data[5] = rotations[i].z;
                    
                // Make it into a FloatX
                var floatX = new FloatX(data);
                    
                // Add it to the list
                floatXs[i] = floatX;
            }
                
            // Create and send sync packet
            var syncPacket = new SyncPacket(syncObjectIDs.Count, ownerIDs, IDs, floatXs);
            networkManager.Client.SendPacket(syncPacket);
        }

        public void OnPacketReceived(IPacket packet)
        {
            // make sure it's a sync packet
            if (packet.PacketType != PacketTypes.SyncPacket)
            {
                return;
            }
            var syncPacket = (SyncPacket)packet;
            
            // Get the sync ids and FloatXs
            int[] ownerIDs = syncPacket.SyncOwnerIDs;
            int[] IDs = syncPacket.SyncIDs;
            FloatX[] floatXs = syncPacket.SyncFloatXs;
            
            // Go through each ObjectSyncer
            for (int i = 0; i < syncObjectIDs.Count; i++)
            {
                //   go through each sync id and compare it to the ObjectSyncer id: get that FloatX and apply it
                for (int j = 0; j < syncPacket.SyncIDs.Length; j++)
                {
                    if (syncObjectIDs[j].OwnerID == ownerIDs[j]) // go to next object if we own this one
                    {
                        continue;
                    }
                    
                    if (syncObjectIDs[i].SyncID == syncPacket.SyncIDs[j] && !syncObjectIDs[i].IsSynced) // Sync IDs match and not yet synced, so sync it
                    {
                        syncObjectIDs[i].SyncFromPacket(floatXs[j]);
                    }
                }
            }
        }

        public int AddSyncObject(ObjectSyncer objectToSync)
        {
            syncObjectIDs.Add(objectToSync);
            return syncObjectIDs.Count - 1;
        }
    }
}