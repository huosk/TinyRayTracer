using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World
{
    private List<RenderObject> spheres;

    public World(RenderObject[] objs)
    {
        this.spheres = new List<RenderObject>(objs);
    }

    public bool SceneIntersect(Vector3 origin,
                    Vector3 dir,
                    ref Vector3 hit,
                    ref Vector3 normal,
                    ref Material hitMaterial)
    {
        float sphere_dist = float.MaxValue;
        for (int i = 0; i < spheres.Count; i++)
        {
            float dist_i = 0;
            if (spheres[i].sphere.RayIntersect(origin, dir, ref dist_i) && dist_i < sphere_dist)
            {
                sphere_dist = dist_i;
                hit = origin + dir * dist_i;
                normal = (hit - spheres[i].sphere.center).normalized;
                hitMaterial = spheres[i].material;
            }
        }

        // Step11

        float plne_dist = float.MaxValue;
        if (Mathf.Abs(dir.y) > 0.001f)
        {// 非平行方向
            float d = -(origin.y + 4f) / dir.y;
            Vector3 hitPoint = origin + dir * d;
            if (d > 0 &&
               Mathf.Abs(hitPoint.x) < 10 &&
               hitPoint.z < -10 &&
               hitPoint.z > -30 &&
               d < sphere_dist)
            {
                plne_dist = d;
                hit = hitPoint;
                normal = Vector3.up;
                Color c = (((int)(0.5f * hit.x + 1000) + (int)(0.5f * hit.z)) & 1) == 1 ?
                    new Color(0.3f, 0.3f, 0.3f) :
                    new Color(0.3f, 0.2f, 0.1f);
                hitMaterial = new Material(c)
                {
                    diffuseAmount = 1.0f
                };
            }
        }

        return Mathf.Min(sphere_dist, plne_dist) < 1000;
        // Step11 end

        // Step05
        //return sphere_dist < 1000;
        // Step05 end
    }
}
