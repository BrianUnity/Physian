using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Physian
{
    public struct PositionComponent : IComponentData
    {
        public float3 v
        {
            get { return new float3(x, y, z); }
            set { x = v.x; y = v.y; z = v.z; }
        }

        public float x;
        public float y;
        public float z;
    }

    public class PositionAuthoring : MonoBehaviour
    {
    }

    public class PositionBaker : Baker<PositionAuthoring>
    {
        public override void Bake(PositionAuthoring authoring)
        {
            Debug.Log("Spawning Position");

            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PositionComponent
            {
                x = authoring.transform.position.x,
                y = authoring.transform.position.y,
                z = authoring.transform.position.z
            });
        }
    }
}
