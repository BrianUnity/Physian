using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Physian
{

    //https://en.wikipedia.org/wiki/Elastic_collision
    //
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(MotionSystem))]
    public partial class SphereSphereCollisionSystem : SystemBase
    {
        EntityQuery colliderQuery;

        protected override void OnCreate()
        {
            colliderQuery = GetEntityQuery(
                ComponentType.ReadOnly<PositionComponent>(),
                ComponentType.ReadOnly<VelocityComponent>(),
                ComponentType.ReadOnly<SphereColliderComponent>(),
                ComponentType.ReadOnly<MassComponent>()
                );
        }


        protected override void OnUpdate()
        {
            JobHandle dependencyHandle = Dependency;

            NativeList<PositionComponent> allPositions = colliderQuery.ToComponentDataListAsync<PositionComponent>(Allocator.TempJob, dependencyHandle, out JobHandle positionQueryHandle);
            NativeList<VelocityComponent> allVelocities = colliderQuery.ToComponentDataListAsync<VelocityComponent>(Allocator.TempJob, dependencyHandle, out JobHandle velocityQueryHandle);
            NativeList<SphereColliderComponent> allSphereColliders = colliderQuery.ToComponentDataListAsync<SphereColliderComponent>(Allocator.TempJob, dependencyHandle, out JobHandle radiusQueryHandle);
            NativeList<MassComponent> allMass = colliderQuery.ToComponentDataListAsync<MassComponent>(Allocator.TempJob, dependencyHandle, out JobHandle massQueryHandle);

            dependencyHandle = JobHandle.CombineDependencies(JobHandle.CombineDependencies(positionQueryHandle, velocityQueryHandle), JobHandle.CombineDependencies(radiusQueryHandle, massQueryHandle));
            JobHandle collisionJobHandle = new SphereSphereCollisionJob()
            {
                otherPositions = allPositions,
                otherVelocities = allVelocities,
                otherSphereColliders = allSphereColliders,
                otherMass = allMass
            }.Schedule(dependencyHandle);

            allPositions.Dispose(collisionJobHandle);
            allVelocities.Dispose(collisionJobHandle);
            allSphereColliders.Dispose(collisionJobHandle);
            allMass.Dispose(collisionJobHandle);

            Dependency = collisionJobHandle;
        }
    }

    public partial struct SphereSphereCollisionJob : IJobEntity
    {
        public NativeList<PositionComponent> otherPositions;
        public NativeList<VelocityComponent> otherVelocities;
        public NativeList<SphereColliderComponent> otherSphereColliders;
        public NativeList<MassComponent> otherMass;

        // Adds one to every SampleComponent value
        void Execute([EntityIndexInQuery] int index,
            ref VelocityComponent velocity,
            in PositionComponent position,
            in SphereColliderComponent sphereCollider,
            in MassComponent mass)
        {
            for (int i = 0; i < otherVelocities.Length; i++)
            {
                //Debug.Log($"Comparing {i} - {index}: {position.v} - {otherPositions[i].v}  :: {velocity.v} - {otherVelocities[i].v}");

                if (i == index)
                    continue;
                if (!WithinRange(position, velocity, sphereCollider, otherPositions[i], otherVelocities[i], otherSphereColliders[i]))
                    continue;

                float massSum = mass.mass + otherMass[i].mass;
                float massDiff = mass.mass - otherMass[i].mass;

                Debug.DrawLine(position.v, otherPositions[i].v, Color.red, 1f);

                float3 newV = velocity.v * (massDiff / massSum) +
                                otherVelocities[i].v * (2f * otherMass[i].mass / massSum);
                velocity = new VelocityComponent(newV);
            }
        }

        private bool WithinRange(in PositionComponent p1, in VelocityComponent v1, in SphereColliderComponent sphere1, in PositionComponent p2, in VelocityComponent v2, in SphereColliderComponent sphere2)
        {
            // Just checking if there is an overlap at the start but really this should find the closest position on the two lines and... apply the collsiion at the correct(!?) position on that line?
            float3 positionDiff = p1.v - p2.v;
            float combinedRadii = sphere1.radius + sphere2.radius;

            return math.lengthsq(positionDiff) < (combinedRadii * combinedRadii);
        }
    }
}
