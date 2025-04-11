using System;
using Networking.Packets;
using UnityEngine;

namespace Networking.Core.Pong
{
    /// <summary>
    /// This handles the control of the Pong ball
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(NetworkComponent))]
    public class BallController : MonoBehaviour
    {
        private NetworkComponent _networkComponent;
        [SerializeField] private float _speed;
        private Rigidbody _rb;
        public int LastPlayerContacted = -1;

        private void OnEnable()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.velocity = Vector3.up * _speed;
            _rb.velocity = Vector3.left * _speed;
            _rb.drag = 0f;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                NetworkComponent networkComponent = other.gameObject.GetComponent<NetworkComponent>();
                LastPlayerContacted = networkComponent.OwnerID;
            }
        }
    }
}