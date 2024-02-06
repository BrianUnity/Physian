using Unity.Entities;
using UnityEngine;

namespace Physian
{
    public struct MassComponent : IComponentData
    {
        public float mass;
    }

    public class MassAuthoring : MonoBehaviour
    {
        public float mass = 1f;
    }

    public class MassBaker : Baker<MassAuthoring>
    {
        public override void Bake(MassAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MassComponent
            {
                mass = authoring.mass
            });
        }
    }
}
