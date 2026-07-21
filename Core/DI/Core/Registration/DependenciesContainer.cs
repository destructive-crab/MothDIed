using System;
using System.Collections.Generic;
using MothDIed.Debug;
using MothDIed.ServiceLocators;

namespace MothDIed.DI
{
    public sealed class DependenciesContainer : IDisposable, IServiceLocator
    {
        private readonly Dictionary<Type, Dependency> dependencies = new ();
        public int Count => dependencies.Count;

        public void Dispose()
        {
            foreach (var dependencyPair in dependencies)
            {
                dependencyPair.Value.Dispose();
            }
        }

        public bool Contains<TDependency>() where TDependency : class
        {
            return Contains(typeof(TDependency));
        }

        public bool Contains(Type dependencyType)
        {
            return dependencies.ContainsKey(dependencyType);
        }

        public object GetBlind(Type extensionType)
        {
            if (!Contains(extensionType))
                return null;
            
            return GetDependency(extensionType).GetInstance();
        }

        public Dependency GetDependency(Type type)
        {
            if (dependencies.TryGetValue(type, out var dependency))
            {
                return dependency;
            }

            return null;
        }

        public void Add(Type dependencyType, Dependency dependency)
        {
            if (!dependencies.TryAdd(dependencyType, dependency))
            {
                LogHistory.PushAsError($"{dependencyType} DEPENDENCY ALREADY REGISTERED");
            }
        }

        public void Remove<TDependency>()
        {
            if (dependencies.ContainsKey(typeof(TDependency)))
                dependencies.Remove(typeof(TDependency));
        }

        public void SetupSingletons()
        {
            foreach (var dependencyPair in dependencies)
            {
                if (dependencyPair.Value.IsSingleton)
                {
                    Game.G<DIKernel>().InjectWithBase(dependencyPair.Value.GetInstance());
                }
            }
        }
    }
}