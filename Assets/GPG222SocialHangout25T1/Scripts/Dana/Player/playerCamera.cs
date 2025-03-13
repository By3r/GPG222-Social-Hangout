using JW.Dana.BaseNetwork;
using UnityEngine;

namespace Dana.DuckCam
{
    public class playerCamera : MonoBehaviour
    {
        [SerializeField] private Camera[] characterCameras;

        private void Start()
        {
            if (NetworkManager.instance == null || NetworkManager.instance.playerData == null)
            {
                Debug.LogError("NetworkManager or the player data is not initialized");
                return;
            }

            int selectedCharacter = NetworkManager.instance.playerData.CharacterID;

            foreach (Camera cam in characterCameras)
            {
                cam.gameObject.SetActive(false);
            }

            characterCameras[selectedCharacter].gameObject.SetActive(true);
        }
    }
}
