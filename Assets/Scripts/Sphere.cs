using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sphere
{
    public Vector3 center;
    public float radius;


    public Sphere(Vector3 c, float r)
    {
        this.center = c;
        this.radius = r;
    }

    public bool RayIntersect(Vector3 origin, Vector3 dir, ref float t)
    {
        // 射线起点到球心的向量
        Vector3 L = center - origin;

        // 投影长
        float tca = Vector3.Dot(L, dir);

        // 垂直距离
        float d2 = Vector3.Dot(L, L) - tca * tca;

        if (d2 > radius * radius) return false;

        // 垂足到与园交点的长度
        float thc = Mathf.Sqrt(radius * radius - d2);

        if (thc > tca)
        {// 起点在球内部
            t = thc + tca;
            return false;
        }
        else
        {// 起点在球外部
            t = tca - thc;
            return true;
        }
    }
}
