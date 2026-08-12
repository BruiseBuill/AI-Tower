using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BF
{
	[CreateAssetMenu(fileName = "EventChannel", menuName = "BF/EventChannel")]
	public class EventChannel : ScriptableObject
	{
		Action onEvent = delegate { };

		public void Invoke()
		{
			onEvent.Invoke();
		}
		public void AddListener(Action action)
		{
			onEvent += action;
        }
		public void RemoveListener(Action action)
		{
            onEvent -= action;
        }
    }
}