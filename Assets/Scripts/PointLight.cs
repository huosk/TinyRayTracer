using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointLight
{
    public Vector3 position;
    public float intensity;

    public PointLight(Vector3 pos, float i)
    {
        this.position = pos;
        this.intensity = i;
    }
}
