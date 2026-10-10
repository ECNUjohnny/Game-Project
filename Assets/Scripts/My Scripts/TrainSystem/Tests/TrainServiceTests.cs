using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Western.Trains.Tests
{
    public sealed class TrainServiceTests
    {
        private GameObject host;
        private TrainRoute route;
        private TrainController controller;
        private Random.State randomState;

        [SetUp]
        public void SetUp()
        {
            randomState = Random.state;
            Random.InitState(145);
            host = new GameObject("Train service test");
            route = host.AddComponent<TrainRoute>();
            route.Configure(new[] {
                new Vector3(-100,0,0), Vector3.zero, new Vector3(100,0,0),
                new Vector3(100,0,100), new Vector3(0,0,100), Vector3.zero
            }, 1, new[] {
                new TrainRoute.Station { name = "Blackwater", stopDistance = 35 },
                new TrainRoute.Station { name = "East", stopDistance = 200 },
                new TrainRoute.Station { name = "North", stopDistance = 360 }
            });
            controller = host.AddComponent<TrainController>();
            controller.Configure(route, new TrainCar[0]);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(host);
            Random.state = randomState;
        }

        [Test]
        public void SwitchingToLoopPreservesEveryCarPosition()
        {
            foreach (float offset in new[] { -4f, 0f, 4f, 10.3f, 23.2f, 29f })
            {
                float distance = route.LoopStartDistance + 14f + offset;
                Assert.Less(Vector3.Distance(route.Sample(distance),
                    route.Sample(distance + route.LoopLength)), 0.0001f);
            }
        }

        [TestCase(0.02f)]
        [TestCase(0.2f)]
        [TestCase(3.5f)]
        public void TwoCircuitsVisitAllThreeStationsInOrder(float step)
        {
            var arrivals = new List<int>();
            controller.StationArrived += (index, dwell) => {
                arrivals.Add(index);
                Assert.That(dwell, Is.InRange(60f, 120f));
                Assert.That(controller.Speed, Is.EqualTo(0f));
                Assert.That(controller.Distance, Is.EqualTo(route.Stations[index].stopDistance).Within(0.001f));
            };
            float elapsed = 0f;
            while (arrivals.Count < 6 && elapsed < 1600f)
            {
                controller.AdvanceSimulation(step);
                Assert.That(controller.Speed, Is.InRange(0f, 8.001f));
                elapsed += step;
            }
            CollectionAssert.AreEqual(new[] { 2, 1, 0, 2, 1, 0 }, arrivals);
        }

        [Test]
        public void WaitingHoldsTrainAndResumesAfterTheFullDwell()
        {
            for (int i = 0; i < 2000 && controller.State != TrainController.ServiceState.WaitingAtStation; i++)
                controller.AdvanceSimulation(0.1f);
            Assert.AreEqual(TrainController.ServiceState.WaitingAtStation, controller.State);
            float position = controller.Distance;
            float wait = controller.RemainingWait;
            Assert.That(wait, Is.InRange(59.9f, 120f));
            controller.AdvanceSimulation(wait - 0.1f);
            Assert.AreEqual(TrainController.ServiceState.WaitingAtStation, controller.State);
            Assert.AreEqual(position, controller.Distance);
            controller.AdvanceSimulation(0.2f);
            Assert.AreEqual(TrainController.ServiceState.Running, controller.State);
            Assert.Less(controller.Distance, position);
        }

        [Test]
        public void BlackwaterDepartureReversesAndPausesBeforeChangingDirection()
        {
            float start = controller.Distance;
            controller.AdvanceSimulation(1f);
            Assert.Greater(controller.Distance, start);
            for (int i = 0; i < 1000 && controller.State != TrainController.ServiceState.ChangingDirection; i++)
                controller.AdvanceSimulation(0.1f);
            Assert.AreEqual(TrainController.ServiceState.ChangingDirection, controller.State);
            Assert.AreEqual(0f, controller.Speed);
            float distance = controller.Distance;
            controller.AdvanceSimulation(1f);
            Assert.AreEqual(distance, controller.Distance);
            controller.AdvanceSimulation(1.2f);
            Assert.AreEqual(TrainController.ServiceState.Running, controller.State);
            Assert.Less(Vector3.Distance(route.Sample(distance), route.Sample(controller.Distance)), 0.2f);
        }

        [Test]
        public void ZeroElapsedTimeDoesNotMoveTheTrain()
        {
            float distance = controller.Distance;
            controller.AdvanceSimulation(0f);
            Assert.AreEqual(distance, controller.Distance);
        }
    }
}
