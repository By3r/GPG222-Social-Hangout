using System;
using Networking.Core.Syncing;
using Networking.Packets;
using TMPro;
using UnityEngine;

namespace Networking.Core
{
    public class NetworkComponent : MonoBehaviour
    {
        #region Variables
        public string GameObjectID { get; private set; }
        public int OwnerID { get; private set; }

        public string Username { get; private set; }

        [SerializeField] private float _packetFrequency = .2f;
        private float _packetTimer = 0f;
        private Transform _syncTransform;

        [SerializeField] private float _movementThreshold = 0.01f;
        [SerializeField] private float _rotationThreshold = 0.5f; // degrees

        private Vector3 _lastSentPosition;
        private Quaternion _lastSentRotation;

        #endregion

        private void OnEnable()
        {
            Client.Instance.PositionPacketReceivedEvent += PositionPacketReceivedEvent;
            _syncTransform = transform;
        }

        private void OnDisable()
        {
            Client.Instance.PositionPacketReceivedEvent -= PositionPacketReceivedEvent;
        }

        private void FixedUpdate()
        {
            if (Client.Instance.PlayersInLobby.Count > 1)
            {
                if (OwnerID != Client.Instance.PlayerData.DuckID) return;

                _packetTimer += Time.fixedDeltaTime;
                if (_packetTimer >= _packetFrequency)
                {
                    _packetTimer = 0f;

                    Vector3 positionDelta = transform.position - _lastSentPosition;
                    float rotationDelta = Quaternion.Angle(transform.rotation, _lastSentRotation);

                    if (positionDelta.sqrMagnitude > _movementThreshold * _movementThreshold ||
                        rotationDelta > _rotationThreshold)
                    {
                        PositionPacket pp = new PositionPacket(
                            Client.Instance.GetPlayerData(OwnerID),
                            GameObjectID,
                            transform.position,
                            transform.rotation
                        );
                        Client.Instance.SendPositionPacket(pp);

                        _lastSentPosition = transform.position;
                        _lastSentRotation = transform.rotation;
                    }
                }

                if (_syncTransform != null)
                {
                    transform.position = Vector3.Lerp(transform.position, _syncTransform.position, Time.fixedDeltaTime / _packetFrequency);
                    transform.rotation = Quaternion.Lerp(transform.rotation, _syncTransform.rotation, Time.fixedDeltaTime / _packetFrequency);
                }
            }
        }

        private void PositionPacketReceivedEvent(PositionPacket pp)
        {
            Debug.LogError($"PP received from {pp.OwnerID} for {pp.ObjectID}");
            if (pp.OwnerID == OwnerID)
            {
                if (GameObjectID == pp.ObjectID)
                {
                    Debug.LogError($"Position set for {gameObject.name}");
                    _syncTransform.position = pp.Position;
                    _syncTransform.rotation = pp.Rotation;
                }
            }
        }

        public void SetObjectData(string objectID, int ownerID, string username = null)
        {
            GameObjectID = objectID;
            OwnerID = ownerID;
            Username = username;
        }

    }
}