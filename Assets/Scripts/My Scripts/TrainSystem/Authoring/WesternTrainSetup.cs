using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Western.Trains.Editor
{
    /// <summary>Builds the service from the Western track pieces, without moving any track or platform.</summary>
    public static class WesternTrainSetup
    {
        private const string RootName = "Western Train System";
        private const string PrefabFolder = "Assets/Prefabs/Western/Vehicles/";

        private sealed class Piece
        {
            public Transform transform;
            public bool curve;
            public float railHeight;

            public Vector3 Sample(float t)
            {
                float angle = t * Mathf.PI * 0.5f;
                Vector3 local = curve
                    ? new Vector3(25f * (1f - Mathf.Cos(angle)), railHeight, 25f * Mathf.Sin(angle))
                    : new Vector3(0f, railHeight, 10f * t);
                return transform.TransformPoint(local);
            }
        }

        [MenuItem("Tools/Train System/Create Western Three-Station Service")]
        public static void Build()
        {
            if (EditorApplication.isPlaying)
                throw new InvalidOperationException("Exit Play Mode before creating the train.");
            var scene = SceneManager.GetActiveScene();
            if (scene.GetRootGameObjects().Any(g => g.name == RootName))
                throw new InvalidOperationException("Western Train System already exists. Use Rebuild Route instead.");
            Transform trackRoot = scene.GetRootGameObjects()
                .FirstOrDefault(g => g.name == "Train Tracks System")?.transform;
            if (trackRoot == null) throw new InvalidOperationException("Train Tracks System was not found.");

            // Preserve the current scene, including the user's existing changes, before adding the train.
            string backup = Path.GetFullPath(".utmp/train-system/Game.before-train.unity");
            Directory.CreateDirectory(Path.GetDirectoryName(backup));
            if (!File.Exists(backup) && !EditorSceneManager.SaveScene(scene, backup, true))
                throw new IOException("Could not back up the current scene.");

            List<Vector3> points = BuildCenterLine(trackRoot, out int junction);
            Transform[] platforms = FindPlatforms(scene);
            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Western Train Service");
            try
            {
                TrainRoute route = root.AddComponent<TrainRoute>();
                route.Configure(points.ToArray(), junction, Array.Empty<TrainRoute.Station>());
                TrainCar engine = CreateCar("SM_Veh_Train_01.prefab", "Locomotive", root.transform, 0f, 8f);
                float tenderOffset = HalfLength("SM_Veh_Train_01.prefab")
                    + HalfLength("SM_Veh_Train_Coal_01.prefab") + 0.35f;
                TrainCar tender = CreateCar("SM_Veh_Train_Coal_01.prefab", "Coal Tender",
                    root.transform, tenderOffset, 4.5f);
                float coachOffset = tenderOffset + HalfLength("SM_Veh_Train_Coal_01.prefab")
                    + HalfLength("SM_Veh_Train_Carriage_01.prefab") + 0.35f;
                TrainCar coach = CreateCar("SM_Veh_Train_Carriage_01.prefab", "Passenger Coach",
                    root.transform, coachOffset, 11f);
                SetStops(route, platforms, coachOffset);
                TrainController controller = root.AddComponent<TrainController>();
                controller.Configure(route, new[] { engine, tender, coach });
                EditorSceneManager.MarkSceneDirty(scene);
                Selection.activeGameObject = root;
                Debug.Log($"Train ready: {points.Count} route samples, {route.LoopLength:F1} m loop, " +
                    $"{route.LoopStartDistance:F1} m siding, three station stops. " +
                    "Existing rails and platforms were preserved.", root);
            }
            catch
            {
                UnityEngine.Object.DestroyImmediate(root);
                throw;
            }
        }

        [MenuItem("Tools/Train System/Rebuild Route From Existing Tracks")]
        public static void RebuildRoute()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
            var scene = SceneManager.GetActiveScene();
            var controller = scene.GetRootGameObjects().Select(g => g.GetComponent<TrainController>())
                .FirstOrDefault(c => c != null);
            if (controller == null) throw new InvalidOperationException("Create the train service first.");
            Transform trackRoot = scene.GetRootGameObjects().First(g => g.name == "Train Tracks System").transform;
            var points = BuildCenterLine(trackRoot, out int junction);
            Undo.RecordObject(controller.Route, "Rebuild Train Route");
            controller.Route.Configure(points.ToArray(), junction, Array.Empty<TrainRoute.Station>());
            SetStops(controller.Route, FindPlatforms(scene), controller.Cars.Max(c => c.distanceBehindEngine));
            controller.ResetService();
            EditorUtility.SetDirty(controller.Route);
            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static Transform[] FindPlatforms(Scene scene)
        {
            var platforms = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
                .Where(t => t.name.StartsWith("SM_Bld_TrainStation_Platform_01", StringComparison.Ordinal)
                    && !t.name.Contains("_Low")).ToArray();
            if (platforms.Length != 3) throw new InvalidOperationException("Expected exactly three station platforms.");
            return platforms;
        }

        private static void SetStops(TrainRoute route, Transform[] platforms, float coachOffset)
        {
            var stops = platforms.Select(p => new TrainRoute.Station
            {
                platform = p,
                stopDistance = route.ProjectDistance(p.position) - coachOffset
            }).OrderBy(s => s.stopDistance).ToArray();
            for (int i = 0; i < stops.Length; i++)
            {
                stops[i].name = i == 0 ? "Blackwater" :
                    (stops[i].platform.position.z > -400f ? "North Station" : "East Station");
            }
            route.SetStations(stops);
        }

        private static float HalfLength(string prefab)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + prefab);
            return asset.GetComponent<MeshFilter>().sharedMesh.bounds.extents.z;
        }

        private static TrainCar CreateCar(string prefab, string name, Transform parent, float offset, float wheelbase)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + prefab);
            if (asset == null) throw new InvalidOperationException("Missing train prefab: " + prefab);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            instance.name = name;
            foreach (Transform child in instance.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, 0);
            var body = instance.GetComponent<Rigidbody>();
            if (body == null) body = instance.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            var car = instance.AddComponent<TrainCar>();
            car.distanceBehindEngine = offset;
            car.wheelbase = wheelbase;
            car.wheels = instance.GetComponentsInChildren<MeshFilter>(true)
                .Where(f => f.name.Contains("_Wheel_"))
                .Select(f => new TrainCar.Wheel { transform = f.transform,
                    radius = f.sharedMesh.bounds.extents.y }).ToArray();
            return car;
        }

        private static void Append(List<Vector3> points, Vector3 point)
        {
            if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], point) > 0.001f)
                points.Add(point);
        }

        private static List<Vector3> BuildCenterLine(Transform root, out int junctionIndex)
        {
            var pieces = root.Cast<Transform>()
                .Where(t => t.name.StartsWith("SM_Env_Train_Track_Straight_01", StringComparison.Ordinal)
                    || t.name.StartsWith("SM_Env_Train_Track_Curve_01", StringComparison.Ordinal))
                .Select(t => new Piece { transform = t, curve = t.name.Contains("_Curve_"),
                    railHeight = t.GetComponent<MeshFilter>().sharedMesh.bounds.max.y }).ToArray();
            if (pieces.Length < 4) throw new InvalidOperationException("Not enough track pieces.");

            var endpoints = pieces.SelectMany(p => new[] { p.Sample(0f), p.Sample(1f) }).ToArray();
            int[] nearest = new int[endpoints.Length];
            for (int i = 0; i < endpoints.Length; i++)
            {
                float best = float.PositiveInfinity;
                for (int j = 0; j < endpoints.Length; j++)
                {
                    if (i / 2 == j / 2) continue;
                    float squared = (endpoints[i] - endpoints[j]).sqrMagnitude;
                    if (squared < best) { best = squared; nearest[i] = j; }
                }
            }
            int[] links = nearest.Select((other, i) => nearest[other] == i ? other : -1).ToArray();
            var branches = Enumerable.Range(0, pieces.Length)
                .Where(i => pieces[i].curve && links[i * 2] < 0 && links[i * 2 + 1] < 0).ToArray();
            if (branches.Length != 1)
                throw new InvalidOperationException("Expected one overlapping turnout curve; inspect track connections.");
            int branch = branches[0];
            var openEnds = Enumerable.Range(0, endpoints.Length)
                .Where(i => i / 2 != branch && links[i] < 0).ToArray();
            if (openEnds.Length != 2) throw new InvalidOperationException("Track has more than one open siding.");

            // The far open end is the end of the Blackwater siding. Follow every other piece to the curve.
            int incoming = openEnds.OrderByDescending(i =>
                Vector3.Distance(endpoints[i], pieces[branch].transform.position)).First();
            var points = new List<Vector3>();
            var visited = new HashSet<int>();
            int branchEntry = -1;
            while (true)
            {
                int pieceIndex = incoming / 2;
                if (!visited.Add(pieceIndex)) throw new InvalidOperationException("Unexpected cycle before turnout.");
                int outgoing = incoming ^ 1;
                float start = incoming % 2;
                float finish = 1f - start;
                bool last = links[outgoing] < 0;
                if (last)
                {
                    if (pieces[pieceIndex].curve)
                        throw new InvalidOperationException("Turnout must overlap a straight track.");
                    Vector3 a = pieces[pieceIndex].Sample(0f);
                    Vector3 delta = pieces[pieceIndex].Sample(1f) - a;
                    float best = float.PositiveInfinity;
                    for (int e = 0; e < 2; e++)
                    {
                        Vector3 point = pieces[branch].Sample(e);
                        float t = Mathf.Clamp01(Vector3.Dot(point - a, delta) / delta.sqrMagnitude);
                        float squared = (a + t * delta - point).sqrMagnitude;
                        if (squared < best) { best = squared; finish = t; branchEntry = e; }
                    }
                    if (best > 1f)
                        throw new InvalidOperationException("Turnout curve is more than one metre from the return track.");
                }
                int samples = Mathf.CeilToInt(Mathf.Abs(finish - start) *
                    (pieces[pieceIndex].curve ? 40f : 10f) / 0.5f);
                for (int i = 0; i <= samples; i++)
                    Append(points, pieces[pieceIndex].Sample(Mathf.Lerp(start, finish, i / (float)Mathf.Max(1, samples))));
                if (last) break;
                if (Vector3.Distance(endpoints[outgoing], endpoints[links[outgoing]]) > 1.5f)
                    throw new InvalidOperationException("A track connection has a gap larger than 1.5 metres.");
                incoming = links[outgoing];
            }
            if (visited.Count != pieces.Length - 1)
                throw new InvalidOperationException("Some track pieces are disconnected.");

            Vector3 turnout = pieces[branch].Sample(1 - branchEntry);
            float bestTurnout = float.PositiveInfinity;
            int insertAt = -1;
            Vector3 projected = Vector3.zero;
            for (int i = 1; i < points.Count; i++)
            {
                Vector3 delta = points[i] - points[i - 1];
                float t = Mathf.Clamp01(Vector3.Dot(turnout - points[i - 1], delta) / delta.sqrMagnitude);
                Vector3 candidate = points[i - 1] + t * delta;
                float squared = (candidate - turnout).sqrMagnitude;
                if (squared < bestTurnout) { bestTurnout = squared; insertAt = i; projected = candidate; }
            }
            if (bestTurnout > 1f || insertAt < 1)
                throw new InvalidOperationException("Turnout does not meet the station straight.");
            if (Vector3.Distance(projected, points[insertAt - 1]) < 0.001f) junctionIndex = insertAt - 1;
            else if (Vector3.Distance(projected, points[insertAt]) < 0.001f) junctionIndex = insertAt;
            else { points.Insert(insertAt, projected); junctionIndex = insertAt; }
            Vector3 junction = points[junctionIndex];

            for (int i = 0; i <= 80; i++)
                Append(points, pieces[branch].Sample(Mathf.Lerp(branchEntry, 1 - branchEntry, i / 80f)));
            // Use the shared physical intersection for both visits to the turnout.
            points[points.Count - 1] = junction;
            return points;
        }
    }
}
