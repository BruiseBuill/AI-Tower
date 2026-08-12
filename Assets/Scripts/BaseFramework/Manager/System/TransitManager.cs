using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace BF
{
    public class TransitManager : Single<TransitManager>
    {
        public UnityAction<Scene> onSceneUnload = delegate { };
        public UnityAction<Scene> onSceneLoad = delegate { };
        public UnityAction<Scene> onSceneLoadOver = delegate { };

        public float LoadProgress 
        {
            get
            {
                if (asyncOperation != null)
                    return asyncOperation.progress;
                return 0;
            }
        }
        [SerializeField] CanvasGroup group;
        [SerializeField] string initialScene;
        [SerializeField] float transitTime;
        AsyncOperation asyncOperation;

        private void Start()
        {
            group.blocksRaycasts = false;
            group.alpha = 0;
            SceneManager.sceneLoaded += (scene, mode) => onSceneLoadOver.Invoke(scene);
#if !UNITY_EDITOR
        TransitScene(null, initialScene);
#endif
        }
        public void TransitScene(string fromNowTo)
        {
            var nowScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1).name;
            TransitScene(nowScene, fromNowTo);
        }
        public void TransitScene(string from, string to)
        {
            StartCoroutine(ChangeScene(from, to));
        }
        IEnumerator ChangeScene(string from, string to)
        {
            onSceneUnload.Invoke(SceneManager.GetSceneByName(from));
            yield return null;
            yield return Fade(1);
            if (from != null)
            {
                yield return SceneManager.UnloadSceneAsync(from);
            }
            asyncOperation = SceneManager.LoadSceneAsync(to, LoadSceneMode.Additive);
            yield return asyncOperation;

            Scene scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            SceneManager.SetActiveScene(scene);

            asyncOperation = null;
            yield return Fade(0);
        }
        IEnumerator Fade(float alpha)
        {
            group.blocksRaycasts = true;
            float speed = Mathf.Abs(group.alpha - alpha) / transitTime;

            while (!Mathf.Approximately(group.alpha, alpha))
            {
                group.alpha = Mathf.MoveTowards(group.alpha, alpha, speed * Time.deltaTime);
                yield return null;
            }
            group.blocksRaycasts = false;
        }
    }
}

