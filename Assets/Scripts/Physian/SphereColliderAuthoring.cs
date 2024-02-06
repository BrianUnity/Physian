using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Physian
{
    public struct SphereColliderComponent : IComponentData
    {
        public float radius;
    }

    public class SphereColliderAuthoring : MonoBehaviour
    {
        public float radius = 0.5f;
    }

    public class SphereColliderBaker : Baker<SphereColliderAuthoring>
    {
        public override void Bake(SphereColliderAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SphereColliderComponent
            {
                radius = authoring.radius
            });
        }
    }
}
