using UnityEngine;

public class NetworkEvents : MonoBehaviour
{
    public delegate void ServerConnect();
    public ServerConnect ServerConnectEvent;
}
