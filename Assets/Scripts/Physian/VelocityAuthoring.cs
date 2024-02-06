using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Physian
{
    public struct VelocityComponent : IComponentData
    {
        public VelocityComponent(float3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public float3 v
        {
            get { return new float3(x, y, z); }
        }

        public float x;
        public float y;
        public float z;
    }

    public class VelocityAuthoring : MonoBehaviour
    {
        public float x;
        public float y;
        public float z;
    }

    public class VelocityBaker : Baker<VelocityAuthoring>
    {
        public override void Bake(VelocityAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new VelocityComponent
            {
                x = authoring.x,
                y = authoring.y,
                z = authoring.z
            });
        }
    }
}
