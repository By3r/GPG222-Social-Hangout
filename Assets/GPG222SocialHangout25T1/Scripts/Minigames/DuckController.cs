using Networking.Core;
using UnityEngine;

public class DuckController : MonoBehaviour
{
    #region Variables
    private NetworkComponent _networkComponent;
    private int _ownerID;

    [SerializeField] private float _moveSpeed = 5f;
    #endregion

    private void Start()
    {
        _networkComponent = GetComponent<NetworkComponent>();
        _ownerID = _networkComponent.OwnerID;
    }

    private void Update()
    {
        if (_ownerID != Client.Instance.PlayerData.DuckID) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 inputDirection = new Vector3(h, 0f, v).normalized;

        if (inputDirection.sqrMagnitude > 0.01f)
        {
            transform.Translate(inputDirection * _moveSpeed * Time.deltaTime, Space.World);

            Quaternion targetRot = Quaternion.LookRotation(inputDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }
    }
}
