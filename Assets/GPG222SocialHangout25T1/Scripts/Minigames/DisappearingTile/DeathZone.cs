using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Duck"))
        {
            collision.gameObject.SetActive(false);
            DuckKiller.Instance.CheckForWinner();
        }
    }
}
