using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Physian
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct CopyToBuiltInSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            new CopyToBuiltInJob() { }.ScheduleParallel();
        }

        public void OnDestroy(ref SystemState state)
        {
        }
    }

    public partial struct CopyToBuiltInJob : IJobEntity
    {

        // Adds one to every SampleComponent value
        void Execute(ref LocalTransform transform, in PositionComponent position)
        {
            transform.Position = new Unity.Mathematics.float3(position.x, position.y, position.z);
        }
    }
}
