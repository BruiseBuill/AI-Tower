using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BF
{
	public abstract class BaseComponent : MonoBehaviour
	{
        protected BaseShareData data;
        [Tooltip("LowerPriorityWillRunFirst")]
        [SerializeField] protected int priority;
        protected virtual void Awake()
        {
            data = GetComponentInChildren<BaseShareData>();
            data.Register(this, priority);
            Debug.Log(3);
        }
        public abstract void Open();
        public abstract void Close();
    }
}