using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MothDIed.Scenes
{
    public interface IFabric
    {
        UniTask<TObject> InstantiateAsync<TObject>(TObject original, Action<TObject> callback = null)
            where TObject : Object;
        
        UniTask<TObject> InstantiateAsync<TObject>(TObject original, Transform parent, Action<TObject> callback = null)
            where TObject : Object;
        
        UniTask<TObject> InstantiateAsync<TObject>(TObject original, Vector3 position, Action<TObject> callback = null)
            where TObject : Object;

        UniTask<TObject> InstantiateAsync<TObject>(TObject original, Vector3 position, Transform parent, Action<TObject> callback = null)
            where TObject : Object;

        UniTask<TObject> InstantiateAsync<TObject>(TObject original, Vector3 position, Quaternion rotation, Action<TObject> callback = null)
            where TObject : Object;

        UniTask<TObject> InstantiateAsync<TObject>(TObject original, Vector3 position, Quaternion rotation,
            Transform parent, Action<TObject> callback = null)
            where TObject : Object;
        
        TObject Instantiate<TObject>(TObject original)
            where TObject : Object;
        
        TObject Instantiate<TObject>(TObject original, Vector3 position)
            where TObject : Object;
        
        TObject Instantiate<TObject>(TObject original, Transform parent)
            where TObject : Object;

        TObject Instantiate<TObject>(TObject original, Vector3 position, Transform parent)
            where TObject : Object;

        TObject Instantiate<TObject>(TObject original, Vector3 position, Quaternion rotation)
            where TObject : Object;

        TObject Instantiate<TObject>(TObject original, Vector3 position, Quaternion rotation,
            Transform parent)
            where TObject : Object;

        void Destroy(Object toDestroy);
    }
}