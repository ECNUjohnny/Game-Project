using System;
using UnityEngine;

namespace Western.Trains
{
    public sealed class TrainCar : MonoBehaviour
    {
        [Serializable]
        public struct Wheel
        {
            public Transform transform;
            [Min(0.01f)] public float radius;
        }

        [Min(0f)] public float distanceBehindEngine;
        [Min(0.1f)] public float wheelbase = 8f;
        public Wheel[] wheels = Array.Empty<Wheel>();
        private Quaternion[] wheelRotations;
        private float[] wheelAngles;
        private Rigidbody body;

        public void Prepare()
        {
            body = GetComponent<Rigidbody>();
            wheelRotations = new Quaternion[wheels.Length];
            wheelAngles = new float[wheels.Length];
            for (int i = 0; i < wheels.Length; i++)
                if (wheels[i].transform != null)
                    wheelRotations[i] = wheels[i].transform.localRotation;
        }

        public void Place(TrainRoute route, float engineDistance, float signedTravel, bool physics)
        {
            if (wheelRotations == null) Prepare();
            // The locomotive faces decreasing route distance even while reversing.
            float centerDistance = engineDistance + distanceBehindEngine;
            Vector3 front = route.Sample(centerDistance - wheelbase * 0.5f);
            Vector3 rear = route.Sample(centerDistance + wheelbase * 0.5f);
            Vector3 position = (front + rear) * 0.5f;
            Vector3 forward = front - rear;
            Quaternion rotation = forward.sqrMagnitude > 0.000001f
                ? Quaternion.LookRotation(forward, Vector3.up) : transform.rotation;

            if (physics && body != null)
            {
                body.MovePosition(position);
                body.MoveRotation(rotation);
            }
            else
            {
                transform.SetPositionAndRotation(position, rotation);
                if (body != null)
                {
                    body.position = position;
                    body.rotation = rotation;
                }
            }

            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i].transform == null) continue;
                wheelAngles[i] = Mathf.Repeat(wheelAngles[i] -
                    signedTravel / Mathf.Max(0.01f, wheels[i].radius) * Mathf.Rad2Deg, 360f);
                wheels[i].transform.localRotation =
                    wheelRotations[i] * Quaternion.AngleAxis(wheelAngles[i], Vector3.right);
            }
        }
    }
}
