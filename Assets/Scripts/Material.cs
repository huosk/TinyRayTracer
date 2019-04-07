using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Material
{
    public Color diffuse;
    public Color specular;
    public float gloss;
    public float diffuseAmount;
    public float specularAmount;
    public float reflectAmount;
    public float refractAmount;
    public float refractFactor;

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