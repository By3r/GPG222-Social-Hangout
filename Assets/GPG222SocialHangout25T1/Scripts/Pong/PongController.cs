using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Networking.Core.Pong
{
    /// <summary>
    ///     Handle's spawning in players and keeping score
    /// </summary>
    public class PongController : MonoBehaviour
    {
        [SerializeField] private List<Transform> playerTransforms = new List<Transform>();
        [Tooltip("Order: left, right, top, bottom")]
        [SerializeField] private List<PongGoal> goals = new List<PongGoal>();
        public Dictionary<int, int> PlayerScores = new Dictionary<int, int>();
        [SerializeField] private TMP_Text scoreText;

        /* Order Of Operations
         * 1. Load the Pong minigame scene
         *      This happens when there are an even number of players, and they are all ready
         * 2. Spawn in the players
         *      This needs to be done per player in the list of players in lobby
         * 3. Spawn in the ball
         * 4. Continue with the minigame from here
         */
        private void Start()
        {
            // Spawn in your own Player paddle
            // TODO: Change location so the host is always left
            Client.Instance.InstantiateOverNetwork(
                "Prefabs/Pong/Player",
                playerTransforms[Client.Instance.PlayerData.DuckID].position,
                playerTransforms[Client.Instance.PlayerData.DuckID].rotation,
                Client.Instance.PlayerData);

            // TODO: Freeze position on the player object based on where they spawned (only move along one axis, but top and bottom are rotated)

            // Set up player scores for everyone in the lobby
            foreach (PlayerData playerData in Client.Instance.PlayersInLobby)
            {
                PlayerScores.Add(playerData.DuckID, 0);
            }

            // Instantiate the Pong Ball as well. this is so the network component can be set up correctly
            if (Client.Instance.SceneHost.DuckID == Client.Instance.PlayerData.DuckID)
            {
                Client.Instance.InstantiateOverNetwork("Prefabs/Pong/Ball", Vector3.zero, Quaternion.identity, Client.Instance.PlayerData);
            }
        }

        /// <summary>
        ///     Called when a goaled is scored in Pong
        /// </summary>
        /// <param name="PlayerID">The DuckID of the player that scored</param>
        public void ScoreGoal(int PlayerID)
        {
            PlayerScores[PlayerID]++;

            // TODO: Think about a good way to display the scores and maybe sort them by score from highest to lowest
            scoreText.text = "Scores: ";
            print("Player scores:");
            foreach (var playerScore in PlayerScores)
            {
                print($"  Player {playerScore.Key}: {playerScore.Value}");
                scoreText.text += playerScore.Key + ": " + playerScore.Value + "\n";
            }
        }
    }
}