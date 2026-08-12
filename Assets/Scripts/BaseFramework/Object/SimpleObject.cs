using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BF
{
    public class SimpleObject : BaseObject
    {
        [SerializeField] protected bool hasLifeTime;
        [SerializeField] protected float lifeTime;
        protected WaitForSeconds wait_LifeTime;

        protected virtual void Awake()
        {
            wait_LifeTime = new WaitForSeconds(lifeTime);
        }
        public virtual void Open(Vector3 pos)
        {
            transform.position = pos;
            gameObject.SetActive(true);
            if (hasLifeTime)
            {
                StartCoroutine("WaitDie");
            }
        }
        public virtual void Open(Vector3 pos, Vector3 orient)
        {
            transform.up = orient;
            Open(pos);
        }
        IEnumerator WaitDie()
        {
            yield return wait_LifeTime;
            Close();
        }
        public override void Close()
        {
            PoolManager.Instance().Recycle(gameObject);
        }
    }
}