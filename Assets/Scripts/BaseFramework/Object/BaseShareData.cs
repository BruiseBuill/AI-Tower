using BF.Utility;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BF
{
    public class BaseShareData : MonoBehaviour
    {
        [FormerlySerializedAs("isAlive")]
        [SerializeField] DataWithEvent<bool> aliveState = new DataWithEvent<bool>();

        [FormerlySerializedAs("_IdentityCode")]
        [SerializeField] int identityCode;

        Sequece<BaseComponent> componentList;
        bool isInitialized;
        bool isOpening;
        bool isOpen;
        bool isClosing;
        bool closeRequestedWhileOpening;

        public bool IsAlive => aliveState.Value;
        public bool IsOpen => isOpen;
        public int IdentityCode => identityCode;

        // Data can request a lifecycle transition; only Control performs it.
        public event Action CloseRequested;

        protected virtual void Awake()
        {
            EnsureInitialized();
        }

        internal void EnsureInitialized()
        {
            if (isInitialized)
            {
                return;
            }

            componentList = new Sequece<BaseComponent>();
            isInitialized = true;
        }

        internal void Open()
        {
            EnsureInitialized();
            if (isOpen || isOpening)
            {
                return;
            }

            isOpening = true;
            identityCode++;
            aliveState.ResetData(true);

            try
            {
                OnOpening();

                for (int i = 0; i < componentList.Count; i++)
                {
                    componentList[i].OpenFromData();
                }

                OnOpened();
                isOpen = true;
            }
            catch
            {
                for (int i = componentList.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        componentList[i].CloseFromData();
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception, componentList[i]);
                    }
                }

                closeRequestedWhileOpening = false;
                isOpen = false;
                aliveState.ResetData(false);
                throw;
            }
            finally
            {
                isOpening = false;
            }

            if (closeRequestedWhileOpening)
            {
                closeRequestedWhileOpening = false;
                CloseRequested?.Invoke();
            }
        }

        internal void Close()
        {
            if (!isOpen || isClosing)
            {
                return;
            }

            isClosing = true;
            isOpen = false;
            try
            {
                try
                {
                    OnClosing();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }

                for (int i = componentList.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        componentList[i].CloseFromData();
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception, componentList[i]);
                    }
                }

                aliveState.ResetData(false);

                try
                {
                    OnClosed();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
            finally
            {
                aliveState.ResetData(false);
                isClosing = false;
            }
        }

        public void RequestClose()
        {
            if (isOpening)
            {
                closeRequestedWhileOpening = true;
            }
            else if (isOpen && !isClosing)
            {
                CloseRequested?.Invoke();
            }
        }

        internal void Register(BaseComponent component, int priority)
        {
            EnsureInitialized();
            componentList.Add(component, priority);

            if (isOpen)
            {
                component.OpenFromData();
            }
        }

        internal void Unregister(BaseComponent component)
        {
            if (!isInitialized)
            {
                return;
            }

            component.CloseFromData();
            componentList.Remove(component);
        }

        protected virtual void OnOpening() { }
        protected virtual void OnOpened() { }
        protected virtual void OnClosing() { }
        protected virtual void OnClosed() { }

        [Serializable]
        public class DataWithEvent<T>
        {
            public event UnityAction<T> onValueChange;
            public event UnityAction<T, T> onValueChange2Value;

            [SerializeField] protected T data;

            public virtual T Value
            {
                get => data;
                set => SetValue(value);
            }

            protected void SetValue(T value)
            {
                T previousValue = data;
                data = value;
                Publish(previousValue, data);
            }

            protected void Publish(T previousValue, T value)
            {
                onValueChange2Value?.Invoke(previousValue, value);
                onValueChange?.Invoke(value);
            }

            // Resets pooled runtime state without publishing a domain change.
            public void ResetData(T value)
            {
                data = value;
            }
        }

        [Serializable]
        public class DataWithEventHop : DataWithEvent<bool>
        {
            public override bool Value
            {
                get => base.Value;
                set
                {
                    if (value != data)
                    {
                        SetValue(value);
                    }
                }
            }
        }

        [Serializable]
        public abstract class DataWithVariableValue<T> : DataWithEvent<T>
        {
            [SerializeField] protected T additive;

            public T Additive => additive;
            public abstract T FullValue { get; }

            public abstract void AddAdditive(T value);

            public void ResetAdditive(T value)
            {
                additive = value;
            }
        }

        [Serializable]
        public class DataWithVariableFloat : DataWithVariableValue<float>
        {
            public override float FullValue => data + additive;

            public override float Value
            {
                get => base.Value;
                set
                {
                    float previousValue = FullValue;
                    data = value;
                    Publish(previousValue, FullValue);
                }
            }

            public override void AddAdditive(float value)
            {
                float previousValue = FullValue;
                additive += value;
                Publish(previousValue, FullValue);
            }
        }

        [Serializable]
        public class DataWithVariableInt : DataWithVariableValue<int>
        {
            public override int FullValue => data + additive;

            public override int Value
            {
                get => base.Value;
                set
                {
                    int previousValue = FullValue;
                    data = value;
                    Publish(previousValue, FullValue);
                }
            }

            public override void AddAdditive(int value)
            {
                int previousValue = FullValue;
                additive += value;
                Publish(previousValue, FullValue);
            }
        }
    }
}
