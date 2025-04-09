using UnityEngine;

public class TileDisabler : MonoBehaviour
{
    #region Variables
    [SerializeField] private float tileDisablingDelay = 3f;
    private bool _isTriggered = false;
    #endregion

    private void Start()
    {
        EnableTile();
    }

    #region Private Functions
    private void OnCollisionEnter(Collision collision)
    {
        if (_isTriggered) return;

        _isTriggered = true;
        Invoke(nameof(DisableTile), tileDisablingDelay);
    }

    private void DisableTile()
    {
        gameObject.SetActive(false);
    }

    private void EnableTile()
    {
        _isTriggered = false;
        gameObject.SetActive(true);
    }
    #endregion
}