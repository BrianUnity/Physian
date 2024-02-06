using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Physian
{
    public struct PlaneColliderComponent : IComponentData
    {
        public float3 normal;
        public float3 forward;
        public float3 right;
        public float2 size;
    }

    public class PlaneColliderAuthoring : MonoBehaviour
    {
        public float2 size = Vector2.one;
    }

    public class PlaneColliderBaker : Baker<PlaneColliderAuthoring>
    {
        public override void Bake(PlaneColliderAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlaneColliderComponent
            {
                normal = authoring.transform.up,
                forward = authoring.transform.forward,
                right = authoring.transform.right,
                size = authoring.size
            });
        }
    }
}
