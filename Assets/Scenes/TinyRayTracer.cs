using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TinyRayTracer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Render();
    }

    private Color CastRay(Vector3 origin, Vector3 dir, Sphere sphere)
    {
        float sphere_dist = float.MaxValue;
        if (!sphere.RayIntersect(origin, dir, ref sphere_dist))
            return new Color(0.2f, 0.7f, 0.8f);
        else
            return new Color(0.4f, 0.4f, 0.3f);
    }

    void Render()
    {
        int width = 1024;
        int height = 768;

        Color[] framebuffer = new Color[width * height];

        Sphere sphere = new Sphere(new Vector3(-3, 0, -16), 2);

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                //Vector3 worldPos = ScreenToWorld(i, j, width, height);
                //Vector3 cameraPos = new Vector3(0, 0, 0);
                //Vector3 dir = (worldPos - cameraPos).normalized;
                //framebuffer[j * width + i] = CastRay(cameraPos, dir, sphere);
                float x = (2 * (i + 0.5f) / (float)width - 1) * Mathf.Tan(60.0f * Mathf.Deg2Rad) * width / (float)height;
                float y = -(2 * (j + 0.5f) / (float)height - 1) * Mathf.Tan(60.0f * Mathf.Deg2Rad);
                Vector3 dir = new Vector3(x, y, -1).normalized;
                framebuffer[j * width + i] = CastRay(Vector3.zero, dir, sphere);
            }
        }

        SaveBuffer(framebuffer, width, height);
    }

    /// <summary>
    /// 根据像素的索引，计算世界坐标
    /// </summary>
    /// <param name="i">像素的水平索引</param>
    /// <param name="j">像素的垂直索引</param>
    /// <returns></returns>
    public Vector3 ScreenToWorld(int i, int j, int width, int height)
    {
        float ix = i + 0.5f;    // 像素的水平中心
        float jy = j + 0.5f;    // 像素的垂直中心

        float aspect = (float)width / height;
        Vector2 nearClipSize = GetNearClipSize(1, 60.0f * Mathf.Deg2Rad, aspect);
        float x = ix * nearClipSize.x / width - width * 0.5f;
        float y = jy * nearClipSize.y / height - height * 0.5f;
        return new Vector3(x, y, -1);
    }

    private void SaveBuffer(Color[] framebuffer, int width, int height)
    {
        Texture2D image = new Texture2D(width, height);
        image.SetPixels(framebuffer);
        image.wrapMode = TextureWrapMode.Clamp;
        image.Apply();

        System.IO.File.WriteAllBytes(Application.dataPath + "/r.jpg", image.EncodeToJPG());
    }

    /// <summary>
    /// 获取近裁剪面的尺寸
    /// </summary>
    /// <param name="near">近裁剪面距离摄像机的距离</param>
    /// <param name="fov">视场角</param>
    /// <param name="aspect">宽高比</param>
    /// <returns></returns>
    private Vector2 GetNearClipSize(float near, float fov, float aspect)
    {
        float height = Mathf.Tan(fov * 0.5f) * 2.0f * near;
        float width = height * aspect;
        return new Vector2(width, height);
    }
}

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