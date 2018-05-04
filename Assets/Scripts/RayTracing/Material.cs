using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RayTracying
{
    public abstract class Material
    {
        /// <summary>
        /// 材质表面发生的光线变化过程
        /// </summary>
        /// <param name="rayIn"></param>
        /// <param name="record"></param>
        /// <param name="attenuation"></param>
        /// <param name="scattered"></param>
        /// <returns>是否发生了光线变化</returns>
        public abstract bool scatter(Ray rayIn, HitRecord record, ref Color attenuation, ref Ray scattered);
    }

    /// <summary>
    /// 理想的反射模型
    /// </summary>
    /// 
    public class Lambertian : Material
    {
        Color albedo;
        public override bool scatter(Ray rayIn, HitRecord record, ref Color attenuation, ref Ray scattered)
        {
            Vector3 target = record.p + record.normal + GetRandomPointInUnitSphere();
            scattered = new Ray(record.p, target - record.p);
            attenuation = albedo;

            return true;
        }

        private Vector3 GetRandomPointInUnitSphere()
        {
            Vector3 p = 2f * new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)) - Vector3.one;
            p = p.normalized * Random.Range(0f, 1f);
            return p;
        }

        public Lambertian(Color a)
        {
            albedo = a;
        }
    }

    /// <summary>
    /// 理想的镜面反射模型
    /// </summary>
    /// 
    public class Metal : Material
    {
        Color albedo;
        public Metal(Color a)
        {
            albedo = a;
        }

        public override bool scatter(Ray rayIn, HitRecord record, ref Color attenuation, ref Ray scattered)
        {
            Vector3 reflected = reflect(rayIn.normalDirection, record.normal);
            scattered = new Ray(record.p, reflected);
            attenuation = albedo;
            return Vector3.Dot(scattered.direction, record.normal) > 0;
        }

        Vector3 reflect(Vector3 vin, Vector3 normal)
        {
            return vin - 2 * Vector3.Dot(vin, normal) * normal;
        }
    }

}
