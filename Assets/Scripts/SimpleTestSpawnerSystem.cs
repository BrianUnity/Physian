using Physian;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public partial class SimpleTestSpawnerSystem : SystemBase
{

    protected override void OnUpdate()
    {
        EntityQuery colliderQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<PositionComponent, VelocityComponent, SphereColliderComponent, MassComponent>()
            .Build(this);

        if (!SystemAPI.TryGetSingleton(out SpawnerComponent spawnerComponent))
            return;

        int currentCount = colliderQuery.CalculateEntityCount();
        if (currentCount >= spawnerComponent.count)
            return;

        Unity.Mathematics.Random rand = new Unity.Mathematics.Random();
        rand.InitState();

        int neededSpawns = spawnerComponent.count - currentCount;
        for (int i = 0; i < neededSpawns; i++)
        {
            Entity newEntity = EntityManager.Instantiate(spawnerComponent.prefab);
            EntityManager.SetComponentData(newEntity, new PositionComponent()
            {
                x = rand.NextFloat(-10f, 10f),
                y = rand.NextFloat(-10f, 10f),
                z = rand.NextFloat(-10f, 10f)
            });
            EntityManager.SetComponentData(newEntity, new VelocityComponent()
            {
                x = rand.NextFloat(-4f, 4f),
                y = rand.NextFloat(-4f, 4f),
                z = rand.NextFloat(-4f, 4f)
            });
        }
    }
}
