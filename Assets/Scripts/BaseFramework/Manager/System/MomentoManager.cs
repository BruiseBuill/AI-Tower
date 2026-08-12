using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BF
{
    [System.Serializable]
    public abstract class BaseMomento
    {
        public string key;
    }
    public class MomentoManager : Single<MomentoManager>
    {
        public BaseMomento CurrentMomento { get; private set; }
        private const string auto_Key = "AutoMomento";
        private const string manual_KEY_0 = "ManualMomento_0";
        private const string manual_KEY_1 = "ManualMomento_1";
        private const string manual_KEY_2 = "ManualMomento_2";

        public void CreateEmptyMomento()
        {

        }
        public void SaveMomento()
        {
            ES3.Save(CurrentMomento.key, CurrentMomento);
        }
        public void LoadMomento(string key)
        {
            if (ES3.KeyExists(key))
            {
                CurrentMomento = ES3.Load<BaseMomento>(key);
            }
        }
        public void DeleteMomento(string key)
        {
            ES3.DeleteKey(key);
        }
    }
}