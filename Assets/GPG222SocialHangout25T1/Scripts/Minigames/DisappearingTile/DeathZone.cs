using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        TryKillDuck(other.gameObject);
    }

    private void TryKillDuck(GameObject obj)
    {
        GameObject duck = obj.transform.root.gameObject;
        Debug.Log($"[DeathZone] Duck hit: {duck.name}");

        duck.SetActive(false);
        DuckKiller.Instance?.CheckForWinner();
    }
}
