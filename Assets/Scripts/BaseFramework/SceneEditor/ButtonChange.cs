using BF.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BF.Tool
{
	public class ButtonChange : MonoBehaviour
	{
		[Header("Change Button To FullButton")]
		[SerializeField] string sfxName;
		[ContextMenu("Run")]
		void Change()
		{
			var btns = FindObjectsOfType<Button>();
            List<GameObject> list = new List<GameObject>();
            for (int i = 0; i < btns.Length; i++)
			{
				list.Add(btns[i].gameObject);
                DestroyImmediate(btns[i]);
			}
			for(int i = 0; i < list.Count; i++)
			{
				if (list[i].TryGetComponent<FullButton>(out FullButton btn))
				{
					//btn.SetSFX(sfxName);
				}
				else
				{
                    list[i].AddComponent<FullButton>();
                }				
            }
		}
		
	}
}