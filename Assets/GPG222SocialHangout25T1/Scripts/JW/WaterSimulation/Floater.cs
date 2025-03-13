using UnityEngine;

namespace GPG222SocialHangout25T1.Scripts.JW.WaterSimulation
{
    public class Floater : MonoBehaviour
    {
        public Rigidbody rigidBody;
        public float depthBeforeSubmerged = 1f;
        public float displacementAmount = 3f;
        public int floaterCount = 1;
        public float waterDrag = .99f;
        public float waterAngularDrag = .5f;

        private void Awake()
        {
            if (rigidBody == null)
            {
                rigidBody = GetComponent<Rigidbody>();
            }
        }

        private void FixedUpdate()
        {
            rigidBody.AddForceAtPosition(Physics.gravity / floaterCount, rigidBody.transform.position, ForceMode.Acceleration);
            
            float waveHeight = WaveManager.instance.GetWaveHeight(transform.position.x);
            if (transform.position.y < waveHeight)
            {
                float displacementMultiplier = Mathf.Clamp01((waveHeight - transform.position.y) / depthBeforeSubmerged) * displacementAmount;
                rigidBody.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f), transform.position,  ForceMode.Force);
                rigidBody.AddForce(-rigidBody.velocity * (displacementMultiplier * waterDrag * Time.deltaTime), ForceMode.VelocityChange);
                rigidBody.AddTorque(-rigidBody.angularVelocity * (displacementMultiplier * waterAngularDrag * Time.deltaTime), ForceMode.VelocityChange);
            }
        }
    }
}
