using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Networking.Core; // 👈 Needed for NetworkComponent reference

public class DuckKiller : MonoBehaviour
{
    #region Variables
    public static DuckKiller Instance;

    private List<GameObject> _allducks = new List<GameObject>();
    [SerializeField] private TMP_Text winnerText;
    #endregion

    private void Awake()
    {
        Instance = this;
    }

    #region Public Functions
    public void RegisterDuck(GameObject duck)
    {
        _allducks.Add(duck);
    }

    public void CheckForWinner()
    {
        int aliveCount = 0;
        GameObject lastAlive = null;

        foreach (GameObject duck in _allducks)
        {
            if (duck.activeSelf)
            {
                aliveCount++;
                lastAlive = duck;
            }
        }

        if (aliveCount == 1)
        {
            string winnerUsername = lastAlive.name;

            NetworkComponent netComp = lastAlive.GetComponent<NetworkComponent>();
            if (netComp != null && !string.IsNullOrEmpty(netComp.Username))
            {
                winnerUsername = netComp.Username;
            }
            winnerText.text = $"Winner: {winnerUsername}";
        }
    }
    #endregion
}