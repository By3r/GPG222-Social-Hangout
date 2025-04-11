using Networking.Packets;
using UnityEngine;

namespace Networking.Core.Pong
{
    /// <summary>
    /// This handles the input and position update of the player paddle GameObject for the Pong minigame
    /// </summary>
    [RequireComponent(typeof(NetworkComponent))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _positionUpdateFrequency = 0.5f;
        private NetworkComponent _networkComponent;
        private float _positionUpdateTimer;
        private Rigidbody _rb;

        private void FixedUpdate()
        {
            if (_networkComponent.OwnerID == Client.Instance.PlayerData.DuckID) // Only bother getting the inputs if we own this player object
            {
                if (Input.GetKey(KeyCode.W))
                {
                    OnMovePlayer(Vector3.up);
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    OnMovePlayer(Vector3.down);
                }
                else
                {
                    OnMovePlayer(Vector3.zero);
                }
            }
        }

        private void OnEnable()
        {
            _networkComponent = GetComponent<NetworkComponent>();
            _rb = GetComponent<Rigidbody>();
        }

        private void OnMovePlayer(Vector3 moveDirection)
        {
            _rb.velocity = moveDirection.normalized * _speed;
        }
    }
}