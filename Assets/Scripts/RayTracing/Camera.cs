using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RayTracying
{
    public class Camera
    {
        public Vector3 position;
        public Vector3 lowLeftCorner;
        public Vector3 horizontal;
        public Vector3 vertical;
        public Vector3 u, v, w;
        public float radius;

        public Camera(Vector3 pos, Vector3 lowLeftCorner, Vector3 hor, Vector3 ver)
        {
            position = pos;
            this.lowLeftCorner = lowLeftCorner;
            horizontal = hor;
            vertical = ver;
        }

        public Camera(float vfov, float aspect)
        {
            float unitAngle = Mathf.PI / 180f * vfov;
            float halfHeight = Mathf.Tan(unitAngle * 0.5f);
            float halfWidth = aspect * halfHeight;
            lowLeftCorner = new Vector3(-halfWidth, -halfHeight, -1f);
            horizontal = new Vector3(2 * halfWidth, 0, 0);
            vertical = new Vector3(0, 2 * halfHeight, 0);
            position = Vector3.zero;
        }

        public Camera(Vector3 lookFrom, Vector3 lookat, Vector3 vup, float vfov, float aspect, float r = 0, float focus_dist = 1)
        {
            radius = r * 0.5f;
            float unitAngle = Mathf.PI / 180f * vfov;
            float halfHeight = Mathf.Tan(unitAngle * 0.5f);
            float halfWidth = aspect * halfHeight;
            position = lookFrom;
            w = (lookFrom - lookat).normalized;
            u = Vector3.Cross(vup, w).normalized;
            v = Vector3.Cross(w, u).normalized;
            lowLeftCorner = lookFrom - w * focus_dist - halfWidth * u * focus_dist - halfHeight * v * focus_dist;
            horizontal = 2 * halfWidth * focus_dist * u;
            vertical = 2 * halfHeight * focus_dist * v;
        }

        public Ray CreateRay(float x, float y)
        {
            ///假如光圈为0就不随机了，节省资源
            if (radius == 0f)
                return new Ray(position, lowLeftCorner + x * horizontal + y * vertical - position);
            else
            {
                Vector3 rd = radius * _M.GetRandomPointInUnitDisk();
                Vector3 offset = rd.x * u + rd.y * v;
                return new Ray(position + offset, lowLeftCorner + x * horizontal + y * vertical - position - offset);
            }

        }

        //public Camera(Vector3 lookFrom, Vector3 lookat, Vector3 vUp, float vFov, float aspect, float r = 0, float focus_dist = 1)

    }
}