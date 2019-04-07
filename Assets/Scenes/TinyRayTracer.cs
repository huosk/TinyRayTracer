using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRT
{
    public class TinyRayTracer : MonoBehaviour
    {
        // Start is called before the first frame update
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

            RenderObject[] objs = new RenderObject[]
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
            };

            Light[] lights = new Light[] {
            new Light(new Vector3(-20,20,20),1.5f),
            //new Light(new Vector3(30.0f,50.0f,-25.0f),1.8f),
            //new Light(new Vector3(30.0f,20.0f,30.0f),1.7f)
        };

            Render(objs, lights);
        }

        private bool SceneIntersect(Vector3 origin,
                                    Vector3 dir,
                                    RenderObject[] spheres,
                                    ref Vector3 hit,
                                    ref Vector3 normal,
                                    ref Material hitMaterial)
        {
            float sphere_dist = float.MaxValue;
            for (int i = 0; i < spheres.Length; i++)
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

            float plne_dist = float.MaxValue;
            if (Mathf.Abs(dir.y) > 0.001f)
            {// 非平行方向
                float d = -(origin.y + 4f) / dir.y;
                Vector3 hitPoint = origin + dir * d;
                if (d > 0 &&
                   Mathf.Abs(hitPoint.x) < 10 &&
                   hitPoint.z < -10 &&
                   hitPoint.z > -30 &&
                   d < sphere_dist)
                {
                    plne_dist = d;
                    hit = hitPoint;
                    normal = Vector3.up;
                    Color c = (((int)(0.5f * hit.x + 1000) + (int)(0.5f * hit.z)) & 1) == 1 ?
                        new Color(0.3f, 0.3f, 0.3f) :
                        new Color(0.3f, 0.2f, 0.1f);
                    hitMaterial = new Material(c)
                    {
                        diffuseAmount = 1.0f
                    };
                }
            }

            return Mathf.Min(sphere_dist, plne_dist) < 1000;
        }

        private Vector3 Reflect(Vector3 I, Vector3 N)
        {
            return N * 2.0f * Vector3.Dot(I, N) - I;
        }

        // factor = n1 / n2
        private Vector3 Refract(Vector3 Q, Vector3 N, float eta_t, float eta_i = 1.0f)
        {
            float cos_theta1 = Vector3.Dot(-Q, N);
            if (cos_theta1 < 0)
                return Refract(Q, -N, eta_i, eta_t);

            float eta = eta_i / eta_t;
            float k = 1 - eta * eta * (1 - cos_theta1 * cos_theta1);
            return k < 0 ? new Vector3(1, 0, 0) : (Q * eta + N * (eta * cos_theta1 - Mathf.Sqrt(k)));
        }

        private Color CastRay(Vector3 origin, Vector3 dir, RenderObject[] objs, Light[] lights, int depth)
        {
            Material material = null;
            Vector3 normal = Vector3.zero;
            Vector3 hitPoint = Vector3.zero;

            if (depth > 4 || !SceneIntersect(origin, dir, objs, ref hitPoint, ref normal, ref material))
                return new Color(0.2f, 0.7f, 0.8f);
            else
            {
                Vector3 reflectDir = -Reflect(dir, normal).normalized;
                Vector3 refl_Origin = Vector3.Dot(reflectDir, normal) < 0 ? hitPoint - normal * 0.001f : hitPoint + normal * 0.001f;
                Color refl_Color = CastRay(refl_Origin, reflectDir, objs, lights, depth + 1);

                Vector3 refractDir = Refract(dir, normal, material.refractFactor).normalized;
                Vector3 refr_Origin = Vector3.Dot(refractDir, normal) < 0f ? hitPoint - normal * 0.001f : hitPoint + normal * 0.001f;
                Color refr_Color = CastRay(refr_Origin, refractDir, objs, lights, depth + 1);

                float diffuse_ligth_intensity = 0f;
                float specular_light_intensity = 0f;
                for (int i = 0; i < lights.Length; i++)
                {
                    // 光照方向，从光线与表面交点到光源位置
                    Vector3 lightDir = (lights[i].position - hitPoint).normalized;

                    Vector3 shadow_origin = Vector3.Dot(lightDir, normal) < 0 ? hitPoint - normal * 0.001f : hitPoint + normal * 0.001f;
                    Vector3 shadow_normal = Vector3.zero;
                    Vector3 shadow_point = Vector3.zero;
                    Material shadow_mt = null;
                    if (SceneIntersect(shadow_origin, lightDir, objs, ref shadow_point, ref shadow_normal, ref shadow_mt) &&
                       Vector3.Distance(shadow_origin, shadow_point) < Vector3.Distance(lights[i].position, hitPoint))
                    {
                        continue;
                    }

                    // 计算光照
                    diffuse_ligth_intensity += lights[i].intensity * Mathf.Max(0f, Vector3.Dot(lightDir, normal));
                    specular_light_intensity += Mathf.Pow(Mathf.Max(0, Vector3.Dot(dir, Reflect(-lightDir, normal))), material.gloss) * lights[i].intensity;
                }

                return material.diffuse * diffuse_ligth_intensity * material.diffuseAmount +
                               material.specular * specular_light_intensity * material.specularAmount +
                               refl_Color * material.reflectAmount +
                               refr_Color * material.refractAmount;
            }
        }

        void Render(RenderObject[] objs, Light[] lights)
        {
            int width = 1024;
            int height = 768;

            Color[] framebuffer = new Color[width * height];

            Sphere sphere = new Sphere(new Vector3(-3, 0, -16), 2);

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    Vector3 worldPos = ScreenToWorld(i, j, width, height);
                    Vector3 cameraPos = new Vector3(0, 0, 0);
                    Vector3 dir = (worldPos - cameraPos).normalized;
                    framebuffer[j * width + i] = CastRay(cameraPos, dir, objs, lights, 0);
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

            // 范围[0,nearClipSize.x]
            float x = ix * nearClipSize.x / width;
            float y = jy * nearClipSize.y / height;

            // 范围：[-nearClipSize.x / 2,nearClipSize.x / 2]
            x -= nearClipSize.x * 0.5f;
            y -= nearClipSize.y * 0.5f;

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

    public class Light
    {
        public Vector3 position;
        public float intensity;

        public Light(Vector3 pos, float i)
        {
            this.position = pos;
            this.intensity = i;
        }
    }
}