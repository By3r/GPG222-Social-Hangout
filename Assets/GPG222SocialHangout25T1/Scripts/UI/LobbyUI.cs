using UnityEngine;
using Networking.Core;

namespace Networking.UI
{
    public class LobbyUI : MonoBehaviour
    {
        public void OnReadyButtonPressed()
        {
            Client.Instance.SendReadyStatus(true);
        }
    }
}