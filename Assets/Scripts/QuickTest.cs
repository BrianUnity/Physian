using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[ExecuteInEditMode]
public class QuickTest : MonoBehaviour
{
    public Transform sphere;
    public Transform plane;

    public float sphereRadius = 0.5f;

    void Update()
    {
        float3 spherePoint = sphere.position;
        float3 sphereDirection = sphere.forward;

        float3 planePoint = plane.position;
        float3 planeNormal = plane.up;

        // https://en.wikipedia.org/wiki/Line-plane_intersection
        // p = l_0 + l * d.
        // d = ((p_0 - l_0) . n) \ l . n
        float lineDotPlane = math.dot(sphereDirection, planeNormal);
        if (lineDotPlane == 0)
            return; // parrallel, does not intersect
        float d = math.dot(planePoint - spherePoint, planeNormal) / lineDotPlane; 
        float3 collisionPoint = spherePoint + (d * sphereDirection);

        Debug.DrawRay(spherePoint, sphereDirection, Color.blue);
        Debug.DrawLine(spherePoint, collisionPoint, Color.magenta);

        float distanceAlongNormal = math.dot(collisionPoint - spherePoint, -planeNormal);

        //  x' = x - 2 D n
        float3 midPoint = collisionPoint + distanceAlongNormal * planeNormal;
        float3 reflectedDirection = 2 * midPoint - spherePoint - collisionPoint;
        
        Debug.DrawRay(collisionPoint, reflectedDirection, Color.cyan);
        Debug.DrawRay(collisionPoint, distanceAlongNormal * planeNormal, Color.red);
    }

    float DistanceToPlane(float3 planeNormal, float3 point)
    {
        // Distance = d / (sqrt(a ^ 2 + b ^ 2 + c ^ 2))
        float d = -point.x * planeNormal.x - point.y * planeNormal.y - point.z * planeNormal.z;
        float distance = d / math.sqrt(planeNormal.x * planeNormal.x + planeNormal.y * planeNormal.y + planeNormal.z * planeNormal.z);

        return distance;
    }
}
