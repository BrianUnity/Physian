using Unity.Entities;
using Unity.Jobs;

namespace Physian
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class MotionSystem : SystemBase
    {
        public JobHandle systemJobHandle;

        protected override void OnUpdate()
        {
            Dependency = new MoveUnderInertiaJob() { deltaTime = SystemAPI.Time.fixedDeltaTime }.ScheduleParallel(Dependency);
        }
    }

    public partial struct MoveUnderInertiaJob : IJobEntity
    {
        public float deltaTime { set; private get; }

        // Adds one to every SampleComponent value
        void Execute(ref PositionComponent position, in VelocityComponent velocity)
        {
            position.x += velocity.x * deltaTime;
            position.y += velocity.y * deltaTime;
            position.z += velocity.z * deltaTime;
        }
    }
}
