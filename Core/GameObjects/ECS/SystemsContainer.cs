using System;
using System.Collections.Generic;
using MothDIed.Debug;
using MothDIed.DI;
using MothDIed.ServiceLocators;

namespace MothDIed.MonoSystems
{
    public sealed class SystemsContainer : IServiceLocator
    {
        private MonoEntity owner;

        public int Count => systems.Count;

        private readonly Dictionary<Type, List<MonoSystem>> systems = new();

        private bool containerStarted = false;

        public void SetOwner(MonoEntity owner)
        {
            if(owner == null) return;

            this.owner = owner;
        }
        
        public void StartContainer()
        {
            foreach (var systemPair in systems)
            {
                systemPair.Value.ForEach((system) =>
                {
                    Game.G<DIKernel>().InjectWithBaseAnd(system, owner.CachedComponents, this, owner.Data);
                    system.ContainerStarted();
                    if(system.EnableOnStart())
                    {
                        system.Enable();
                    }
                    else
                    {
                        system.Disable();
                    }

                });
            }
            containerStarted = true;
        }

        ~SystemsContainer()
        {
            foreach (var extension in systems)
            {
                extension.Value.ForEach(system => system.Dispose());
            }
        }

        #region Checks

        public TExtension Get<TExtension>()
            where TExtension : MonoSystem
        {
            return GetBlind(typeof(TExtension)) as TExtension;
        }

        public TSystem[] GetAllOf<TSystem>()
        {
            var result = new List<TSystem>();

            foreach (var systemPair in systems)
            {
                foreach (var system in systemPair.Value)
                {
                    if(system is TSystem target)
                        result.Add(target);
                }
            }


            return result.ToArray();
        }

        public MonoSystem[] GetAllSystemsOf<TSystem>()
        {
            var result = new List<MonoSystem>();

            foreach (var systemPair in systems)
            {
                foreach (var system in systemPair.Value)
                {
                    if(system is TSystem)
                        result.Add(system);
                }
            }

            return result.ToArray();
        }
        
        public bool Contains<TExtension>() where TExtension : MonoSystem
        {
            return Contains(typeof(TExtension));
        }

        public bool Contains(Type serviceType)
        {
            return systems.ContainsKey(serviceType);
        }

        public object GetBlind(Type extensionType)
        {
            if (!systems.ContainsKey(extensionType))
            {
                return null;
            }
            
            return systems[extensionType][0];
        }
        
        #endregion

        #region Extensions Managment

        public TExtension AddSystem<TExtension>(TExtension system)
            where TExtension : MonoSystem
        {
            Type extensionType = typeof(TExtension);
            if (Attribute.GetCustomAttribute(extensionType, typeof(DisallowMultipleSystemsAttribute)) != null)
            {
                if (systems.ContainsKey(extensionType))
                {
#if UNITY_EDITOR
                    LogHistory.PushAsError("YOU TRIED TO ADD MULTIPLE EXTENSIONS OF TYPE " + extensionType);
#endif
                    return system;
                }
            }
            
            systems.TryAdd(extensionType, new List<MonoSystem>());
            systems[extensionType].Add(system);

            system.Initialize(owner);
            
            if (containerStarted)
            {
                Game.G<DIKernel>().InjectWithBaseAnd(system, owner.CachedComponents, this, owner.Data);
                
                system.ContainerStarted();
                if(system.EnableOnStart())
                {
                    system.Enable();
                }
                else
                {
                    system.Disable();
                }
            }

            return system;
        }

        public void RemoveSystem<TExtension>()
            where TExtension : MonoSystem
        {
            Type extension = typeof(TExtension);

            if (systems.ContainsKey(extension))
            {
                systems[extension][0].Dispose();
                systems[extension].RemoveAt(0);

                if (systems[extension].Count == 0)
                    systems.Remove(extension);
            }
        }

        public void RemoveAllSystems<TExtension>()
            where TExtension : MonoSystem
        {
            while (systems.ContainsKey(typeof(MonoSystem)))
            {
                RemoveSystem<TExtension>();
            }
        }
        
        public void RemoveSystems<TExtension>(int count)
            where TExtension : MonoSystem
        {
            for(int i = 0; i < count; i++)
            {
                RemoveSystem<TExtension>();
            }
        }

        #endregion

        #region Event Methods For Extensions

        public void EnableAll()
        {
            foreach (var extensionPair in systems)
            {
                extensionPair.Value.ForEach(extension => extension.Enable());
            }
        }
        public void DisableAll()
        {
            foreach (var extensionPair in systems)
            {
                extensionPair.Value.ForEach(extension => extension.Disable());
            }
        }
        public void UpdateContainer()
        {
            foreach (var extensionPair in systems)
            {
                extensionPair.Value.ForEach(extension =>
                {
                    if (extension.Enabled) extension.Update();
                });
            }
        }
        public void FixedUpdateContainer()
        {
            foreach (var extensionPair in systems)
            {
                extensionPair.Value.ForEach(extension =>
                {
                    if (extension.Enabled) extension.FixedUpdate();
                });
            }
        }

        #endregion

        public void Dispose()
        {
            foreach (var extension in systems)
            {
                extension.Value.ForEach(system => system.Dispose());
            }
        }
    }
}