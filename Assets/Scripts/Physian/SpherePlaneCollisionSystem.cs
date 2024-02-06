using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Physian
{
    // https://mathworld.wolfram.com/Reflection.html
    //
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(MotionSystem))]
    public partial class SpherePlaneCollisionSystem : SystemBase
    {
        EntityQuery planeQuery;

        protected override void OnCreate()
        {
            planeQuery = GetEntityQuery(
                ComponentType.ReadOnly<PositionComponent>(),
                ComponentType.ReadOnly<PlaneColliderComponent>()
                );
        }


        protected override void OnUpdate()
        {
            JobHandle dependencyHandle = Dependency;

            NativeList<PositionComponent> allPositions = planeQuery.ToComponentDataListAsync<PositionComponent>(Allocator.TempJob, dependencyHandle, out JobHandle positionQueryHandle);
            NativeList<PlaneColliderComponent> allPlanes = planeQuery.ToComponentDataListAsync<PlaneColliderComponent>(Allocator.TempJob, dependencyHandle, out JobHandle planeQueryHandle);

            dependencyHandle = JobHandle.CombineDependencies(positionQueryHandle, planeQueryHandle);
            JobHandle collisionJobHandle = new SpherePlaneCollisionJob()
            {
                planePositions = allPositions,
                otherPlanes = allPlanes,
                deltaTime = SystemAPI.Time.fixedDeltaTime
            }.Schedule(dependencyHandle);

            allPositions.Dispose(collisionJobHandle);
            allPlanes.Dispose(collisionJobHandle);

            Dependency = collisionJobHandle;
        }
    }
    public partial struct SpherePlaneCollisionJob : IJobEntity
    {
        public NativeList<PositionComponent> planePositions;
        public NativeList<PlaneColliderComponent> otherPlanes;
        public float deltaTime;

        // So a lot of this assmes the sphere is a point. Must improve this later.
        void Execute([EntityIndexInQuery] int index,
            ref VelocityComponent sphereVelocity,
            in PositionComponent spherePosition,
            in SphereColliderComponent sphereCollider,
            in MassComponent mass)
        {
            for (int i = 0; i < planePositions.Length; i++)
            {
                //Debug.Log($"Comparing {i} - {index}: {position.v} - {otherPositions[i].v}  :: {velocity.v} - {otherVelocities[i].v}");

                float3 planeNormal = otherPlanes[i].normal;
                float3 sphereDirection = math.normalize(sphereVelocity.v);
                float lineDotPlane = math.dot(sphereDirection, otherPlanes[i].normal);
                if (lineDotPlane == 0)
                    continue; // parrallel, does not intersect

                // Without this part the collision is calculated against the centre of the sphere. Instead we want to apply it at the point that touches the plane.
                float3 directionOfPlane = math.normalize(otherPlanes[i].normal * lineDotPlane);
                float3 sphereEdgeOffset = directionOfPlane * sphereCollider.radius;
                float3 sphereEdge = spherePosition.v + sphereEdgeOffset;
                //UnityEngine.Debug.DrawLine(spherePosition.v, sphereEdge, Color.red);

                float d = math.dot(planePositions[i].v - sphereEdge, otherPlanes[i].normal) / lineDotPlane;
                float3 planeIntersectionPoint = sphereEdge + (d * sphereDirection);

                if (!WithinRange(planeIntersectionPoint, sphereEdge, sphereVelocity, sphereCollider, planePositions[i], otherPlanes[i]))
                    continue;

                float distanceAlongNormal = math.dot(planeIntersectionPoint - sphereEdge, -planeNormal);

                //  x' = x - 2 D n
                float3 midPoint = planeIntersectionPoint + distanceAlongNormal * planeNormal;
                float3 reflectedDirection = math.normalize(2 * midPoint - sphereEdge - planeIntersectionPoint);
                float speed = math.length(sphereVelocity.v);

                sphereVelocity = new VelocityComponent(reflectedDirection * speed);

                UnityEngine.Debug.DrawRay(planeIntersectionPoint, -sphereDirection, Color.blue, 1f);
                UnityEngine.Debug.DrawRay(planeIntersectionPoint, sphereVelocity.v, Color.blue, 1f);
                UnityEngine.Debug.DrawLine(planeIntersectionPoint, midPoint, Color.magenta, 1f);
            }
        }

        private bool WithinRange(in float3 planeIntersectionPoint, in float3 position, in VelocityComponent sphereVelocity, in SphereColliderComponent sphere, in PositionComponent planePosition, in PlaneColliderComponent plane)
        {
            // Does this also catch spheres behind he plane? - No :(
            float3 intersectionDistance = planeIntersectionPoint - position;
            //UnityEngine.Debug.Log($"WithinRange: {sphereVelocity.v} | {position} -> {planeIntersectionPoint} | {math.lengthsq(intersectionDistance)} > {math.lengthsq(sphereVelocity.v * deltaTime)}");
            if (math.lengthsq(intersectionDistance) > math.lengthsq(sphereVelocity.v * deltaTime))
               return false;

            float3 intersectionOffset = planeIntersectionPoint - planePosition.v;
            float distanceRight = math.dot(plane.right, intersectionOffset);
            if (math.abs(distanceRight) > plane.size.x * 0.5f)
                return false;
            float distanceForward = math.dot(plane.forward, intersectionOffset);
            if (math.abs(distanceForward) > plane.size.y * 0.5f)
                return false;

            return true;
        }
    }
}
