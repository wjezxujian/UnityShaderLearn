using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RayTracying
{
    public class RayTracyingCtrl : EditorWindow
    {

        // Use this for initialization
        private const int WIDTH = 800;
        private const int HEIGHT = 400;
        private const string IMG_PATH = "C:/Users/jian/Desktop/RayTracing.png";
        private const int SAMPLE = 100;
        private float SAMPLE_WEIGHT = 0.01f;
        private int MAX_SCATTER_TIME = 50;

        [MenuItem("RayTracing/光线追踪渲染器")]
        public static void OnClick()
        {
            RayTracyingCtrl window = GetWindow<RayTracyingCtrl>();
            window.Show();
        }

        public void OnGUI()
        {
            if (GUILayout.Button("测试图片"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestPNG(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试射线"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestRay(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试简单球体"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorTestSphere(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试球体法线"))
            {
                CreatePng(WIDTH, HEIGHT, createColorForTestNormal(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("抽象碰撞信息"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestHitRecord(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试反锯齿"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestAntialiasing(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试散射模型"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestDiffusing(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试镜面模型"))
            {
                CreatePng(WIDTH, HEIGHT, CrateColorForTestMetal(WIDTH, HEIGHT));
            }

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
            if (hitableList.Hit(ray, 0f, float.MaxValue, ref record))
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

        #region 第七版（测试Diffuse)
        //此处用于取得无序的反射方向，并用于模拟散射模型
        Vector3 GetRandomPointInUnitSphereForTestDiffusing()
        {
            Vector3 p = 2f * new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)) - Vector3.one;
            p = p.normalized * Random.Range(0f, 1f);
            //Vector3 p = Vector3.zero;
            //do
            //{
            //    p = 2f * new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)) - Vector3.one;
            //}
            //while (p.sqrMagnitude > 1f);
            ////效率有点低了
            return p;
        }

        Color GetColorForTestDiffusing(Ray ray, HitableList hitableList)
        {
            HitRecord record = new HitRecord();
            if(hitableList.Hit(ray, 0.0001f, float.MaxValue, ref record))
            {
                Vector3 target = record.p + record.normal + GetRandomPointInUnitSphereForTestDiffusing();
                //此处假定有50%的光被吸收，剩下的则从入射点开始取随机方向再次发射一条射线
                return 0.5f * GetColorForTestDiffusing(new Ray(record.p, target - record.p), hitableList);
            }
            float t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1f);
        }

        Color[] CreateColorForTestDiffusing(int width, int height)
        {
            //视锥体的左下角，长宽和起点扫射点设定
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
            float recio_width = 1f / width;
            float recip_height = 1f / height;
            for (int j = height - 1; j >= 0; j--)
            {
                for (int i = 0; i < width; i++)
                {
                    Color color = new Color(0, 0, 0);
                    for(int s = 0; s < SAMPLE; s++)
                    {
                        Ray r = camera.CreateRay((i + Random.Range(0f, 1f)) * recio_width, (j + Random.Range(0f, 1f)) * recip_height);
                        color += GetColorForTestDiffusing(r, hitableList);
                    }

                    color *= SAMPLE_WEIGHT;

                    //为了使球体看起来更亮， 改变gamma值
                    color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), 1f);
                    color.a = 1f;
                    colors[i + j * width] = color;
                }
            }

            return colors;
        }
        #endregion

        #region 第八版（测试镜面）
        Color GetColorForTestMetal(Ray ray, HitableList hitableList, int depth)
        {
            HitRecord record = new HitRecord();
            if(hitableList.Hit(ray, 0.0001f, float.MaxValue, ref record))
            {
                Ray r = new Ray(Vector3.zero, Vector3.zero);
                Color attenuation = Color.black;
                if(depth < MAX_SCATTER_TIME && record.material.scatter(ray, record, ref attenuation, ref r))
                {
                    Color c = GetColorForTestMetal(r, hitableList, depth + 1);
                    return new Color(c.r * attenuation.r, c.g * attenuation.g, c.b * attenuation.b);
                }
                else
                {
                    //假设已经反射了太多次，或者压根就没有发生反射，那么就认为黑了
                    return Color.black;
                }
            }
            float t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1f);
        }
        
        Color[] CrateColorForTestMetal(int width, int height)
        {
            //视锥体的左下角，长宽和起始扫射设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            HitableList hitableList = new HitableList();
            hitableList.list.Add(new Sphere(new Vector3(0, 0, -1), 0.5f, new Lambertian(new Color(0.8f, 0.3f, 0.3f))));
            hitableList.list.Add(new Sphere(new Vector3(0, -100.5f, -1), 100f, new Lambertian(new Color(0.8f, 0.8f, 0.0f))));
            hitableList.list.Add(new Sphere(new Vector3(1, 0, -1), 0.5f, new Metal(new Color(0.8f, 0.6f, 0.2f))));
            hitableList.list.Add(new Sphere(new Vector3(-1, 0, -1), 0.5f, new Metal(new Color(0.8f, 0.8f, 0.8f))));
            Color[] colors = new Color[l];
            Camera camera = new Camera(original, lowLeftCorner, horizontal, vertical);
            float recip_width = 1f / width;
            float recip_height = 1f / height;
            for(int j = height - 1; j > 0; j--)
            {
                for(int i = 0; i < width; i++)
                {
                    Color color = new Color(0, 0, 0);
                    for(int s = 0; s < SAMPLE; s++)
                    {
                        Ray r = camera.CreateRay((i + Random.Range(0f, 1f)) * recip_width, j + Random.Range(0f, 1f) * recip_height);
                        color += GetColorForTestMetal(r, hitableList, 0);
                    }
                    color *= SAMPLE_WEIGHT;
                    //为了使球体看起来更亮，改变gamma值
                    color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), 1f);
                    color.a = 1f;
                    colors[i + j * width] = color;
                }
            }
            return colors;
        }
        #endregion


    }

}