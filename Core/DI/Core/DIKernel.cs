using System;
using System.Collections.Generic;
using System.Reflection;
using MothDIed.Debug;
using MothDIed.ServiceLocators;
using UnityEngine;
using Scene = MothDIed.Scenes.Scene;

namespace MothDIed.DI
{
    public sealed class DIKernel
    {
        private const BindingFlags BindingFlags =
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic;

        private readonly DependenciesContainer coreContainer = new();
        private DependenciesContainer sceneContainer;

        public void ClearSceneDependenciesContainer()
        {
            if(sceneContainer == null) return;
            
            sceneContainer?.Dispose();
            sceneContainer = null;
        }

        #region Registration
        public void RegisterDependenciesToCore(GameStartPoint startPoint)
        {
            var providers
                = startPoint.transform.GetComponentsInChildren<IDependenciesProvider>();

            Register(providers, coreContainer);
            Register(startPoint.GetProviders(), coreContainer);
        }
        
        public void RegisterSceneDependencies(Scene scene)
        {
            if(scene.Modules.TryGetModule(out SceneDependenciesModule dependenciesModule))
            {
                sceneContainer = new DependenciesContainer();
                Register(dependenciesModule.GetDependencies, sceneContainer);
            }
        }

        private void Register(IDependenciesProvider[] providers, DependenciesContainer container)
        {
            foreach (var provider in providers)
            {
                var methods = provider.GetType().GetMethods(BindingFlags);
            
                foreach (var method in methods)
                {
                    if (!Attribute.IsDefined(method, typeof(ProvideAttribute)))
                    {
                        continue;
                    }

                    RegisterDependency(method, provider);
                }
            }

            void RegisterDependency(MethodInfo method, IDependenciesProvider provider)
            {
                Type dependencyType = method.ReturnType;

                bool isSingleton = ((ProvideAttribute) Attribute.GetCustomAttribute(method, typeof(ProvideAttribute))).IsSingleton;

                Func<object> fabric
                    = (Func<object>) method.CreateDelegate(typeof(Func<object>), provider);

                var dependencyRegistration = new Dependency(dependencyType, fabric, isSingleton);

                container.Add(dependencyType, dependencyRegistration);

#if UNITY_EDITOR
                if (method.ReturnType.IsSubclassOf(typeof(Component)) || method.ReturnType == typeof(GameObject))
                {
                    LogHistory.PushAsWarning($"It's not recommended to store GameObject instances with DI({method.ReturnType} in {provider})." +
                                     $" If you want to store prefabs, use special containers");
                }
#endif
            }
            
            container.SetupSingletons();
        }

        #endregion

        #region Inject

        public void InjectWithBaseAnd(object toInject, params IServiceLocator[] serviceLocators)
        {
            List<IServiceLocator> locators = GetBaseLocators();

            locators.AddRange(serviceLocators);
 
            InjectWith(toInject, locators.ToArray());
        }

        public void InjectWithBase(object toInject)
        {
            List<IServiceLocator> locators = GetBaseLocators();

            InjectWith(toInject, locators.ToArray());
        }

        private List<IServiceLocator> GetBaseLocators()
        {
            List<IServiceLocator> locators = new List<IServiceLocator>();
            
            if(coreContainer.Count > 0) locators.Add(coreContainer);
            if(sceneContainer != null)  locators.Add(sceneContainer);
            if(Game.GetModulesLocator().Count > 0) locators.Add(Game.GetModulesLocator());
            if(Game.G<SceneSwitcher>().IsSceneLoaded && Game.G<SceneSwitcher>().CurrentScene.Modules.Count > 0)  locators.Add(Game.G<SceneSwitcher>().CurrentScene.Modules.IServiceLocator());
            return locators;
        }

        public void InjectWith(object toInject, params IServiceLocator[] serviceLocators)
        {
            Type type = toInject.GetType();

            //if it is gameobject so we need to inject in all his components not in it
            if (TryInjectInGameObject(toInject, serviceLocators)) 
                return;

            InjectRegion region = InjectRegion.All;
            
            var customAttribute = (InjectRegionAttribute)toInject.GetType().GetCustomAttribute(typeof(InjectRegionAttribute));
            
            if (customAttribute != null)
            {
                region = customAttribute.Region;
            }
            
            //methods
            if((region & InjectRegion.Methods) == InjectRegion.Methods)
            {
                var allMethods = type.GetMethods(BindingFlags);

                foreach (var info in allMethods)
                {
                    foreach (var attribute in info.GetCustomAttributes())
                    {
                        if (attribute is not InjectAttribute) continue;
                    
                        var requiredParameters = info.GetParameters();

                        List<object> parameters = new List<object>();

                        foreach (var parameter in requiredParameters)
                        {
                            parameters.Add(FindIn(parameter.ParameterType, serviceLocators));
                        }

                        info.Invoke(toInject, parameters.ToArray());
                    }
                }
            }
            
            //fields
            if((region & InjectRegion.Fields) == InjectRegion.Fields)
            {
                var allFields = type.GetFields(BindingFlags);
        
                foreach (var info in allFields)
                {
                    foreach (var attribute in info.GetCustomAttributes())
                    {
                        if (attribute is not InjectAttribute) continue;
                        
                        info.SetValue(toInject, FindIn(info.FieldType, serviceLocators));
                    }
                }
            }

            //properties
            if((region & InjectRegion.Properties) == InjectRegion.Properties)
            {
                var allProperties = type.GetProperties(BindingFlags);
        
                foreach (var info in allProperties)
                {
                    foreach (var attribute in info.GetCustomAttributes())
                    {
                        if (attribute is not InjectAttribute) continue;

                        info.SetValue(toInject, FindIn(info.PropertyType, serviceLocators));
                    }
                }
            }
        }

        private bool TryInjectInGameObject(object toInject, IServiceLocator[] locators)
        {
            if (toInject is GameObject gameObject)
            {
                var allMonoBehaviours = gameObject.GetComponentsInChildren<MonoBehaviour>(true);

                foreach (var monoBehaviour in allMonoBehaviours)
                {
                    InjectWith(monoBehaviour, locators);
                }

                return true;
            }

            return false;
        }

        private object FindIn(Type type, params IServiceLocator[] locators)
        {
            for (var i = 0; i < locators.Length; i++)
            {
                var locator = locators[i];
                
                if (locator == null) { LogHistory.PushAsError($"[DI KERNEL : FIND IN] NULL LOCATOR FOUND. INDEX {i}"); }

                var possibleDependency = locator.GetBlind(type);

                if (possibleDependency != null)
                {
                    return possibleDependency;
                }
            }

            LogHistory.PushAsError($"[DI KERNEL : FIND IN] NO DEPENDENCY OF TYPE {type.ToString()} WAS FOUND IN {locators.Length} DEPENDENCY LOCATORS");
            return null;
        }

        #endregion
    }
}