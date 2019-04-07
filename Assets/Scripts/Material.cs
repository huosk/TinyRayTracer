using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Material
{
    public Color diffuse;
    public Color specular;
    public float gloss;

    public Material(Color d)
    {
        this.diffuse = d;
    }
}

public class RenderObject
{
    public Sphere sphere;
    public Material material;
}