using System;
using Networking.Core.Syncing;
using UnityEngine;

namespace Networking.Core
{
    public class NetworkComponent : MonoBehaviour
    {
        public string GameObjectID { get; private set; }
        public int OwnerID { get; private set; }

        private void OnDestroy()
        {
            SyncManager.DestroyOverNetwork(gameObject);
        }

        public void SetObjectData(string objectID, int ownerID)
        {
            GameObjectID = objectID;
            OwnerID = ownerID;
        }
    }
}