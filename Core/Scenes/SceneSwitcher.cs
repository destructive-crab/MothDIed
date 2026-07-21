using System;
using Cysharp.Threading.Tasks;
using MothDIed.DI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scene = MothDIed.Scenes.Scene;

namespace MothDIed
{
    public class SceneSwitcher : IGMModuleBoot
    {
        public bool IsSceneLoaded { get; private set; }
        public bool IsPersistentSceneLoaded { get; private set; }
        
        public Scene CurrentScene { get; private set; }
        
        public event Action<Scene> OnSwitchingFromCurrent; //prev
        public event Action<Scene, Scene> OnSwitched; //prev/new
        public static event Action<Scene> OnLoaded; //new

        private static SceneSwitcher sceneSwitcher;

        private static UnityEngine.SceneManagement.Scene persistentScene;

        public SceneSwitcher()
        {
            if (sceneSwitcher == null)
            {
                sceneSwitcher = this;
            }
            else
            {
                throw new Exception("MULTIPLE SCENE SWITCHER INSTANCES ARE NOT ALLOWED");
            }
        }

        public async UniTask Boot()
        {
            await CreatePersistentScene();
        }

        public async UniTask CreatePersistentScene()
        {
            await SceneManager.LoadSceneAsync("PERSISTENT SCENE", LoadSceneMode.Additive);
            persistentScene = SceneManager.GetSceneByName("PERSISTENT SCENE");
            IsPersistentSceneLoaded = true;
        }

        public async UniTaskVoid SwitchTo<TScene>(TScene scene, Action onSwitched = null)
            where TScene : Scene
        {
            OnSwitchingFromCurrent?.Invoke(CurrentScene);
            IsSceneLoaded = false;
            
            //clearing previous scene if exists
            if (CurrentScene != null)
            {
                CurrentScene.DisposeScene();
                Game.G<DIKernel>().ClearSceneDependenciesContainer();

                await SceneManager.UnloadSceneAsync(CurrentScene.GetSceneName());
            }

            if (SceneManager.sceneCount > 1)
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var loadedScene = SceneManager.GetSceneAt(i);
                    
                    if (loadedScene.name != "PERSISTENT SCENE")
                    {
#if UNITY_EDITOR
#endif
                        await SceneManager.UnloadSceneAsync(loadedScene.name);
                    }
                }
            }
            
            //starting loading new scene
            Scene prevScene = CurrentScene;
            CurrentScene = scene;
            
            scene.InitModules();
            scene.PrepareScene();

            Game.G<DIKernel>().RegisterSceneDependencies(scene);
            
            LoadScene(scene.GetSceneName(), Complete);
            
            return;

            void Complete()
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene.GetSceneName()));
                scene.LoadScene();
    
                IsSceneLoaded = true;
                
                onSwitched?.Invoke();
                OnSwitched?.Invoke(prevScene, scene);
                OnLoaded?.Invoke(scene);
            }
        }

        public void LoadScene(string sceneName, Action onSceneLoadedCallback = null)
        {
            if (GetCurrentSceneName() == sceneName)
            {
                onSceneLoadedCallback?.Invoke();
                return;
            }

            var loadSceneOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            loadSceneOperation.completed += (_) => onSceneLoadedCallback?.Invoke();
        }


        private static string GetCurrentSceneName() => SceneManager.GetActiveScene().name;

        public bool MoveToPersistentScene(GameObject gameObject)
        {
            if (IsPersistentSceneLoaded)
            {
                SceneManager.MoveGameObjectToScene(gameObject, persistentScene);
                return true;
            }

            return false;
        }

        public void MoveFromPersistentScene(GameObject gameObject)
        {
            gameObject.transform.parent = null;
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
        }
    }
}