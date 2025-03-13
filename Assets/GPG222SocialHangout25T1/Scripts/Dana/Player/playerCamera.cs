using JW.Dana.BaseNetwork;
using UnityEngine;

namespace Dana.DuckCam
{/// <summary>
///  Not in use rn
/// </summary>
    public class playerCamera : MonoBehaviour
    {
        [SerializeField] private Camera[] characterCameras;

        private void Start()
        {
            if (NetworkManager.instance == null)
            {
                Debug.LogError("NetworkManager is not initialized");
                return;
            }
            
            if (NetworkManager.instance.playerData == null)
            {
                Debug.LogError("The player data is not initialized");
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
