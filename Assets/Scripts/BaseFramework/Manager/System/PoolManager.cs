using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BF 
{
    public class PoolManager : Single<PoolManager>
    {
        [SerializeField] Pool[] itemPool;
        [SerializeField] Pool[] vfxPool;
        [SerializeField] Pool[] characterPool;
        [SerializeField]
        Pool[] uiPool;

        Dictionary<string, Pool> dictionary = new Dictionary<string, Pool>();

        protected void Awake()
        {
            int i = 0;
            for (i = 0; i < itemPool.Length; i++)
            {
                itemPool[i].Initialize(transform);
                dictionary.Add(itemPool[i].prefab.name, itemPool[i]);
            }
            for(i = 0; i < vfxPool.Length; i++)
            {
                vfxPool[i].Initialize(transform);
                dictionary.Add(vfxPool[i].prefab.name, vfxPool[i]);
            }
            for (i = 0; i < characterPool.Length; i++)
            {
                characterPool[i].Initialize(transform);
                dictionary.Add(characterPool[i].prefab.name, characterPool[i]);
            }
            for (i = 0; i < uiPool.Length; i++)
            {
                uiPool[i].Initialize(transform);
                dictionary.Add(uiPool[i].prefab.name, uiPool[i]);
            }
        }
        private void Start()
        {
            TransitManager.Instance().onSceneUnload += RecycleAll;
        }
        public GameObject Release(string a)
        {
#if UNITY_EDITOR
            if (!dictionary.ContainsKey(a))
            {
                Debug.Log(string.Format("PoolManagerError,no such key:{0}", a));
            }
#endif 
            return dictionary[a].GetFromPool();
        }
        public void Recycle(GameObject a)
        {
            dictionary[a.name].BackToPool(a);
        }
        public bool IsContain(string a)
        {
            return dictionary.ContainsKey(a);
        }
        void RecycleAll(Scene scene) => RecycleAll();
        public void RecycleAll()
        {
            int i = 0;
            for (i = 0; i < itemPool.Length; i++)
            {
                itemPool[i].Recycle();
            }
            for (i = 0; i < vfxPool.Length; i++)
            {
                vfxPool[i].Recycle();
            }
            for (i = 0; i < characterPool.Length; i++)
            {
                characterPool[i].Recycle();
            }
            for (i = 0; i < uiPool.Length; i++)
            {
                uiPool[i].Recycle();
            }

        }
        private void OnDisable()
        {
            TransitManager.Instance().onSceneUnload -= RecycleAll;
        }
        [Serializable]
        class Pool
        {
            public GameObject prefab;
            public int size;
            Queue<GameObject> queue = new Queue<GameObject>();
            List<GameObject> list = new List<GameObject>();
            Transform transParent;
            void Create()
            {
                GameObject a = GameObject.Instantiate(prefab);
                a.transform.SetParent(transParent);
                a.SetActive(false);
                a.name = prefab.name;
                queue.Enqueue(a);
            }
            public void Initialize(Transform parent)
            {
                transParent = parent;
                for (int i = 0; i < size; i++)
                {
                    Create();
                }
            }
            public GameObject GetFromPool()
            {
                GameObject a;
                if (queue.Count <= 0)
                {
                    Create();
                }
                a = queue.Dequeue();
                list.Add(a);
                return a;
            }
            public void BackToPool(GameObject a)
            {
                a.SetActive(false);
#if UNITY_EDITOR    
                if (queue.Contains(a))
                {
                    Debug.LogError(a.transform.position);
                    Debug.LogError(a.name);
                }
#endif
                list.Remove(a);
                queue.Enqueue(a);
            }
            public void Recycle()
            {
                while(list.Count > 0)
                {
                    BackToPool(list[0]);
                } 
            }
        }
    }
}



