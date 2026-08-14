using System;
using UnityEngine;

namespace BF
{
	public abstract class BaseComponent : MonoBehaviour
	{
        [SerializeField] BaseShareData sharedData;
        [Tooltip("LowerPriorityWillRunFirst")]
        [SerializeField] protected int priority;

        bool isOpen;

        protected BaseShareData data => sharedData;
        protected virtual Type RequiredDataType => typeof(BaseShareData);

        protected virtual void Awake()
        {
            if (sharedData == null)
            {
                sharedData = GetComponentInChildren(RequiredDataType, true) as BaseShareData;
            }

            if (sharedData == null)
            {
                sharedData = GetComponentInParent(RequiredDataType, true) as BaseShareData;
            }

            if (sharedData == null)
            {
                Debug.LogError($"{GetType().Name} requires {RequiredDataType.Name} in its hierarchy.", this);
                enabled = false;
                return;
            }

            sharedData.Register(this, priority);
        }

        protected virtual void OnDestroy()
        {
            if (sharedData != null)
            {
                sharedData.Unregister(this);
            }
        }

        internal void OpenFromData()
        {
            if (isOpen)
            {
                return;
            }

            isOpen = true;
            OnOpen();
        }

        internal void CloseFromData()
        {
            if (!isOpen)
            {
                return;
            }

            isOpen = false;
            OnClose();
        }

        protected abstract void OnOpen();
        protected abstract void OnClose();
    }

    public abstract class BaseComponent<TData> : BaseComponent where TData : BaseShareData
    {
        protected new TData data => (TData)base.data;
        protected override Type RequiredDataType => typeof(TData);
    }
}
