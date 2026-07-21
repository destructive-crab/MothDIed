using System;
using Cysharp.Threading.Tasks;
using MothDIed.Core.GameObjects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MothDIed.Scenes
{
    public class GameFabric : IFabric
    {
        public event Action<Object> OnInstantiated;
        public event Action<Object> OnDestroyed;

        public event Action<GameObject> OnGameObjectInstantiated;
        public event Action<GameObject> OnGameObjectDestroyed;

        private GameFabricSceneModule[] modules;

        public void RefreshModules()
        {
            modules = Game.G<SceneSwitcher>().CurrentScene.Modules.GetAllOfType<GameFabricSceneModule>();
        }

        public UniTask<TObject> InstantiateAsync<TObject>(TObject original, Action<TObject> callback = null)
            where TObject : Object
        {
            return InstantiateAsync(original, Vector3.up, Quaternion.identity, null, callback);
        }

        public UniTask<TObject> InstantiateAsync<TObject>(TObject original, Transform parent, Action<TObject> callback = null)
            where TObject : Object
        {
            return InstantiateAsync(original, Vector2.zero, Quaternion.identity, parent, callback);
        }

        public UniTask<TObject> InstantiateAsync<TObject>(TObject original, Vector3 position, Action<TObject> callback = null)
            where TObject : Object
        {
            return InstantiateAsync(original, position, Quaternion.identity, null, callback);
        }

        public UniTask<TObject> InstantiateAsync<TObject>(TObject original, Vector3 position, Transform parent, Action<TObject> callback = null)
            where TObject : Object
        {
            return InstantiateAsync(original, position, Quaternion.identity, parent, callback);
        }

        public UniTask<TObject> InstantiateAsync<TObject>(TObject original, Vector3 position, Quaternion rotation, Action<TObject> callback = null)
            where TObject : Object
        {
            return InstantiateAsync(original, position, rotation, null, callback);
        }

        public async UniTask<TObject> InstantiateAsync<TObject>(TObject original, Vector3 position, Quaternion rotation, Transform parent, Action<TObject> callback = null)
            where TObject : Object
        {
            AsyncInstantiateOperation<TObject> instantiateOperation = Object.InstantiateAsync<TObject>(original, parent, position, rotation);

            await instantiateOperation;

            TObject instance = instantiateOperation.Result[0];

            foreach (var module in modules)
            {
                await module.OnInstantiatedAsync(instance);
            }

            OnInstantiated?.Invoke(instance);

            await InvokeGameObjectEventsIfGameObject();

            callback?.Invoke(instance);
            return instance;

            async UniTask InvokeGameObjectEventsIfGameObject()
            {
                if (instance is GameObject gameObject)
                {
                    OnGameObjectInstantiated?.Invoke(gameObject);
                    foreach (var module in modules)
                    {
                        await module.OnGameObjectInstantiatedAsync(gameObject);
                    }
                }
                else if (instance is Component component)
                {
                    OnGameObjectInstantiated?.Invoke(component.gameObject);
                    foreach (var module in modules)
                    {
                        await module.OnGameObjectInstantiatedAsync(component.gameObject);
                    }
                }
            }
        }

        public TObject Instantiate<TObject>(TObject original) where TObject : Object
        {
            return Instantiate(original, Vector3.zero, Quaternion.identity, null);
        }

        public virtual TObject Instantiate<TObject>(TObject original, Vector3 position)
            where TObject : Object
        {
            return Instantiate(original, position, Quaternion.identity, null);
        }

        public TObject Instantiate<TObject>(TObject original, Transform parent)
            where TObject : Object
        {
            return Instantiate(original, Vector3.zero, Quaternion.identity, parent);
        }

        public virtual TObject Instantiate<TObject>(TObject original, Vector3 position, Transform parent)
            where TObject : Object
        {
            return Instantiate(original, position, Quaternion.identity, parent);
        }

        public virtual TObject Instantiate<TObject>(TObject original, Vector3 position, Quaternion rotation)
            where TObject : Object
        {
            return Instantiate(original, position, rotation, null);
        }

        public virtual TObject Instantiate<TObject>(TObject original, Vector3 position, Quaternion rotation,
            Transform parent)
            where TObject : Object
        {
            var instance = Object.Instantiate(original, position, rotation, parent);

            foreach (var module in modules)
            {
                module.OnInstantiated(instance);
            }

            OnInstantiated?.Invoke(instance);
            InvokeGameObjectEventsIfGameObject();

            return instance;

            void InvokeGameObjectEventsIfGameObject()
            {
                if (instance is GameObject gameObject)
                {
                    OnGameObjectInstantiated?.Invoke(gameObject);
                    foreach (var module in modules)
                    {
                        module.OnGameObjectInstantiated(gameObject);
                    }
                }
                else if (instance is Component component)
                {
                    OnGameObjectInstantiated?.Invoke(component.gameObject);
                    foreach (var module in modules)
                    {
                        module.OnGameObjectInstantiated(component.gameObject);
                    }
                }
            }
        }

        public virtual void Destroy(Object toDestroy)
        {
            OnDestroyed?.Invoke(toDestroy);

            if(toDestroy is GameObject gameObject)
            {
                OnGameObjectDestroyed?.Invoke(gameObject);

                foreach (var module in modules)
                {
                    module.BeforeGameObjectDestroyed(gameObject);
                }
            }

            Object.Destroy(toDestroy);
        }
    }
}
