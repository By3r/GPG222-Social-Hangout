using Dana.Shared.Packets;
using UnityEngine;

namespace JW.Syncing
{
    public class ObjectSyncer : MonoBehaviour
    {
        public int SyncID;
        public int OwnerID;
        public bool IsSynced;
        private SyncManager syncManager;

        private void Awake()
        {
            syncManager = SyncManager.Instance;
            IsSynced = false;
            SyncID = syncManager.AddSyncObject(this);
        }

        public FloatX GetSyncData()
        {
            float[] syncData = new float[6];
            syncData[0] = transform.position.x;
            syncData[1] = transform.position.y;
            syncData[2] = transform.position.z;
            syncData[3] = transform.rotation.x;
            syncData[4] = transform.rotation.y;
            syncData[5] = transform.rotation.z;
            FloatX returnPacket = new FloatX(syncData);
            return returnPacket;
        }

        public void SyncFromPacket(FloatX packet)
        {
            Vector3.Lerp(transform.position, new Vector3(packet.data[0], packet.data[1], packet.data[2]), Time.deltaTime);
            Vector3.Lerp(transform.rotation.eulerAngles, new Vector3(packet.data[3], packet.data[4], packet.data[5]), Time.deltaTime);
        }
    }
}
