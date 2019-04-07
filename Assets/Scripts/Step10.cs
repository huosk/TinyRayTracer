using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Step10 : MonoBehaviour
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
            refractFactor = 1.0f,
            diffuseAmount = 0.6f,
            specularAmount = 0.3f,
            reflectAmount = 0.1f,
            refractAmount = 0.0f,
            gloss = 50.0f
        };
        Material glass = new Material(new Color(0.6f, 0.7f, 0.8f))
        {
            specular = new Color(1.0f, 1.0f, 1.0f),
            gloss = 125.0f,
            diffuseAmount = 0.0f,
            specularAmount = 0.5f,
            reflectAmount = 0.1f,
            refractAmount = 0.8f,
            refractFactor = 1.5f
        };
        Material red_rubber = new Material(new Color(0.3f, 0.1f, 0.1f))
        {
            specular = new Color(1.0f, 1.0f, 1.0f),
            gloss = 10.0f,
            diffuseAmount = 0.9f,
            specularAmount = 0.1f,
            reflectAmount = 0.0f,
            refractAmount = 0.0f,
            refractFactor = 1.0f
        };
        Material red_mirror = new Material(new Color(1.0f, 1.0f, 1.0f))
        {
            specular = new Color(1.0f, 1.0f, 1.0f),
            gloss = 1420f,
            diffuseAmount = 0.0f,
            specularAmount = 10.0f,
            reflectAmount = 0.8f,
            refractAmount = 0.0f,
            refractFactor = 1.0f
        };

        world = new World(new RenderObject[]
        {
            new RenderObject(){
                sphere = new Sphere(new Vector3(-3.0f,0.0f,-16.0f), 2.0f),
                material = ivory
            },
            new RenderObject(){
                sphere = new Sphere(new Vector3(-1.0f,-1.5f,-12.0f),2.0f),
                material = glass,
            },
            new RenderObject(){
                sphere = new Sphere(new Vector3(1.5f,-0.5f,-18.0f),3.0f),
                material = red_rubber,
            },
            new RenderObject(){
                sphere = new Sphere(new Vector3(7.0f,5.0f,-18.0f),4.0f),
                material = red_mirror
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

    private Vector3 Refract(Vector3 Q, Vector3 N, float eta_t, float eta_i = 1.0f)
    {
        float cos_theta1 = Vector3.Dot(-Q, N);
        if (cos_theta1 < 0)
            return Refract(Q, -N, eta_i, eta_t);

        float eta = eta_i / eta_t;
        float k = 1 - eta * eta * (1 - cos_theta1 * cos_theta1);
        return k < 0 ? new Vector3(0, 0, 0) : (Q * eta - N * (eta * cos_theta1 + Mathf.Sqrt(k)));
    }

    private Color CastRay(Vector3 origin, Vector3 dir, int depth)
    {
        Material material = null;
        Vector3 normal = Vector3.zero;
        Vector3 hitPoint = Vector3.zero;

        if (depth > 4 || !world.SceneIntersect(origin, dir, ref hitPoint, ref normal, ref material))
            return new Color(0.2f, 0.7f, 0.8f);
        else
        {
            // 反射部分
            Vector3 reflDir = -Reflect(dir, normal).normalized;
            Vector3 refl_Origin = Vector3.Dot(reflDir, normal) < 0 ? hitPoint - normal * 0.001f : hitPoint + normal * 0.001f;
            Color refl_Color = CastRay(refl_Origin, reflDir, depth);

            // 折射部分
            Vector3 refractDir = Refract(dir, normal, material.refractFactor).normalized;
            Color refr_Color = new Color(0f, 0f, 0f);
            if (refractDir != Vector3.zero)
            {
                Vector3 refr_Origin = Vector3.Dot(refractDir, normal) < 0f ? hitPoint - normal * 0.001f : hitPoint + normal * 0.001f;
                refr_Color = CastRay(refr_Origin, refractDir, depth + 1);
            }

            float diffuse_light_intensity = 0.0f;
            float specular_light_intensity = 0.0f;
            for (int i = 0; i < lights.Length; i++)
            {
                Vector3 lightDir = (lights[i].position - hitPoint).normalized;

                Vector3 sdRayOrigin = Vector3.Dot(normal, lightDir) < 0 ? hitPoint - normal * 0.001f : hitPoint + normal * 0.001f;
                Vector3 sdHitNormal = Vector3.zero;
                Vector3 sdHitPoint = Vector3.zero;
                Material sdHitMaterial = null;
                if (world.SceneIntersect(sdRayOrigin, lightDir, ref sdHitPoint, ref sdHitNormal, ref sdHitMaterial) &&
                   Vector3.Distance(sdRayOrigin, sdHitPoint) < Vector3.Distance(hitPoint, lights[i].position))
                {// 检测在光源和物体之间，是否存在其他遮挡物
                    continue;
                }

                diffuse_light_intensity += lights[i].intensity * Mathf.Max(0.0f, Vector3.Dot(lightDir, normal));
                specular_light_intensity += Mathf.Pow(Mathf.Max(0, Vector3.Dot(dir, Reflect(-lightDir, normal))), material.gloss) * lights[i].intensity;
            }

            return material.diffuse * diffuse_light_intensity * material.diffuseAmount +
                           material.specular * specular_light_intensity * material.specularAmount +
                           refl_Color * material.reflectAmount + 
                           refr_Color * material.refractAmount;
        }
    }

    Color GetPixel(int i, int j)
    {
        Camera cam = new Camera();

        Vector3 worldPos = cam.ScreenToWorld(i, j, width, height);
        Vector3 cameraPos = new Vector3(0, 0, 0);
        Vector3 dir = (worldPos - cameraPos).normalized;

        return CastRay(cameraPos, dir, 1);
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
