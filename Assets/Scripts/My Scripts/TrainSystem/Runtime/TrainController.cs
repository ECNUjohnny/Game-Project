using System;
using UnityEngine;

namespace Western.Trains
{
    /// <summary>Three-station service with a reversing move at the Blackwater siding.</summary>
    public sealed class TrainController : MonoBehaviour
    {
        public enum ServiceState { ReversingOut, ChangingDirection, Running, WaitingAtStation }

        [SerializeField] private TrainRoute route;
        [SerializeField] private TrainCar[] cars = Array.Empty<TrainCar>();
        [Header("Movement (metres / seconds)")]
        [SerializeField, Min(0.1f)] private float cruiseSpeed = 8f;
        [SerializeField, Min(0.1f)] private float reversingSpeed = 3f;
        [SerializeField, Min(0.1f)] private float acceleration = 0.8f;
        [SerializeField, Min(0.1f)] private float braking = 1.2f;
        [Header("Station dwell (game seconds; pauses with the game)")]
        [SerializeField, Min(0f)] private float minimumDwell = 60f;
        [SerializeField, Min(0f)] private float maximumDwell = 120f;
        [Header("Turnout")]
        [Tooltip("The locomotive, at the rear of the reversing train, must clear the turnout.")]
        [SerializeField, Min(8f)] private float clearanceDistance = 14f;
        [SerializeField, Min(0f)] private float directionChangeDelay = 2f;
        [Header("Live service status")]
        [SerializeField] private ServiceState state;
        [SerializeField] private float distance;
        [SerializeField] private float speed;
        [SerializeField] private float remainingWait;
        [SerializeField] private int stationIndex;
        private bool initialized;

        public TrainRoute Route => route;
        public TrainCar[] Cars => cars;
        public ServiceState State => state;
        public float Distance => distance;
        public float Speed => speed;
        public float RemainingWait => remainingWait;
        public int StationIndex => stationIndex;
        public event Action<int, float> StationArrived;
        public event Action<int> StationDeparted;

        public void Configure(TrainRoute trainRoute, TrainCar[] trainCars)
        {
            route = trainRoute;
            cars = trainCars;
            ResetService();
        }

        [ContextMenu("Reset Train To Blackwater")]
        public void ResetService()
        {
            initialized = false;
            if (route == null) throw new InvalidOperationException("Train has no route.");
            route.Rebuild();
            if (route.Stations == null || route.Stations.Length != 3)
                throw new InvalidOperationException("This service requires three stations.");
            for (int i = 0; i < route.Stations.Length; i++)
            {
                float stop = route.Stations[i].stopDistance;
                if (stop <= 7f || stop >= route.Length ||
                    (i > 0 && stop <= route.Stations[i - 1].stopDistance))
                    throw new InvalidOperationException("Station distances must be valid and ordered along the route.");
            }
            if (route.Stations[0].stopDistance >= route.LoopStartDistance ||
                route.Stations[1].stopDistance <= route.LoopStartDistance + clearanceDistance)
                throw new InvalidOperationException("Blackwater must be on the siding, with room to clear the turnout.");

            distance = route.Stations[0].stopDistance;
            speed = 0f;
            remainingWait = 0f;
            stationIndex = 0;
            state = ServiceState.ReversingOut;
            foreach (TrainCar car in cars)
                if (car != null) car.Prepare();
            initialized = true;
            PlaceCars(0f, false);
        }

        private void Start()
        {
            try { ResetService(); }
            catch (Exception exception)
            {
                Debug.LogError("Train service could not start: " + exception.Message, this);
                enabled = false;
            }
        }

        private void FixedUpdate()
        {
            if (!initialized) return;
            float travel = AdvanceSimulation(Time.fixedDeltaTime);
            PlaceCars(travel, true);
        }

        // Small integration steps preserve braking and station stops even after a long frame.
        internal float AdvanceSimulation(float deltaTime)
        {
            if (!initialized || deltaTime <= 0f) return 0f;
            float travelled = 0f;
            while (deltaTime > 0.000001f)
            {
                if (state == ServiceState.WaitingAtStation || state == ServiceState.ChangingDirection)
                {
                    float consumed = Mathf.Min(deltaTime, remainingWait);
                    remainingWait -= consumed;
                    deltaTime -= consumed;
                    if (remainingWait > 0.000001f) break;
                    remainingWait = 0f;
                    if (state == ServiceState.ChangingDirection)
                    {
                        // Same physical pose on the straight; next movement now takes the curve.
                        distance += route.LoopLength;
                        stationIndex = route.Stations.Length - 1;
                        state = ServiceState.Running;
                    }
                    else
                    {
                        StationDeparted?.Invoke(stationIndex);
                        if (stationIndex == 0) state = ServiceState.ReversingOut;
                        else { stationIndex--; state = ServiceState.Running; }
                    }
                    continue;
                }

                float step = Mathf.Min(deltaTime, 0.02f);
                deltaTime -= step;
                bool reverse = state == ServiceState.ReversingOut;
                float direction = reverse ? 1f : -1f;
                float target = reverse ? route.LoopStartDistance + clearanceDistance
                    : route.Stations[stationIndex].stopDistance;
                float remainingDistance = Mathf.Max(0f, direction * (target - distance));
                float limit = reverse ? reversingSpeed : cruiseSpeed;
                float desiredSpeed = Mathf.Min(limit, Mathf.Sqrt(2f * braking * remainingDistance));
                float previousSpeed = speed;
                speed = Mathf.MoveTowards(speed, desiredSpeed,
                    (desiredSpeed < speed ? braking : acceleration) * step);
                float movement = Mathf.Min(remainingDistance, (previousSpeed + speed) * 0.5f * step);
                distance += direction * movement;
                travelled += direction * movement;

                if (remainingDistance - movement > 0.002f) continue;
                travelled += target - distance;
                distance = target;
                speed = 0f;
                if (reverse)
                {
                    state = ServiceState.ChangingDirection;
                    remainingWait = directionChangeDelay;
                }
                else
                {
                    state = ServiceState.WaitingAtStation;
                    remainingWait = UnityEngine.Random.Range(minimumDwell, maximumDwell);
                    StationArrived?.Invoke(stationIndex, remainingWait);
                }
            }
            return travelled;
        }

        private void PlaceCars(float travel, bool physics)
        {
            foreach (TrainCar car in cars)
                if (car != null) car.Place(route, distance, travel, physics);
        }

        private void OnValidate()
        {
            cruiseSpeed = Mathf.Max(0.1f, cruiseSpeed);
            reversingSpeed = Mathf.Max(0.1f, reversingSpeed);
            acceleration = Mathf.Max(0.1f, acceleration);
            braking = Mathf.Max(0.1f, braking);
            minimumDwell = Mathf.Max(0f, minimumDwell);
            maximumDwell = Mathf.Max(minimumDwell, maximumDwell);
            clearanceDistance = Mathf.Max(8f, clearanceDistance);
            directionChangeDelay = Mathf.Max(0f, directionChangeDelay);
        }
    }
}
