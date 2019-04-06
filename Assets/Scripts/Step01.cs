using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Step01 : MonoBehaviour
{
    int width = 1024;
    int height = 768;

    // Use this for initialization
    void Start()
    {
        Render();
    }

    Color GetPixel(int i, int j)
    {
        return new Color((float)i / width, (float)j / height, 0f);
    }

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
}
