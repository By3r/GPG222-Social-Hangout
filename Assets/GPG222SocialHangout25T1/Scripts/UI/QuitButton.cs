using UnityEngine;

namespace Networking.UI
{
    public class QuitButton : MonoBehaviour
    {
        public void Quit()
        {
            Application.Quit();
        }
        public static void OnQuit()
        {
            Application.Quit();
        }
    }
}