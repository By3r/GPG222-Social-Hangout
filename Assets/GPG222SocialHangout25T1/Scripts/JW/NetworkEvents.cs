using UnityEngine;

namespace JW
{
	public class NetworkEvents : MonoBehaviour
	{
		public delegate void ServerConnect();
		public ServerConnect ServerConnectEvent;
	}
}