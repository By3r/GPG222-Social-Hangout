using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Networking.Core.WaterSimulation
{
    [RequireComponent(typeof(Rigidbody))]
    public class BouyancyObject : MonoBehaviour
    {
        public float UnderWaterDrag = 3f;
        public float UnderWaterAngularDrag = 1f;
        public float AirDrag = 0f;
        public float AirAngularDrag = 0.05f;
        public float FloatingPower = 15f;
        
        public Transform[] Floaters;
        public float WaterHeight = 0f;
        private int _floatersUnderWater;
        public int FloatersUnderWaterThreshold;
        
        private Rigidbody _rigidbody;
        [SerializeField] private bool _isUnderWater;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();

            // Automatically grab all the floating point transforms
            var floaters = GetComponentsInChildren<Floater>();
            Floaters = new Transform[floaters.Length];
            for (int i = 0; i < floaters.Length; i++)
            {
                Floaters[i] = floaters[i].transform;
            }
        }

        private void FixedUpdate()
        {
            _floatersUnderWater = 0;
            foreach (var floater in Floaters)
            {
                float difference = floater.position.y - WaterHeight;

                if (difference < 0)
                {
                    _floatersUnderWater++;
                    _rigidbody.AddForceAtPosition(Vector3.up * FloatingPower * Mathf.Abs(difference), floater.position, ForceMode.Force);

                    if (!_isUnderWater && _floatersUnderWater >= FloatersUnderWaterThreshold)
                    {
                        _isUnderWater = true;
                        SwitchState(_isUnderWater);
                    }
                }
            }
            
            
            if (_isUnderWater && _floatersUnderWater <= FloatersUnderWaterThreshold)
            {
                _isUnderWater = false;
                SwitchState(_isUnderWater);
            }
        }

        private void SwitchState(bool isUnderWater)
        {
            if (isUnderWater)
            {
                _rigidbody.drag = UnderWaterDrag;
                _rigidbody.angularDrag = UnderWaterAngularDrag;
            }
            else
            {
                _rigidbody.drag = AirDrag;
                _rigidbody.angularDrag = AirAngularDrag;
            }
        }
    }
}