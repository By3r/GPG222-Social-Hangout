using System;
using System.Collections.Generic;
using Networking.Packets;
using UnityEngine;

namespace Networking.Core.Pong
{
    [RequireComponent(typeof(NetworkComponent))] [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        NetworkComponent networkComponent;
        Rigidbody rb;
        [SerializeField] private float speed = 5f;
        
        private void FixedUpdate()
        {
            if (Input.GetKey(KeyCode.W))
            {
                OnMovePlayer(Vector3.up);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                OnMovePlayer(Vector3.down);
            }
        }

        public void OnMovePlayer(Vector3 moveDirection)
        {
            rb.velocity = moveDirection.normalized * speed;
            PositionPacket pp = new PositionPacket(
                Client.Instance.PlayerData, 
                networkComponent.GameObjectID, 
                networkComponent.gameObject.transform.position, 
                networkComponent.gameObject.transform.rotation);
            Client.Instance.SendPositionPacket(pp);
        }

        private void OnPositionPacketRecieved(PositionPacket packet)
        {
            // TODO: Implement the position being updated
            if (packet.OwnerID != Client.Instance.PlayerData.DuckID)
            {
                if (packet.ObjectID == networkComponent.GameObjectID)
                {
                    transform.position = packet.Position;
                    transform.rotation = packet.Rotation;
                }
            }
        }
    }
}