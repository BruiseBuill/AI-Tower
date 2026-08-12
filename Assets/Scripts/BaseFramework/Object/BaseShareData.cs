using BF.Utility;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace BF
{
	public class BaseShareData : MonoBehaviour
	{
        public DataWithEvent<bool> isAlive;
		public int _IdentityCode;
        public Sequece<BaseComponent> componentList;

        bool isInitialized;

        public virtual void Awake()
		{
            isInitialized = true;
            componentList = new Sequece<BaseComponent>();
		}
		public virtual void Open()
		{
            isAlive.ResetData(true);

            isAlive.onValueChange += (alive) => 
            {
                if (!alive) 
                    Close();
            };
			_IdentityCode++;

            for (int i = 0; i < componentList.Count; i++)
            {
                componentList[i].Open();
            }
        }
		public virtual void Close()
		{
            for (int i = componentList.Count; i >= 0; i--) 
            {
                componentList[i].Close();
            }
            isAlive.onValueChange = delegate { };
        }
        public void Register(BaseComponent component, int priority)
        {
            if (!isInitialized)
            {
                Awake();
            }
            componentList.Add(component, priority);
        }
        public void Unregister(BaseComponent component)
        {
            componentList.Remove(component);
        }
        #region Data
        [Serializable]
		public class DataWithEvent<T>
		{
			public UnityAction<T> onValueChange = delegate { };
			public UnityAction<T, T> onValueChange2Value = delegate { };
            [SerializeField] protected T data;

            /// <summary>
            /// 赋值后会触发事件
            /// </summary>
            public virtual T Value 
            {
                get => data;			
                set
				{
					onValueChange2Value.Invoke(data, value);
                    data = value;
					onValueChange.Invoke(data);
				}
			}
			public DataWithEvent()
			{
				onValueChange = delegate { };
                onValueChange2Value = delegate { };
            }
            /// <summary>
            /// 给data初始化，不会触发事件，用来在open当中调用
            /// </summary>
            public void ResetData(T data)
            {
                this.data = data;

            }
        }
		[Serializable]
		public class DataWithEventHop: DataWithEvent<bool> 
		{
            public override bool Value 
			{ 
				get => base.Value;
				set 
				{
					if (value != data) 
					{
                        data = value;
                        onValueChange.Invoke(data);
                    }
				}
			}
		}
        #endregion
        #region Data with Additive
        [Serializable]
		public abstract class DataWithVariableValue<T> : DataWithEvent<T> 
		{
            protected T additive;
            public T Additive
            {
                get => additive;
            }
			public virtual T FullValue
			{
				get;
			}
            
            public abstract void AddAdditive(T additive);
            /// <summary>
            /// 给additive初始化，不会触发事件，用来在open当中调用
            /// </summary>
            public void ResetAdditive(T additive)
            {
                this.additive = additive;
            }
        }
		[Serializable]
        public class DataWithVariableFloat : DataWithVariableValue<float>
		{
            public override float FullValue 
			{
				get => data + additive;
			}
            public override float Value
            {
                get => base.Value;
                set
                {
                    onValueChange2Value.Invoke(FullValue,Additive+ value); 
                    data = value;
                    onValueChange.Invoke(FullValue);
                }
            }
            public override void AddAdditive(float additive)
            {
				onValueChange2Value.Invoke(FullValue, FullValue + additive);
				this.additive += additive;
				onValueChange.Invoke(FullValue);
            }
        }
        [Serializable]
        public class DataWithVariableInt : DataWithVariableValue<int>
        {
            public override int FullValue
            {
                get => data + Additive;
            }
            public override int Value
            {
                get => base.Value;
                set
                {
                    onValueChange2Value.Invoke(FullValue, Additive + value);
                    data = value;
                    onValueChange.Invoke(FullValue);
                }
            }
            public override void AddAdditive(int additive)
            {
                onValueChange2Value.Invoke(FullValue, FullValue + additive);
                this.additive += additive;
                onValueChange.Invoke(FullValue);
            }
        }
        #endregion
    }
}