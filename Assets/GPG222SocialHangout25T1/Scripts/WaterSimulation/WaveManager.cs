using System;
using TMPro;
using UnityEngine;

namespace Networking.Core.WaterSimulation
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance;

        [Header("Wave Parameters")]
        public float Amplitude = 1f;
        public float Length = 2f;
        public float Speed = 1f;
        public float Offset = 0f;

        [Header("Debug")] 
        public Transform OffseTransform;
        
        private void Awake()
        {
            // Set up Singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this);
            }
        }

        private void Update()
        {
            Offset += Speed * Time.deltaTime;
        }

        public float GetWaveHeight(float x)
        {
            return Amplitude * Mathf.Sin(x / Length + Offset);
        }
    }
}