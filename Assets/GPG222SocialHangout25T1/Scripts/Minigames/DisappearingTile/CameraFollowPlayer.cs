using UnityEngine;
using Networking.Core;

public class CameraFollowPlayer : MonoBehaviour
{
    [Header("Target Following")]
    [SerializeField] private Vector3 offset = new Vector3(0, 5f, -8f);
    [SerializeField] private float followSpeed = 5f;

    private Transform target;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Invoke(nameof(FindLocalDuck), 1f);
    }

    private void FindLocalDuck()
    {
        int myDuckID = Client.Instance.PlayerData.DuckID;
        NetworkComponent[] allDucks = FindObjectsOfType<NetworkComponent>();

        foreach (var duck in allDucks)
        {
            if (duck.OwnerID == myDuckID)
            {
                target = duck.transform;
                break;
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.LookAt(target);
    }
}
