using UnityEngine;

namespace Networking.Core.Pong
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    public class BallController : MonoBehaviour
    {
        [SerializeField] private float speed;
        private Rigidbody rb;

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.name == "Player1Goal")
            {
                transform.position = Vector3.zero; // TODO: Properly reset positions and stuff
            }
            else if (collision.gameObject.name == "Player2Goal")
            {
                transform.position = Vector3.zero; // TODO: Properly reset positions and stuff
            }
        }
    }
}