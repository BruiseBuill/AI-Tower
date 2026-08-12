using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BF
{
    public class GenericEventChannel<T, T1> : ScriptableObject
    {
        protected Action<T, T1> onEvent = delegate { };

        public void Invoke(T t, T1 t1)
        {
            onEvent.Invoke(t, t1);
        }
        public void AddListener(Action<T, T1> action)
        {
            onEvent += action;
        }
        public void RemoveListener(Action<T, T1> action)
        {
            onEvent -= action;
        }
    }
}