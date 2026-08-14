using System;
using UnityEngine;

namespace BF
{
	public interface ControlInit { }

	public abstract class BaseControl : BaseObject
	{
		[SerializeField] BaseShareData sharedData;

        bool isOpening;
        bool isClosing;
        bool closeRequestedWhileOpening;

        protected BaseShareData data => sharedData;
        protected virtual Type RequiredDataType => typeof(BaseShareData);

        public bool IsAlive => sharedData != null && sharedData.IsAlive;
        public int IdentityCode => sharedData == null ? 0 : sharedData.IdentityCode;

        public event Action<BaseControl> Opened;
        public event Action<BaseControl> Closed;

		protected virtual void Awake()
		{
			if (sharedData == null)
            {
                sharedData = GetComponentInChildren(RequiredDataType, true) as BaseShareData;
            }

            if (sharedData == null)
            {
                Debug.LogError($"{GetType().Name} requires {RequiredDataType.Name} in its hierarchy.", this);
                enabled = false;
                return;
            }

            sharedData.EnsureInitialized();
            sharedData.CloseRequested += HandleCloseRequested;
		}

        protected virtual void OnDestroy()
        {
            if (sharedData != null)
            {
                sharedData.CloseRequested -= HandleCloseRequested;
            }
        }

        public abstract void Initialize(ControlInit parameters);

        public void Open()
        {
            if (sharedData == null || sharedData.IsOpen || isOpening || isClosing)
            {
                return;
            }

            isOpening = true;
            try
            {
                OnOpening();
                sharedData.Open();

                if (!sharedData.IsOpen)
                {
                    return;
                }

                OnOpened();
                Opened?.Invoke(this);
            }
            catch
            {
                if (sharedData.IsOpen)
                {
                    sharedData.Close();
                }

                throw;
            }
            finally
            {
                isOpening = false;

                if (closeRequestedWhileOpening)
                {
                    closeRequestedWhileOpening = false;
                    Close();
                }
            }
        }

        public override void Close()
        {
            if (isOpening)
            {
                closeRequestedWhileOpening = true;
                return;
            }

            if (sharedData == null || !sharedData.IsOpen || isClosing)
            {
                return;
            }

            isClosing = true;
            try
            {
                OnClosing();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }

            try
            {
                sharedData.Close();

                try
                {
                    OnClosed();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }

                Closed?.Invoke(this);
            }
            finally
            {
                isClosing = false;
            }
        }

        protected virtual void OnOpening() { }
        protected virtual void OnOpened() { }
        protected virtual void OnClosing() { }
        protected virtual void OnClosed() { }

        void HandleCloseRequested()
        {
            if (isOpening)
            {
                closeRequestedWhileOpening = true;
            }
            else
            {
                Close();
            }
        }
	}

    public abstract class BaseControl<TData> : BaseControl where TData : BaseShareData
    {
        protected new TData data => (TData)base.data;
        protected override Type RequiredDataType => typeof(TData);
    }

    public abstract class BaseControl<TData, TInit> : BaseControl<TData>
        where TData : BaseShareData
        where TInit : ControlInit
    {
        public sealed override void Initialize(ControlInit parameters)
        {
            if (!(parameters is TInit typedParameters))
            {
                throw new ArgumentException(
                    $"{GetType().Name} expects initialization data of type {typeof(TInit).Name}.",
                    nameof(parameters));
            }

            Initialize(typedParameters);
        }

        public abstract void Initialize(TInit parameters);
    }
}
