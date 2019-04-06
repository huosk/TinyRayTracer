using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Step04 : MonoBehaviour
{
    int width = 1024;
    int height = 768;

    // Use this for initialization
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

    Color GetPixel(int i, int j)
    {
        Camera cam = new Camera();

        Vector3 worldPos = cam.ScreenToWorld(i, j, width, height);
        Vector3 cameraPos = new Vector3(0, 0, 0);
        Vector3 dir = (worldPos - cameraPos).normalized;

        Sphere sphere = new Sphere(new Vector3(-3, 0, -16), 2);

        return CastRay(cameraPos, dir, sphere);
    }

    #region Step01 输出图片

    void Render()
    {
        Color[] framebuffer = new Color[width * height];

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                framebuffer[j * width + i] = GetPixel(i, j);
            }
        }

        SaveBuffer(framebuffer);
    }

    private void SaveBuffer(Color[] framebuffer)
    {
        Texture2D image = new Texture2D(width, height);
        image.SetPixels(framebuffer);
        image.wrapMode = TextureWrapMode.Clamp;
        image.Apply();

        System.IO.File.WriteAllBytes(Application.dataPath + "/" + GetType().Name + ".jpg", image.EncodeToJPG());
    }

    #endregion

}
