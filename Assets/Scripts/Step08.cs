using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Step08 : MonoBehaviour 
{
    int width = 1024;
    int height = 768;

    World world;
    PointLight[] lights;

    // Use this for initialization
    void Start()
    {
        Material ivory = new Material(new Color(0.4f, 0.4f, 0.3f))
        {
            specular = new Color(1.0f, 1.0f, 1.0f),
            gloss = 150f
        };

        Material red_rubber = new Material(new Color(0.3f, 0.1f, 0.1f))
        {
            specular = new Color(1.0f, 1.0f, 1.0f),
            gloss = 10f
        };

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

        lights = new PointLight[] {
            new PointLight(new Vector3(-20,20,20),1.5f)
        };

        Render();
    }

    private Vector3 Reflect(Vector3 I, Vector3 N)
    {
        return N * 2.0f * Vector3.Dot(I, N) - I;
    }

    private Color CastRay(Vector3 origin, Vector3 dir)
    {
        Material material = null;
        Vector3 normal = Vector3.zero;
        Vector3 hitPoint = Vector3.zero;

        if (!world.SceneIntersect(origin, dir, ref hitPoint, ref normal, ref material))
            return new Color(0.2f, 0.7f, 0.8f);
        else
        {
            float diffuse_light_intensity = 0.0f;
            float specular_light_intensity = 0.0f;
            for (int i = 0; i < lights.Length; i++)
            {
                Vector3 lightDir = (lights[i].position - hitPoint).normalized;

                Vector3 sdRayOrigin = Vector3.Dot(normal, lightDir) < 0 ? hitPoint - normal * 0.001f : hitPoint + normal * 0.001f;
                Vector3 sdHitNormal = Vector3.zero;
                Vector3 sdHitPoint = Vector3.zero;
                Material sdHitMaterial = null;
                if(world.SceneIntersect(sdRayOrigin,lightDir,ref sdHitPoint,ref sdHitNormal,ref sdHitMaterial) &&
                   Vector3.Distance(sdRayOrigin,sdHitPoint) < Vector3.Distance(hitPoint,lights[i].position))
                {// 检测在光源和物体之间，是否存在其他遮挡物
                    continue;
                }

                diffuse_light_intensity += lights[i].intensity * Mathf.Max(0.0f, Vector3.Dot(lightDir, normal));
                specular_light_intensity += Mathf.Pow(Mathf.Max(0, Vector3.Dot(dir, Reflect(-lightDir, normal))), material.gloss) * lights[i].intensity;
            }

            return material.diffuse * diffuse_light_intensity +
                           material.specular * specular_light_intensity;
        }
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
