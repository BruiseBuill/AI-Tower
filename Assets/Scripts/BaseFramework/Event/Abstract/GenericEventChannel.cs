using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BF
{
    public class GenericEventChannel<T> : ScriptableObject
	{
        protected Action<T> onEvent = delegate { };

        public void Invoke(T t)
        {
            onEvent.Invoke(t);
        }
        public void AddListener(Action<T> action)
        {
            onEvent += action;
        }
        public void RemoveListener(Action<T> action)
        {
            onEvent -= action;
        }
    }
    
    
}