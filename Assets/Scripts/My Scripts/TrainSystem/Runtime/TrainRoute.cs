using System;
using UnityEngine;

namespace Western.Trains
{
    /// <summary>An open station siding followed by one complete circuit back to the turnout.</summary>
    public sealed class TrainRoute : MonoBehaviour
    {
        [Serializable]
        public sealed class Station
        {
            public string name;
            public Transform platform;
            [Tooltip("Locomotive distance at which the passenger carriage is beside the platform.")]
            public float stopDistance;
        }

        [SerializeField] private Vector3[] points = Array.Empty<Vector3>();
        [SerializeField] private int loopStartPoint;
        [SerializeField] private Station[] stations = Array.Empty<Station>();
        private Vector3[] worldPoints;
        private float[] cumulative;

        public float Length => cumulative == null ? 0f : cumulative[cumulative.Length - 1];
        public float LoopStartDistance => cumulative == null ? 0f : cumulative[loopStartPoint];
        public float LoopLength => Length - LoopStartDistance;
        public Station[] Stations => stations;
        public int PointCount => points.Length;

        public void Configure(Vector3[] worldPositions, int junctionIndex, Station[] stops)
        {
            points = new Vector3[worldPositions.Length];
            for (int i = 0; i < points.Length; i++)
                points[i] = transform.InverseTransformPoint(worldPositions[i]);
            loopStartPoint = junctionIndex;
            stations = stops;
            Rebuild();
        }

        public void SetStations(Station[] stops)
        {
            stations = stops;
        }

        public void Rebuild()
        {
            if (points == null || points.Length < 3 || loopStartPoint < 1 ||
                loopStartPoint >= points.Length - 1)
                throw new InvalidOperationException("Train route needs a siding and a complete loop.");

            worldPoints = new Vector3[points.Length];
            cumulative = new float[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                worldPoints[i] = transform.TransformPoint(points[i]);
                if (i > 0)
                {
                    float length = Vector3.Distance(worldPoints[i - 1], worldPoints[i]);
                    if (length < 0.0001f)
                        throw new InvalidOperationException("Train route contains duplicate consecutive points.");
                    cumulative[i] = cumulative[i - 1] + length;
                }
            }
            if (LoopLength < 1f || Vector3.Distance(worldPoints[loopStartPoint],
                    worldPoints[worldPoints.Length - 1]) > 0.1f)
                throw new InvalidOperationException("Train route loop must end at its turnout.");
        }

        // Distances beyond the end continue onto the straight after the turnout.
        // This lets the whole train clear the switch before entering the circuit.
        public Vector3 Sample(float distance)
        {
            if (cumulative == null)
                Rebuild();
            if (distance > Length)
                distance = LoopStartDistance + Mathf.Repeat(distance - LoopStartDistance, LoopLength);
            distance = Mathf.Clamp(distance, 0f, Length);

            int low = 0;
            int high = cumulative.Length - 1;
            while (high - low > 1)
            {
                int middle = (low + high) / 2;
                if (cumulative[middle] <= distance) low = middle;
                else high = middle;
            }
            float t = (distance - cumulative[low]) / (cumulative[high] - cumulative[low]);
            return Vector3.Lerp(worldPoints[low], worldPoints[high], t);
        }

        public float ProjectDistance(Vector3 position)
        {
            if (cumulative == null)
                Rebuild();
            float bestSquaredDistance = float.PositiveInfinity;
            float bestDistance = 0f;
            for (int i = 1; i < worldPoints.Length; i++)
            {
                Vector3 delta = worldPoints[i] - worldPoints[i - 1];
                float t = Mathf.Clamp01(Vector3.Dot(position - worldPoints[i - 1], delta) / delta.sqrMagnitude);
                float squaredDistance = (position - (worldPoints[i - 1] + t * delta)).sqrMagnitude;
                if (squaredDistance < bestSquaredDistance)
                {
                    bestSquaredDistance = squaredDistance;
                    bestDistance = Mathf.Lerp(cumulative[i - 1], cumulative[i], t);
                }
            }
            return bestDistance;
        }

        private void OnDrawGizmosSelected()
        {
            if (points == null || points.Length < 2) return;
            Gizmos.color = new Color(0.1f, 0.85f, 1f);
            for (int i = 1; i < points.Length; i++)
                Gizmos.DrawLine(transform.TransformPoint(points[i - 1]), transform.TransformPoint(points[i]));
            Gizmos.color = Color.yellow;
            foreach (Station station in stations)
                if (station.platform != null)
                    Gizmos.DrawWireSphere(station.platform.position + Vector3.up, 2f);
        }
    }
}
