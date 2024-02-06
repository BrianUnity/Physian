using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct SpawnerComponent : IComponentData
{
    public Entity prefab;
    public int count;
}

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject prefab;
    public int count = 100;
}

public class SpawnerBaker : Baker<SpawnerAuthoring>
{
    public override void Bake(SpawnerAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new SpawnerComponent
        {
            prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
            count = authoring.count
        });
    }
}
