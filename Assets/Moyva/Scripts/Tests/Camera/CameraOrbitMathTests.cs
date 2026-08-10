using Kruty1918.Moyva.Camera.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Camera
{
    public sealed class CameraOrbitMathTests
    {
        [TestCase(30)]
        [TestCase(60)]
        [TestCase(144)]
        public void AccelerateHoldAndRelease_ProducesSameAngleAtEveryFps(int fps)
        {
            SimulationResult baseline = Simulate(60);
            SimulationResult actual = Simulate(fps);

            Assert.That(actual.Angle, Is.EqualTo(baseline.Angle).Within(0.0005f));
            Assert.That(actual.Velocity, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void StepThatReachesTarget_IntegratesRemainingConstantVelocityTime()
        {
            float velocity = CameraOrbitMath.Advance(
                currentVelocity: 0f,
                targetVelocity: 90f,
                acceleration: 180f,
                deltaTime: 1f,
                out float angle);

            Assert.That(velocity, Is.EqualTo(90f).Within(0.0001f));
            Assert.That(angle, Is.EqualTo(67.5f).Within(0.0001f));
        }

        private static SimulationResult Simulate(int fps)
        {
            float deltaTime = 1f / fps;
            float velocity = 0f;
            float angle = 0f;

            // One second held, followed by one second of critically short release.
            for (int frame = 0; frame < fps; frame++)
            {
                velocity = CameraOrbitMath.Advance(
                    velocity,
                    targetVelocity: 90f,
                    acceleration: 540f,
                    deltaTime,
                    out float stepAngle);
                angle += stepAngle;
            }

            for (int frame = 0; frame < fps; frame++)
            {
                velocity = CameraOrbitMath.Advance(
                    velocity,
                    targetVelocity: 0f,
                    acceleration: 720f,
                    deltaTime,
                    out float stepAngle);
                angle += stepAngle;
            }

            return new SimulationResult(angle, velocity);
        }

        private readonly struct SimulationResult
        {
            public SimulationResult(float angle, float velocity)
            {
                Angle = angle;
                Velocity = velocity;
            }

            public float Angle { get; }
            public float Velocity { get; }
        }
    }
}
