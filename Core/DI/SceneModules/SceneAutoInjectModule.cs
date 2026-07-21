using MothDIed.Scenes;
using MothDIed.Scenes.SceneModules;
using UnityEngine;

namespace MothDIed.DI
{
    public class SceneAutoInjectModule : SceneModule
    {
        public override void StartModule(Scene scene)
        {
            base.StartModule(scene);
            
            var all = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var monoBehaviour in all)
            {
                Game.G<DIKernel>().InjectWithBaseAnd(monoBehaviour, Game.G<SceneSwitcher>().CurrentScene.Modules.IServiceLocator());
            }
        }
    }
}