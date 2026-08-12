using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BF
{
    public class Single<T> : MonoBehaviour where T : Component
    {
        static T instance;
        public static T Instance()
        {
            if (!instance)
            {
                instance = FindObjectOfType<T>();
                if (FindObjectsOfType<T>().Length > 1)//
                {
                    Debug.LogError("Error");
                }
            }
            return instance;
        }
    }
}

