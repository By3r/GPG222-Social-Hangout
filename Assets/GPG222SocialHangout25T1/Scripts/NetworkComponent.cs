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
        
        public Transform LastSyncedTransform { get; set; }

        public TMP_Text OwnerText;
        public TMP_Text ObjectIDText;

        private void Awake()
        {
            Client.Instance.PositionPacketReceivedEvent += PositionPacketReceivedEvent;
        }

        private void OnDestroy()
        {
            Client.Instance.PositionPacketReceivedEvent -= PositionPacketReceivedEvent;
        }

        private void FixedUpdate()
        {
            if (LastSyncedTransform != null)
            {
                transform.position = Vector3.Lerp(transform.position, LastSyncedTransform.position, Time.fixedDeltaTime);
            }
        }

        private void PositionPacketReceivedEvent(PositionPacket pp)
        {
            Debug.LogError("PP received");
            if (pp.OwnerID == OwnerID)
            {
                if (GameObjectID == pp.ObjectID)
                {
                    LastSyncedTransform.position = pp.Position;
                    LastSyncedTransform.rotation = pp.Rotation;
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
            
            LastSyncedTransform = transform;
        }
    }
}