using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Step05 : MonoBehaviour
{
    int width = 1024;
    int height = 768;

    World world;

    // Use this for initialization
    void Start()
    {
        Material ivory = new Material(new Color(0.4f, 0.4f, 0.3f));
        Material red_rubber = new Material(new Color(0.3f, 0.1f, 0.1f));

        world = new World(new RenderObject[]
        {
            new RenderObject(){
                sphere = new Sphere(new Vector3(-3.0f,0.0f,-16.0f), 2.0f),
                material = ivory
            },
            new RenderObject(){
                sphere = new Sphere(new Vector3(-1.0f,-1.5f,-12.0f),2.0f),
                material = red_rubber,
            },
            new RenderObject(){
                sphere = new Sphere(new Vector3(1.5f,-0.5f,-18.0f),3.0f),
                material = red_rubber,
            },
            new RenderObject(){
                sphere = new Sphere(new Vector3(7.0f,5.0f,-18.0f),4.0f),
                material = ivory
                }
        });

        Render();
    }

    private Color CastRay(Vector3 origin, Vector3 dir)
    {
        Material material = null;
        Vector3 normal = Vector3.zero;
        Vector3 hitPoint = Vector3.zero;

        if (!world.SceneIntersect(origin, dir, ref hitPoint, ref normal, ref material))
            return new Color(0.2f, 0.7f, 0.8f);
        else
            return material.diffuse;
    }

    Color GetPixel(int i, int j)
    {
        Camera cam = new Camera();

        Vector3 worldPos = cam.ScreenToWorld(i, j, width, height);
        Vector3 cameraPos = new Vector3(0, 0, 0);
        Vector3 dir = (worldPos - cameraPos).normalized;

        return CastRay(cameraPos, dir);
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
