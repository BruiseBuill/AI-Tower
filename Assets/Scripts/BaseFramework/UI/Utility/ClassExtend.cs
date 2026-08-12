using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BF.UI
{
    
    public static class TextMeshProExtend
    {
        public static Tweener DoText(this TextMeshProUGUI target, string endValue, float duration, bool richTextEnabled = false, ScrambleMode scrambleMode = ScrambleMode.None, string scrambleChars = null)
        {
            //target.maxVisibleCharacters = 0;
            target.text = "";
            return DOTween.To(() => target.text, x => target.text = x, endValue, duration)
                      .SetOptions(richTextEnabled, scrambleMode, scrambleChars)
                      .OnStart(() => target.ForceMeshUpdate());
        }
    }
}