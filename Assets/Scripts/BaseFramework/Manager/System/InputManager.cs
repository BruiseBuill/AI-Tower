using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace BF
{
    public class InputManager : Single<InputManager>
    {
        [SerializeField] bool canMouseInput = true;
        public bool CanInput
        {
            get => canMouseInput;
            set
            {
                canMouseInput = value;
                lastPressPoint = Input.mousePosition;
            }
        }

        // Position in these delegates is physical screen position. When screen become bigger, the same start and the end of ONDRAG will represent shorter physical distance   
        public static UnityAction<Vector3> onPointerDown = delegate { };
        public static UnityAction<Vector3> onPointerUp = delegate { };

        public static UnityAction<Vector3, Vector3> onDrag = delegate { };
        public static UnityAction<Vector3, Vector3> onDragEnd = delegate { };
        public static UnityAction<Vector3> onDragCancel = delegate { };
        public static UnityAction<Vector3> onClick = delegate { };
        public static UnityAction<Vector3> onDoubleClick = delegate { };
        public static UnityAction<Vector3, float> onLongPress = delegate { };
        public static UnityAction<Vector3, float> onLongPressEnd = delegate { };
        public static UnityAction onClickRight = delegate { };

        [SerializeField] Vector3 minTouchPort = new Vector3(0, 0, 0);
        [SerializeField] Vector3 maxTouchPort = new Vector3(1920, 1080, 0);
        [SerializeField] Vector3 referenceTouchResolution = new Vector3(1920, 1080, 0);
        public Vector3 ReferenceResolution => referenceTouchResolution;

        Vector3 minScreenPort;
        Vector3 maxScreenPort;
        //
        [SerializeField] float dragOffset = 20f;
        [SerializeField] float minLongPressTime = 0.25f;
        [SerializeField] float doubleClickTime = 0.1f;
        float sqrDragOffset;
        ///WaitForSeconds wait_DoubleClickTime;
        float lastPressTime;
        Vector3 lastPressPoint;
        //
        bool isPressValid;
        //When mouse down, you can switch LONGPRESS to DRAG. But you can not switch DRAG to LONGPRESS
        [SerializeField] bool isDrag;
        bool isLongPress;
        //
        bool isSingleClickLast;
        //
        Dictionary<string, KeyEvent> keyEventDic = new Dictionary<string, KeyEvent>();

        public ref Action GetKeyEvent(KeyCode keyCode, KeyCondition keyCondition)
        {
            string keyName = keyCode.ToString() + keyCondition.ToString();
            if (!keyEventDic.ContainsKey(keyName))
            {
                switch (keyCondition)
                {
                    case KeyCondition.Up:
                        keyEventDic.Add(keyName, new KeyEventUp(keyCode, keyCondition));
                        break;
                    case KeyCondition.Down:
                        keyEventDic.Add(keyName, new KeyEventDown(keyCode, keyCondition));
                        break;
                }
            }
            return ref keyEventDic[keyName].onKey;
        }

        private void Awake()
        {
            Vector3 scale = new Vector3(Screen.width / referenceTouchResolution.x, Screen.height / referenceTouchResolution.y, 0);

            sqrDragOffset = dragOffset * dragOffset * scale.x * scale.y;
            minScreenPort = new Vector3(minTouchPort.x * scale.x, minTouchPort.y * scale.y, 0);
            maxScreenPort = new Vector3(maxTouchPort.x * scale.x, maxTouchPort.y * scale.y, 0);

#if UNITY_ANDROID
#endif
#if UNITY_STANDALONE_WIN
#endif
        }
        private void Update()
        {
            if (!canMouseInput)
            {
                return;
            }
            MouseCheck();
#if UNITY_STANDALONE_WIN
            KeyboardCheck();
#endif
        }
        void MouseCheck()
        {
            if (Input.GetMouseButtonUp(1))
            {
                onClickRight.Invoke();
            }

            if (!TouchPortCheck(Input.mousePosition))
                return;

            if (Input.GetMouseButtonDown(0))
            {
                //DoubleClick
                onPointerDown.Invoke(Input.mousePosition);
                isPressValid = true;
                isDrag = false;
                if (!isSingleClickLast || (isSingleClickLast && Time.time - lastPressTime > doubleClickTime) || (lastPressPoint - Input.mousePosition).sqrMagnitude > sqrDragOffset)
                {
                    isSingleClickLast = false;
                    lastPressTime = Time.time;
                    lastPressPoint = Input.mousePosition;
                }
            }
            else if (Input.GetMouseButtonUp(0) && isPressValid)
            {
                onPointerUp.Invoke(Input.mousePosition);
                isPressValid = false;
                
                if ((lastPressPoint - Input.mousePosition).sqrMagnitude < sqrDragOffset)
                {
                    if (isDrag)
                    {
                        onDragCancel.Invoke(Input.mousePosition);
                    }
                    else if (Time.time - lastPressTime < doubleClickTime)
                    {
                        if (isSingleClickLast)
                        {
                            isSingleClickLast = false;
                            onDoubleClick.Invoke(Input.mousePosition);
                        }
                        else
                        {
                            isSingleClickLast = true;
                            onClick.Invoke(Input.mousePosition);
                        }
                    }
                    else if (Time.time - lastPressTime < minLongPressTime)
                    {
                        isSingleClickLast = true;
                        onClick.Invoke(Input.mousePosition);
                    }
                    else //if (Time.time - lastPressTime > minLongPressTime)
                    {
                        onLongPressEnd.Invoke(Input.mousePosition, Time.time - lastPressTime);
                        isSingleClickLast = false;  
                    }
                }
                else
                {
                    onDragEnd.Invoke(lastPressPoint, Input.mousePosition);
                    isSingleClickLast = false;
                }
            }
            else if (Input.GetMouseButton(0))
            {
                if ((lastPressPoint - Input.mousePosition).sqrMagnitude > sqrDragOffset)
                {
                    onDrag.Invoke(lastPressPoint, Input.mousePosition);
                    isDrag = true;
                    if (isLongPress)
                    {
                        isLongPress = false;
                    }
                }
                else if (Time.time - lastPressTime > minLongPressTime && !isDrag)
                {
                    isLongPress = true;
                    onLongPress.Invoke(Input.mousePosition, Time.time - lastPressTime);
                }
            }
        }
        void KeyboardCheck()
        {
            foreach(var pair in keyEventDic)
            {
                pair.Value.Check();
            }
        }
        bool TouchPortCheck(Vector3 mousePos)
        {
            return mousePos.x > minScreenPort.x && mousePos.x < maxScreenPort.x && mousePos.y > minScreenPort.y && mousePos.y < maxScreenPort.y && !EventSystem.current.IsPointerOverGameObject();
        }


        [Serializable]
        abstract class KeyEvent
        {
            public Action onKey = delegate { };
            [SerializeField] protected KeyCode keyCode;
            [SerializeField] protected KeyCondition condition;
            public KeyEvent(KeyCode keyCode, KeyCondition condition)
            {
                onKey = delegate { };
                this.keyCode = keyCode;
                this.condition = condition;
            }
            public abstract void Check();
        }
        class KeyEventUp : KeyEvent
        {
            public KeyEventUp(KeyCode keyCode, KeyCondition condition) : base(keyCode, condition) { }
            public override void Check()
            {
                if (Input.GetKeyUp(keyCode))
                    onKey.Invoke();
            }
        }
        class KeyEventDown : KeyEvent
        {
            public KeyEventDown(KeyCode keyCode, KeyCondition condition) : base(keyCode, condition) { }
            public override void Check()
            {
                if (Input.GetKeyDown(keyCode))
                    onKey.Invoke();
            }
        }  
    }


}