using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BF
{
    public class GameManager : Single<GameManager>
    {
        [SerializeField] protected bool isGame;
        public bool IsGame
        {
            get => isGame;
            set
            {
                if (isGame == value)
                {
                    return;
                }
                isGame = value;
            }
        }

        [SerializeField] protected bool isPlaying;
        public bool IsPlaying 
        {
            get => isPlaying;
            set
            {
                if (isPlaying == value)
                {
                    return;
                }
                isPlaying = value;
                if (isPlaying)
                {
                    InputManager.Instance().CanInput = true;
                    Time.timeScale = 1;
                }
                else
                {
                    InputManager.Instance().CanInput = false;   
                    Time.timeScale = 0;
                }
            }
        }
        
        private void Awake()
        {
            if (Instance().gameObject != gameObject)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            //
        }
        public static void ExitGame()
        {
            
    #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
    #endif
            Application.Quit();
        }
    }
}

