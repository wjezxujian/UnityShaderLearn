using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ray
{
    public Vector3 original;
    public Vector3 direction;
    public Vector3 normalDirection;
    public Ray (Vector3 o, Vector3 d)
    {
        original = o;
        direction = d;
        normalDirection = d.normalized;
    }

    public Vector3 GetPoint(float t)
    {
        return original + t * direction;
    }

}

