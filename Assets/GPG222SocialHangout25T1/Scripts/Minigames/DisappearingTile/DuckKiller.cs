using System.Collections.Generic;
using UnityEngine;

public class DuckKiller : MonoBehaviour
{
    public static DuckKiller Instance;

    private List<GameObject> _allducks = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

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
            Debug.Log($" COngrats {lastAlive.name} !!!");
            // TODO: Trigger win UI
        }
    }
}
