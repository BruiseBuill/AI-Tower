using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BF.Utility
{
    public static class ColorExtend
    {
        /// <param name="color"></param>
        /// <param name="alpha"></param>
        /// <returns>!This func dont change value, you must use sth to receive return </returns>
        public static Color Alpha(this Color color,float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
    public static class Vector3IntExtend
    {
        public static Vector3Int Cross(this Vector3Int v1, Vector3Int v2)
        {
            return new Vector3Int(v1.y * v2.z - v1.z * v2.y, v1.z * v2.x - v1.x * v2.z, v1.x * v2.y - v2.x * v1.y);
        }

    }
    public static class Vector3Extend
    {
        public static Vector3 RotateVertical(this Vector3 orient,float angle)
        {
            var rad = angle * Mathf.Deg2Rad;
            var cosB = Mathf.Cos(rad);
            var sinB = Mathf.Sin(rad);
            return new Vector3(orient.x * cosB - orient.y * sinB, orient.y * cosB + orient.x * sinB, orient.z);
        }
        public static bool RotateOrientVertical(this Vector3 orient,Vector3 aimOrient)
        {
            return (orient.x * aimOrient.y - orient.y * aimOrient.x) > 0;
        }
    }
    public static class CameraExtend
    {
        public static Vector3 Screen2WorldProject(this Camera camera, Vector3 screenPos)
        {
            var pos = camera.ScreenToWorldPoint(screenPos);
            pos.z = 0;
            return pos;
        }
    }
}
