using Cysharp.Threading.Tasks;
using MothDIed.Scenes;
using MothDIed.Scenes.SceneModules;
using UnityEngine;

namespace MothDIed.Core.GameObjects
{
    public class GameFabricSceneModule : SceneModule
    {
        public override void PrepareModule(Scene scene)
            => scene.Fabric.RefreshModules();

        public virtual void OnInstantiated<TObject>(TObject instance) where TObject : Object { }

        public virtual UniTask OnInstantiatedAsync<TObject>(TObject instance) where TObject : Object { return UniTask.CompletedTask; }

        public virtual void OnGameObjectInstantiated(GameObject instance) { }
        public virtual UniTask OnGameObjectInstantiatedAsync(GameObject instance) { return UniTask.CompletedTask; }

        public virtual void OnDestroyed(Object toDestroy) { }
        public virtual void BeforeGameObjectDestroyed(GameObject instance) { }
    }
}
