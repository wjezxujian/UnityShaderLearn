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
        float fuzz;
        public Metal(Color a, float f = 0.0f)
        {
            albedo = a;
            fuzz = f < 1 ? f : 1;
        }

        public override bool scatter(Ray rayIn, HitRecord record, ref Color attenuation, ref Ray scattered)
        {
            Vector3 reflected = _M.reflect(rayIn.normalDirection, record.normal);
            scattered = new Ray(record.p, reflected + fuzz * _M.GetRandomPointInUnitSphere());
            attenuation = albedo;
            return Vector3.Dot(scattered.direction, record.normal) > 0;
        }

        Vector3 reflect(Vector3 vin, Vector3 normal)
        {
            return vin - 2 * Vector3.Dot(vin, normal) * normal;
        }
    }

    /// <summary>
    /// 透明折射模型
    /// </summary>
    /// 
    public class Dielectric : Material
    {
        //相对空气折射率
        float ref_idx;
        public Dielectric(float ri)
        {
            ref_idx = ri;
        }

        public override bool scatter(Ray rayIn, HitRecord record, ref Color attenuation, ref Ray scattered)
        {
            Vector3 outNormal;
            Vector3 reflected = _M.reflect(rayIn.direction, record.normal);
            //透明的物体当然不吸收任何光线
            attenuation = Color.white;
            float ni_no = 1f;
            Vector3 refracted = Vector3.zero;
            float cos = 0;
            //反射比
            float reflect_prob = 0;
            //假如光线是从介质内向介质外传播，那么法线就要反转一下
            if(Vector3.Dot(rayIn.direction, record.normal) > 0)
            {
                outNormal = -record.normal;
                ni_no = ref_idx;
                cos = ni_no * Vector3.Dot(rayIn.normalDirection, record.normal);
            }
            else
            {
                outNormal = record.normal;
                ni_no = 1f / ref_idx;
                cos = -Vector3.Dot(rayIn.normalDirection, record.normal);
            }

            //如果没发生折射，就用反射
            if (_M.refract(rayIn.direction, outNormal, ni_no, ref refracted))
            {
                reflect_prob = _M.schlick(cos, ref_idx);
            }
            else
            {
                //此时反射比为100%
                reflect_prob = 1;
            }

            //因为一条光线只会采样一个点，所以这里就用蒙特卡洛模拟的方法，用概率去决定数值
            if (Random.Range(0f, 1f) <= reflect_prob)
            {
                scattered = new Ray(record.p, reflected);
            }
            else
            {
                scattered = new Ray(record.p, refracted);
            }

            return true;
        }
    }

}
