using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BF.Tool
{
 	public class TextFontChange : MonoBehaviour
	{
        [SerializeField] TMP_FontAsset defaultFont;
        [SerializeField] List<string> specialTagTextList;
        [SerializeField] List<TMP_FontAsset> specialFontList;

        [ContextMenu("Run")]
        void Run()
        {
            var texts = FindObjectsOfType<TextMeshProUGUI>();
            
            for (int i = 0; i < texts.Length; i++) 
            {
                if (!specialTagTextList.Contains(texts[i].gameObject.tag))
                    texts[i].font = defaultFont;
                else
                {
                    texts[i].font = specialFontList[specialTagTextList.IndexOf(texts[i].gameObject.tag)];
                }
            }
        }
	}
}
