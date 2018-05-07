using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RayTracying
{
    public class _M
    {
        public static Vector3 GetRandomPointInUnitSphere()
        {
            Vector3 p = 2f * new Vector3(_M.R(), _M.R(), 0) - new Vector3(1, 1, 0);
            p = p.normalized * _M.R();
            return p;
        }

        /// <summary>
        /// 这个去的是圆而不是球
        /// </summary>
        /// <param name="vin"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static Vector3 GetRandomPointInUnitDisk()
        {
            Vector3 p = 2f * new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0) - new Vector3(1, 1, 0);
            p = p.normalized * Random.Range(0f, 1f);
            return p;
        }

        public static Vector3 reflect(Vector3 vin, Vector3 normal)
        {
            return vin - 2 * Vector3.Dot(vin, normal) * normal;
        }

        public static bool refract(Vector3 vin, Vector3 normal, float ni_no, ref Vector3 refracted)
        {
            Vector3 uvin = vin.normalized;
            float dt = Vector3.Dot(uvin, normal);
            float discrimination = 1 - ni_no * ni_no * (1 - dt * dt);
            if(discrimination > 0)
            {
                refracted = ni_no * (uvin - normal * dt) - normal * Mathf.Sqrt(discrimination);
                return true;
            }

            return false;
        }
        /// <summary>
        /// Schiiick近似菲涅尔反射
        /// </summary>
        /// <returns>返回的是菲尼尔反射比</returns>
        /// 
        public static float schlick(float cos, float ref_idx)
        {
            float r0 = (1 - ref_idx) / (1 + ref_idx);
            r0 *= r0;
            return r0 + (1 - r0) * Mathf.Pow((1 - cos), 5);
        }

        public static float R()
        {
            return Random.Range(0f, 1f);
        }
    }
}