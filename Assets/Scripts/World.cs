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

        return sphere_dist < 1000;
    }
}
