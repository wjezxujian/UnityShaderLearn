using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class RayTracyingCtrl : MonoBehaviour
{

    // Use this for initialization
    private const int WIDTH = 800;
    private const int HEIGHT = 400;
    private const string IMG_PATH = "C:/Users/jian.xu/Desktop/RayTracing.png";

    void Start()
    {
        //CreatePng(WIDTH, HEIGHT, CreateColorForTestPNG(WIDTH, HEIGHT));
        //CreatePng(WIDTH, HEIGHT, CreateColorForTestRay(WIDTH, HEIGHT));
        //CreatePng(WIDTH, HEIGHT, CreateColorTestSphere(WIDTH, HEIGHT));
        //CreatePng(WIDTH, HEIGHT, createColorForTestNormal(WIDTH, HEIGHT));
        CreatePng(WIDTH, HEIGHT, CreateColorForTestHitRecord(WIDTH, HEIGHT));
    }

    // Update is called once per frame
    void Update()
    {

    }

    Color[] CreateColorForTestPNG(int width, int height)
    {
        int l = width * height;
        Color[] colors = new Color[l];
        for (int j = height - 1; j >= 0; j--)
        {
            for (int i = 0; i < width; i++)
            {
                colors[i + j * width] = new Color(i / (float)width, j / (float)height, 0.2f);
            }
        }

        return colors;
    }

    void CreatePng(int width, int height, Color[] colors)
    {
        if (width * height != colors.Length)
        {
            EditorUtility.DisplayDialog("ERROR", "长宽与数组长度无法对应!", "ok");
            return;
        }
        Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        tex.SetPixels(colors);
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        FileStream fs = new FileStream(IMG_PATH, FileMode.Create);
        BinaryWriter bw = new BinaryWriter(fs);
        bw.Write(bytes);
        fs.Close();
        bw.Close();
    }

    #region 第二版（测试射线，简单的摄像机和背景）
    Color GetColorForTestRay(Ray ray)
    {
        float t = 0.5f * ray.normalDirection.y + 1f;
        return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
    }

    Color[] CreateColorForTestRay(int width, int height)
    {
        // 视锥体的左下角，长宽和起始扫射点设定
        Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
        Vector3 horizontal = new Vector3(4, 0, 0);
        Vector3 vertical = new Vector3(0, 2, 0);
        Vector3 original = new Vector3(0, 0, 0);
        int l = width * height;
        Color[] colors = new Color[l];
        for (int j = height - 1; j >= 0; j--)
        {
            for (int i = 0; i < width; i++)
            {
                Ray r = new Ray(original, lowLeftCorner + horizontal * i / (float)width + vertical * j / (float)height);
                colors[i + j * width] = GetColorForTestRay(r);
            }
        }

        return colors;
    }
    #endregion

    #region 第三版（测试一个简单的球体）
    bool isHitSphereForTestSphere(Vector3 center, float radius, Ray ray)
    {
        var oc = ray.original - center;
        float a = Vector3.Dot(ray.direction, ray.direction);
        float b = 2f * Vector3.Dot(oc, ray.direction);
        float c = Vector3.Dot(oc, oc) - radius * radius;
        //实际上是判断这个方程有没有根，如果有两个根就击中
        float discriminant = b * b - 4 * a * c;
        return (discriminant >= 0);
    }

    Color GetColorForTestSphere(Ray ray)
    {
        if (isHitSphereForTestSphere(new Vector3(0, 0, -1), 0.5f, ray))
        {
            return new Color(1, 0, 0);
        }
        float t = 0.5f * ray.normalDirection.y + 1f;
        return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1.0f);
    }

    Color[] CreateColorTestSphere(int width, int height)
    {
        // 视锥体的左下角，长宽和起始扫射点设定
        Vector3 lowLeftCorner = new Vector3(-2, -1, -2);
        Vector3 horizontal = new Vector3(4, 0, 0);
        Vector3 vertical = new Vector3(0, 2, 0);
        Vector3 original = new Vector3(0, 0, 0);
        int large = width * height;
        Color[] colors = new Color[large];
        for (int j = height - 1; j >= 0; j--)
        {
            for (int i = 0; i < width; i++)
            {
                Ray r = new Ray(original, lowLeftCorner + horizontal * i / (float)width + vertical * j / (float)height);
                colors[i + j * width] = GetColorForTestSphere(r);
            }
        }

        return colors;
    }
    #endregion

    #region 第四版（测试球体的表面法线）
    float HitSphereForTestNormal(Vector3 center, float radius, Ray ray)
    {
        var oc = ray.original - center;
        float a = Vector3.Dot(ray.direction, ray.direction);
        float b = 2f * Vector3.Dot(oc, ray.direction);
        float c = Vector3.Dot(oc, oc) - radius * radius;
        //实际上是判断这个方程有没有根，如果有2个根就是击中
        float discriminat = b * b - 4 * a * c;
        if (discriminat < 0)
            return -1;
        else
        {
            //返回距离最近的那个根
            return (-b - Mathf.Sqrt(discriminat)) / (2f * a);
        }
    }

    Color GetColorForTestNormal(Ray ray)
    {
        float t = HitSphereForTestNormal(new Vector3(0, 0, -1), 0.5f, ray);
        if (t > 0)
        {
            Vector3 normal = Vector3.Normalize(ray.GetPoint(t) - new Vector3(0, 0, -1));
            return 0.5f * new Color(normal.x + 1, normal.y + 1, normal.z + 1, 2f);
        }
        t = 0.5f * ray.normalDirection.y + 1f;
        return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1f);
    }

    Color[] createColorForTestNormal(int width, int height)
    {
        //视锥体的左下角、长宽和起始扫射点设定
        Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
        Vector3 horizontal = new Vector3(4, 0, 0);
        Vector3 vertical = new Vector3(0, 2, 0);
        Vector3 original = new Vector3(0, 0, 0);
        int l = width * height;
        Color[] colors = new Color[l];
        for (int j = height - 1; j >= 0; j--)
            for (int i = 0; i < width; i++)
            {
                Ray r = new Ray(original, lowLeftCorner + horizontal * i / (float)width + vertical * j / (float)height);
                colors[i + j * width] = GetColorForTestNormal(r);
            }
        return colors;
    }
    #endregion

    #region 第五版（测试Hit的抽象）
    Color GetColorForTestHitRecord(Ray ray, HitableList hitableList)
    {
        HitRecord record = new HitRecord();
        if (hitableList.Hit(ray, 0f, float.MaxValue, ref record))
        {
            return 0.5f * new Color(record.normal.x + 1, record.normal.y + 1, record.normal.z + 1, 2f);
        }

        float t = 0.5f * ray.normalDirection.y + 1f;
        return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
    }

    Color[] CreateColorForTestHitRecord(int width, int height)
    {
        //视锥体的左下角，长宽和起始扫射点设定
        Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
        Vector3 horizontal = new Vector3(4, 0, 0);
        Vector3 vertical = new Vector3(0, 2, 0);
        Vector3 original = new Vector3(0, 0, 0);
        int l = width * height;
        HitableList hitableList = new HitableList();
        hitableList.list.Add(new Sphere(new Vector3(0, 0, -1), 0.5f));
        hitableList.list.Add(new Sphere(new Vector3(0, -100.5f, -1), 100));
        Color[] colors = new Color[l];
        for (int j = height - 1; j >= 0; j--)
        {
            for (int i = 0; i < width; i++)
            {
                Ray r = new Ray(original, lowLeftCorner + horizontal * i / (float)width + vertical * j / (float)height);
                colors[i + j * width] = GetColorForTestHitRecord(r, hitableList);
            }
        }
        return colors;
    }
    #endregion

    #region 第六版（测试抗锯齿）
    Color GetColorForTestAntialiasing(Ray ray, HitableList hitableList)
    {
        HitRecord record = new HitRecord();
        if(hitableList.Hit(ray, 0f, float.MaxValue, ref record))
        {
            return 0.5f * new Color(record.normal.x + 1, record.normal.y + 1, record.normal.z + 1, 2); 
        }
        float t = 0.5f * ray.normalDirection.y + 1f;
        return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1.0f);
    }

    Color[] CreateColorForTestAntialiasing(int width, int height)
    {
        //视锥体的左下角、长宽和起始扫射点设定
        Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
        Vector3 horizontal = new Vector3(4, 0, 0);
        Vector3 vertical = new Vector3(0, 2, 0);
        Vector3 original = new Vector3(0, 0, 0);
        int l = width * height;
        HitableList hitableList = new HitableList();
        hitableList.list.Add(new Sphere(new Vector3(0, 0, -1), 0.5f));
        hitableList.list.Add(new Sphere(new Vector3(0, -100.5f, -1), 100f));
        Color[] colors = new Color[l];
        Camera camera = new Camera(original, lowLeftCorner, horizontal, vertical);
        float recip_width = 1f / width;
        float recip_height = 1f / height;
        for (int j = height - 1; j >= 0; j--)
            for (int i = 0; i < width; i++)
            {
                Color color = new Color(0, 0, 0);
                for (int s = 0; s < SAMPLE; s++)
                {
                    Ray r = camera.CreateRay((i + Random.Range(0f, 1f)) * recip_width, (j + Random.Range(0f, 1f)) * recip_height);
                    color += GetColorForTestAntialiasing(r, hitableList);
                }
                color *= SAMPLE_WEIGHT;
                color.a = 1f;
                colors[i + j * width] = color;
            }
        return colors;
    }
    #endregion
}