// **************************************************************** //
//
//   Copyright (c) RimuruDev. All rights reserved.
//   Contact me: 
//          - Gmail:    rimuru.dev@gmail.com
//          - LinkedIn: https://www.linkedin.com/in/rimuru/
//          - GitHub:   https://github.com/RimuruDev
//
// **************************************************************** //
//
//   Copyright (c) YUJECK. All rights reserved.
//   Contact me: 
//          - GitHub:   https://github.com/YUJECK
//
// **************************************************************** //

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scene = MothDIed.Scenes.Scene;

namespace MothDIed.DI
{
    public sealed class IndieInjector
    {
        private const BindingFlags BindingFlags =
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic;

        private readonly DependenciesContainer coreContainer = new();
        private DependenciesContainer sceneContainer;

        public IndieInjector()
        {
            SceneManager.sceneUnloaded += ClearSceneDependenciesContainer;
        }

        private void ClearSceneDependenciesContainer(UnityEngine.SceneManagement.Scene _)
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
                    Debug.LogWarning($"It's not recommended to store instances of GameObject or MonoBehaviour with DI({method.ReturnType} in {provider})." +
                                     $" If you want to store prefabs, use special containers");
                }
#endif
            }
            
            container.SetupSingletons();
        }

        #endregion

        #region Inject

        public void Inject(object toInject)
        {
            var type = toInject.GetType();

            //if it is gameobject so we need to inject in all his components not in it
            if (TryInjectInGameObject(toInject)) 
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
                            parameters.Add(Find(parameter.ParameterType).GetInstance());
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
                        
                        info.SetValue(toInject, Find(info.FieldType).GetInstance());
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

                        info.SetValue(toInject, Find(info.PropertyType).GetInstance());
                    }
                }
            }
        }

        private bool TryInjectInGameObject(object toInject)
        {
            if (toInject is GameObject gameObject)
            {
                var allMonoBehaviours = gameObject.GetComponentsInChildren<MonoBehaviour>(true);

                foreach (var monoBehaviour in allMonoBehaviours)
                {
                    Inject(monoBehaviour);
                }

                return true;
            }

            return false;
        }

        private Dependency Find(Type type)
        {
            if (coreContainer.Get(type) != null)
            {
                return coreContainer.Get(type);
            }
            if (sceneContainer.Get(type) != null)
            {
                return sceneContainer.Get(type);
            }

            throw new IndieResolveException($"There is no dependency of type {type}");
        }

        #endregion
    }
}