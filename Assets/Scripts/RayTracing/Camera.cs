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

        public Camera(Vector3 pos, Vector3 lowLeftCorner, Vector3 hor, Vector3 ver)
        {
            position = pos;
            this.lowLeftCorner = lowLeftCorner;
            horizontal = hor;
            vertical = ver;
        }

        public Ray CreateRay(float u, float v)
        {
            return new Ray(position, lowLeftCorner + u * horizontal + v * vertical - position);
        }

        //public Camera(Vector3 lookFrom, Vector3 lookat, Vector3 vUp, float vFov, float aspect, float r = 0, float focus_dist = 1)

    }
}