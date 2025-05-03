using System;
using TMPro;
using UnityEngine;

namespace Networking.Core.WaterSimulation
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance;

        [Header("Wave Parameters")] 
        public Mesh WaterMesh;
        public float Amplitude = 1f;
        public float Length = 2f;
        public float Speed = 1f;
        public float Offset = 0f;
        
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
            
            WaterMesh = GetComponent<MeshFilter>().mesh;
        }

        private void Update()
        {
            Offset += Speed * Time.deltaTime;
        }

        public float GetWaveHeight(float x)
        {
            return Amplitude * Mathf.Sin(x / Length + Offset);
        }

        public Vector3 GetNearestVertex(Vector3 point)
        {
            point = transform.InverseTransformPoint(point);
            float minDistance = Mathf.Infinity;
            Vector3 nearestVertex = Vector3.zero;

            foreach (Vector3 vertex in WaterMesh.vertices)
            {
                Vector3 difference = point - vertex;
                float distance = difference.sqrMagnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestVertex = vertex;
                }
            }
            
            return nearestVertex;
        }
    }
}