using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BF
{
    #region InputProtection
    [Serializable]
    public class InputProtection
    {
        //The necessary condition for this action
        //Normally, action can only be done when condition=true and input
        //Advance: input before condition=true
        //Lag: input after condition=false
        public Action<bool> onConditionChange = delegate { };
        public Action onPlayerInput = delegate { };

        public Action onAct = delegate { };

        public bool condition;
        public float lastConditionTrueTime;
        public bool hasInput;
        public float lastInputTime;

        public float advanceProtectionTime;
        public float lagProtectionTime;

        public void Initialize(float advanceProtectionTime, float lagProtectionTime)
        {
            this.advanceProtectionTime = advanceProtectionTime;
            this.lagProtectionTime = lagProtectionTime;

            condition = true;
            lastConditionTrueTime = Time.time;
            hasInput = false;
            lastInputTime = Time.time;

            onPlayerInput += OnPlayerInput;
            onConditionChange += OnConditionChange;
        }
        void OnPlayerInput()
        {
            hasInput = true;
            lastInputTime = Time.time;
            if (condition || Time.time - lastConditionTrueTime < lagProtectionTime)
            {
                hasInput = false;
                onAct.Invoke();
            }
        }
        void OnConditionChange(bool condition)
        {
            if (this.condition == condition)
            {
                return;
            }
            this.condition = condition;

            if (condition)
            {
                if (hasInput && Time.time - lastInputTime < advanceProtectionTime)
                {
                    hasInput = false;
                    onAct.Invoke();
                }
            }
            else
            {
                lastConditionTrueTime = Time.time;
            }
        }
    }
    #endregion
}
