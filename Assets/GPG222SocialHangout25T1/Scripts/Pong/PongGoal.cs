using System;
using TMPro;
using UnityEngine;

namespace Networking.Core.Pong
{
    /// <summary>
    /// This handles detecting the Pong ball's collision to register a goal
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class PongGoal : MonoBehaviour
    {
        [SerializeField] private PongController pongController;

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("PongBall"))
            {
                BallController ballController = other.gameObject.GetComponent<BallController>();
                if (ballController.LastPlayerContacted == -1)
                {
                    ballController.LastPlayerContacted = Client.Instance.PlayersInLobby[0].DuckID;
                }
                pongController.ScoreGoal(ballController.LastPlayerContacted);
                ballController.ResetBall();
            }
        }
    }
}