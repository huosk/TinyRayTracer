using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera
{
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
