using System;
using Cysharp.Threading.Tasks;
using MothDIed.Debug;
using MothDIed.ServiceLocators;
using UnityEngine;

namespace MothDIed
{
    public static class Game
    {
        public static bool AllowDebug { get; private set; }

        //STATE
        public static bool Awake { get; private set; } = false;
        public static bool IsBootstrapping { get; private set; } = true;

        private static readonly GMModulesStorage debugModulesStorage = new();
        private static readonly GMModulesStorage modulesStorage = new();

        public static async UniTask StartGame(bool allowDebug, GameStartPoint point)
        {
            AllowDebug = allowDebug;

            point.BuildModules(modulesStorage, debugModulesStorage);

            string output = await modulesStorage.Boot();

            if (true)
            {
                output = $"\n Game started with from {point.GetType().ToString()}({point.name}). ";
                output += $"\n With {modulesStorage.Count} modules, including: ";

                foreach (object module in modulesStorage.all)
                {
                    output += module.ToString() + "; ";
                }

                string debugModulesOutput = await debugModulesStorage.Boot();
                output += debugModulesOutput;

                output += $"\n With {debugModulesStorage.Count} debug modules, including: ";
                foreach (object module in debugModulesStorage.all)
                {
                    output += module.ToString() + "; ";
                }
            }

            LogHistory.PushAsMessage(output);

            IsBootstrapping = false;
            Awake = true;

            point.Complete();

            InnerLoop();
        }

        public static async void QuitGame()
        {
            Awake = false;

            foreach (IGMModuleQuit quit in modulesStorage.quits)
            {
                await quit.Quit();
            }

            Application.Quit();
        }

        private static async void InnerLoop()
        {
            while (Awake)
            {
                if (G<SceneSwitcher>().CurrentScene != null && G<SceneSwitcher>().IsSceneLoaded)
                {
                    foreach (IGMModuleTick tick in modulesStorage.ticks)
                    {
                        tick.Tick();
                    }

                    foreach (Action onTick in modulesStorage.tickHooks)
                    {
                        onTick.Invoke();
                    }

                    G<SceneSwitcher>().CurrentScene?.UpdateScene();
                }

                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

                while (!Application.isPlaying)
                {
                    await UniTask.Yield();
                }
            }
        }

        public static TModule G<TModule>()
            where TModule : class
        {
            return modulesStorage.Get<TModule>();
        }

        public static TModule GDb<TModule>()
            where TModule : class
        {
            return debugModulesStorage.Get<TModule>();
        }

        public static bool TG<TModule>(out TModule module)
            where TModule : class
        {
            module = modulesStorage.Get<TModule>();
            if (AllowDebug && module == null)
            {
                module = debugModulesStorage.Get<TModule>();
            }
            return module != null;
        }

        public static IServiceLocator GetModulesLocator()
        {
            return modulesStorage;
        }
    }

    public interface IGMModuleBoot
    {
        public UniTask Boot();
    }

    public interface IGMModuleTick
    {
        public void Tick();
    }

    public interface IGMModuleQuit
    {
        public UniTask Quit();
    }
}