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
        private string IMG_PATH = "";
        private string IMG_PATH2 = "C:/Users/jian/Desktop/";
        private const int SAMPLE = 4;
        private float SAMPLE_WEIGHT = 0.01f;
        private int MAX_SCATTER_TIME = 50;

        private long m_rayTimes = 0;

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
                IMG_PATH = IMG_PATH2 + "RayTracing1.png";
                CreatePng(WIDTH, HEIGHT, CreateColorForTestPNG(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试射线"))
            {
                IMG_PATH = IMG_PATH2 + "RayTracing2.png";
                CreatePng(WIDTH, HEIGHT, CreateColorForTestRay(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试简单球体"))
            {
                IMG_PATH = IMG_PATH2 + "RayTracing3.png";
                CreatePng(WIDTH, HEIGHT, CreateColorTestSphere(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试球体法线"))
            {
                IMG_PATH = IMG_PATH2 + "RayTracing4.png";
                CreatePng(WIDTH, HEIGHT, createColorForTestNormal(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("抽象碰撞信息"))
            {
                IMG_PATH = IMG_PATH2 + "RayTracing5.png";
                CreatePng(WIDTH, HEIGHT, CreateColorForTestHitRecord(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试反锯齿"))
            {
                IMG_PATH = IMG_PATH2 + "RayTracing6.png";
                CreatePng(WIDTH, HEIGHT, CreateColorForTestAntialiasing(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试散射模型"))
            {
                IMG_PATH = IMG_PATH2 + "RayTracing7.png";
                CreatePng(WIDTH, HEIGHT, CreateColorForTestDiffusing(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试镜面模型"))
            {
                IMG_PATH = IMG_PATH2 + "RayTracing8.png";
                CreatePng(WIDTH, HEIGHT, CreateColorForTestMetal(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试透明模型"))
            {
                IMG_PATH = IMG_PATH2 + "RayTracing9.png";
                CreatePng(WIDTH, HEIGHT, CreateColorForTestDielectric(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试景深"))
            {
                IMG_PATH = IMG_PATH2 + "RayTracing11.png";
                CreatePng(WIDTH, HEIGHT, CreateColorForTestDefocus(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试场景"))
            {
                IMG_PATH = IMG_PATH2 + "RayTracing12.png";
                CreatePng(WIDTH, HEIGHT, CreateColorForTestScene(WIDTH, HEIGHT));
            }

        }

        #region 第一版（测试图片）
        /// <summary>
        /// 创建PNG图片颜色
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>Color[]</returns>
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
        #endregion

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
            ++m_rayTimes;
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
        
        Color[] CreateColorForTestMetal(int width, int height)
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
            hitableList.list.Add(new Sphere(new Vector3(1, 0, -1), 0.5f, new Metal(new Color(0.8f, 0.6f, 0.2f), 1.0f)));
            hitableList.list.Add(new Sphere(new Vector3(-1, 0, -1), 0.5f, new Metal(new Color(0.8f, 0.8f, 0.8f), 0.3f)));
            Color[] colors = new Color[l];
            Camera camera = new Camera(original, lowLeftCorner, horizontal, vertical);
            float recip_width = 1f / width;
            float recip_height = 1f / height;
            for(int j = height - 1; j >= 0; j--)
            {
                for(int i = 0; i < width; i++)
                {
                    Color color = new Color(0, 0, 0);
                    for(int s = 0; s < SAMPLE; s++)
                    {
                        Ray r = camera.CreateRay((i + Random.Range(0f, 1f)) * recip_width, (j + Random.Range(0f, 1f)) * recip_height);
                        color += GetColorForTestMetal(r, hitableList, 0);
                    }
                    color *= SAMPLE_WEIGHT;
                    //为了使球体看起来更亮，改变gamma值
                    color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), 1f);
                    color.a = 1f;
                    colors[i + j * width] = color;
                }
            }

            Debug.Log("RayTracing time: %l" + m_rayTimes.ToString());

            return colors;
        }
        #endregion

        #region 第九版（测试透明）
        Color GetColorForTestDielectric(Ray ray, Hitable hitableList, int depth)
        {
            HitRecord record = new HitRecord();
            if(hitableList.Hit(ray, 0.0001f, float.MaxValue, ref record))
            {
                Ray r = new Ray(Vector3.zero, Vector3.zero);
                Color attenuation = Color.black;
                if(depth < MAX_SCATTER_TIME && record.material.scatter(ray, record, ref attenuation, ref r))
                {
                    Color c = GetColorForTestDielectric(r, hitableList, depth + 1);
                    return new Color(c.r * attenuation.r, c.g * attenuation.g, c.b * attenuation.b);
                }
                else
                {
                    //假设已经反射了太多次，或者压根没有发生反射，那么就默认黑了
                    return Color.black;
                }
            }
            float t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1.0f);
        }

        Color[] CreateColorForTestDielectric(int width, int height)
        {
            //视锥体的左下角，长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            HitableList hitalbeList = new HitableList();
            hitalbeList.list.Add(new Sphere(new Vector3(0, 0, -1), 0.5f, new Lambertian(new Color(0.1f, 0.2f, 0.5f))));
            hitalbeList.list.Add(new Sphere(new Vector3(0, -100.5f, -1), 100f, new Lambertian(new Color(0.8f, 0.8f, 0.0f))));
            hitalbeList.list.Add(new Sphere(new Vector3(1, 0, -1), 0.5f, new Metal(new Color(0.8f, 0.6f, 0.2f), 0.0f)));
            hitalbeList.list.Add(new Sphere(new Vector3(-1, 0, -1), 0.5f, new Dielectric(1.5f)));
            hitalbeList.list.Add(new Sphere(new Vector3(-1, 0, -1), -0.45f, new Dielectric(1.5f)));
            Color[] colors = new Color[l];
            Camera camera = new Camera(original, lowLeftCorner, horizontal, vertical);
            //Camera camera = new Camera(45, 2);
            //Camera camera = new Camera(new Vector3(-2, 2, -1), new Vector3(0, 0, -1), new Vector3(0, 1, 0), 90, 2);
            float recip_width = 1f / width;
            float recip_height = 1f / height;
            for(int j = height - 1; j >= 0; j--)
            {
                for(int i = 0; i < width; i++)
                {
                    Color color = new Color(0, 0, 0);
                    for(int s = 0; s < SAMPLE; s++)
                    {
                        Ray r = camera.CreateRay((i + _M.R()) * recip_width, (j + _M.R()) *recip_height);
                        color += GetColorForTestDielectric(r, hitalbeList, 0);
                    }
                    color *= SAMPLE_WEIGHT;
                    //为了使球体看起来更亮，改变gamma值
                    color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), 1f);
                    color.a = 1;
                    colors[i + j * width] = color;
                }
            }
            return colors;
        }
        #endregion

        #region 第十一版（测试景深）
        Color GetColorForTestDefocus(Ray ray, HitableList hitableList, int depth)
        {
            HitRecord record = new HitRecord();
            if(hitableList.Hit(ray, 0.0001f, float.MaxValue, ref record))
            {
                Ray r = new Ray(Vector3.zero, Vector3.zero);
                Color attenuation = Color.black;
                if(depth < MAX_SCATTER_TIME && record.material.scatter(ray, record, ref attenuation, ref r))
                {
                    Color c = GetColorForTestDefocus(r, hitableList, depth + 1);
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

        Color[] CreateColorForTestDefocus(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            //Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            //Vector3 horizontal = new Vector3(4, 0, 0);
            //Vector3 vertical = new Vector3(0, 2, 0);
            //Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            HitableList hitableList = new HitableList();
            //这里注释的两句话是随机场景渲染用的
            //HitableList hitableList = _M.CreateRandomScene();
            hitableList.list.Add(new Sphere(new Vector3(0, 0, -1), 0.5f, new Lambertian(new Color(0.2f, 0.2f, 0.8f))));
            hitableList.list.Add(new Sphere(new Vector3(0, -100.5f, -1f), 100f, new Lambertian(new Color(0.8f, 0.8f, 0.0f))));
            hitableList.list.Add(new Sphere(new Vector3(1, 0, -1), 0.5f, new Metal(new Color(0.8f, 0.6f, 0.2f), 0.0f)));
            hitableList.list.Add(new Sphere(new Vector3(-1, 0, -1), 0.5f, new Dielectric(1.5f)));
            Color[] colors = new Color[l];
            //Vector3 from = new Vector3(10f, 2f, -2);
            //Vector3 to = new Vector3(0, 1, 0);
            Vector3 from = new Vector3(0f, 1f, 2f);
            Vector3 to = new Vector3(0, 0, 0);
            Camera camera = new Camera(from, to, Vector3.up, 90, (float)width / height);
            //Camera camera = new Camera(from, to, Vector3.up, 35, width / height);
            float recip_widht = 1f / width;
            float recip_height = 1f / height;
            for(int j = height - 1; j >= 0; j--)
            {
                for(int i = 0; i < width; i++)
                {
                    Color color = new Color(0, 0, 0);
                    for(int s = 0; s < SAMPLE; s++)
                    {
                        Ray r = camera.CreateRay((i + _M.R()) * recip_widht, (j + _M.R()) * recip_height);
                        color += GetColorForTestDefocus(r, hitableList, 0);
                    }
                    color *= SAMPLE_WEIGHT;
                    //为了使球体看起来更亮，改变gamma值
                    color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), 1f);
                    color.a = 1f;
                    colors[i + j * width] = color;
                }
                EditorUtility.DisplayProgressBar("", "", j / (float)height);
            }
            EditorUtility.ClearProgressBar();
            return colors;
        }
        #endregion

        #region 第十二版（实现封面）
        HitableList GetRandomScene()
        {
            HitableList hitableList = new HitableList();
            hitableList.list.Add(new Sphere(new Vector3(0, -1000, 0), 1000, new Lambertian(new Color(0.5f, 0.5f, 0.5f))));
            for(int a = -11; a < 11; a++)
            {
                for(int b = -11; b < 11; b++)
                {
                    float choose_mat = _M.R();
                    Vector3 center = new Vector3(a + 0.9f * _M.R(), 0.2f, b + 0.9f * _M.R()) ;
                    if((center - new Vector3(4, 0.2f, 0)).magnitude > 0.9f)
                    {
                        if(choose_mat < 0.8) //diffuse
                        {
                            hitableList.list.Add(new Sphere(center, 0.2f, new Lambertian(new Color(_M.R() * _M.R(), _M.R() * _M.R(), _M.R() * _M.R()))));
                        }
                        else if(choose_mat < 0.95) //metal
                        {
                            hitableList.list.Add(new Sphere(center, 0.2f, new Metal(new Color(0.5f * (1 + _M.R()), 0.5f * (1 + _M.R()), 0.5f * _M.R()))));
                        }
                        else //glass
                        {
                            hitableList.list.Add(new Sphere(center, 0.2f, new Dielectric(1.5f)));
                        }
                    }
                    
                }
            }

            hitableList.list.Add(new Sphere(new Vector3(0, 1, 0), 1.0f, new Dielectric(1.5f)));
            hitableList.list.Add(new Sphere(new Vector3(-4, 1, 0), 1.0f, new Lambertian(new Color(0.4f, 0.2f, 0.1f))));
            hitableList.list.Add(new Sphere(new Vector3(4, 1, 0), 1.0f, new Metal(new Color(0.7f, 0.6f, 0.5f), 0.0f)));

            return hitableList;
        }

        Color[] CreateColorForTestScene(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            //Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            //Vector3 horizontal = new Vector3(4, 0, 0);
            //Vector3 vertical = new Vector3(0, 2, 0);
            //Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            //HitableList hitableList = new HitableList();
            //这里注释的两句话是随机场景渲染用的
            //HitableList hitableList = _M.CreateRandomScene();
            HitableList hitableList = GetRandomScene();
             Color[] colors = new Color[l];
            //Vector3 from = new Vector3(10f, 2f, -2);
            //Vector3 to = new Vector3(0, 1, 0);
            Vector3 from = new Vector3(13f, 2f, 3f);
            Vector3 to = new Vector3(0, 0, 0);
            Camera camera = new Camera(from, to, Vector3.up, 20, width / height, 0.0f, 0.7f * (from - to).magnitude);
            //Camera camera = new Camera(from, to, Vector3.up, 35, width / height);
            float recip_widht = 1f / width;
            float recip_height = 1f / height;
            for (int j = height - 1; j >= 0; j--)
            {
                for (int i = 0; i < width; i++)
                {
                    Color color = new Color(0, 0, 0);
                    for (int s = 0; s < SAMPLE; s++)
                    {
                        Ray r = camera.CreateRay((i + _M.R()) * recip_widht, (j + _M.R()) * recip_height);
                        color += GetColorForTestDefocus(r, hitableList, 0);
                    }
                    color *= SAMPLE_WEIGHT;
                    //为了使球体看起来更亮，改变gamma值
                    color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), 1f);
                    color.a = 1f;
                    colors[i + j * width] = color;

                    EditorUtility.DisplayProgressBar("", "", (float)(i + j * width) / (float)l);
                }
               
            }
            EditorUtility.ClearProgressBar();
            return colors;
        }

        #endregion

    }

}