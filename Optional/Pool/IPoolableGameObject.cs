using UnityEngine;

namespace MothDIed.Pool
{
    public interface IPoolableGameObject<TPoolable> : IPoolable
        where TPoolable : Component
    {
        GameObjectPool<TPoolable> Pool { get;  }

        void OnPopulated(GameObjectPool<TPoolable> pool);
    }
}