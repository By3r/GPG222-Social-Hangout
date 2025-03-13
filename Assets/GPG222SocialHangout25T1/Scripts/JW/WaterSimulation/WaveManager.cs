using UnityEngine;

namespace GPG222SocialHangout25T1.Scripts.JW.WaterSimulation
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager instance;

        [SerializeField] float amplitdue = 1f;
        [SerializeField] float length = 2f;
        [SerializeField] float speed = 2f;
        [SerializeField] float offset = 0f;
    
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.LogWarning("WaveManager instance already exists, destroying object");
                Destroy(this);
            }
        }

        void Update()
        {
            offset += Time.deltaTime * speed;
        }

        public float GetWaveHeight(float _x)
        {
            return amplitdue * Mathf.Sin(_x / length + offset); 
        }
    }
}
