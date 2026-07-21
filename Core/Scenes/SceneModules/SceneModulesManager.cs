using MothDIed.ServiceLocators;

namespace MothDIed.Scenes.SceneModules
{
    public sealed class SceneModulesManager
    {
        //TODO: ADD INJECTIONS
        
        private readonly Scene scene;
        private readonly ServiceLocator<SceneModule> modules = new();
        public int Count => modules.Count;

        public IServiceLocator IServiceLocator() => modules as IServiceLocator;
        
        public SceneModulesManager(Scene scene)
        {
            this.scene = scene;
        }
        
        public void PrepareAllModules()
        {
            foreach (var module in modules.GetAll())
            {
                module.PrepareModule(scene);
            }
        }
 
        public void StartAllModules()
        {
            foreach (var module in modules.GetAll())
            {
                module.StartModule(scene);
            }
        }
        
        public void StopModule<TModule>()
            where TModule : SceneModule
        {
            if (modules.TryGet<TModule>(out var module) && module.Active)
            {
                module.StopModule(scene);
            }
        }

        public void StartModule<TModule>()
            where TModule : SceneModule, new()
        {
            if (modules.TryGet<TModule>(out TModule module) && module.Stopped)
            {
                module.StartModule(scene);
            }
            else if (module == null)
            {
                AddModule(new TModule()).StartModule(scene);
            }
        }

        public bool Contains<TModule>()
            where TModule : SceneModule
        {
            return modules.Contains<TModule>();
        }

        public TModule Get<TModule>()
            where TModule : SceneModule
        {
            return modules.Get<TModule>();
        }

        public bool TryGetModule<TModule>(out TModule module)
            where TModule : SceneModule
        {
            return modules.TryGet(out module);
        }

        public T[] GetAllOfType<T>()
            where T : SceneModule
        {
            return modules.GetAllOfType<T>();
        }

        public void UpdateModules()
        {
            foreach (var module in modules.GetAll())
            {
                if(module.Stopped) continue;
                module.UpdateModule(scene);
            }
        }

        public TModule AddModule<TModule>(TModule module)
            where TModule : SceneModule
        {
            modules.Register(module);

            return module;
        }

        public void RemoveModule<TModule>()
            where TModule : SceneModule
        {
            if (modules.Unregister<TModule>(out TModule module))
            {   
                module.Dispose(scene);
            }
        }
    }
}