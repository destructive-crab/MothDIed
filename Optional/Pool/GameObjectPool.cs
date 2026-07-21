using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MothDIed.Debug;
using MothDIed.Scenes;
using UnityEngine;

namespace MothDIed.Pool
{
    public class GameObjectPool<TObject> : IDisposable
        where TObject : Component
    {
        public Config<TObject> PoolConfiguration{ get; private set; }
        
        private readonly List<TObject> Available = new();
        private readonly List<TObject> CurrentlyInUse = new();

        public PoolInstanceRoot Root { get; private set; }

        public bool IsPoolReady { get; private set; } = false;
        public int TotalCount => Available.Count + CurrentlyInUse.Count;
        public ushort CurrentSize { get; private set; }

        public GameObjectPool(Config<TObject> poolConfiguration)
        {
            PoolConfiguration = poolConfiguration;
            RefreshSize();
        }

        private void RefreshSize()
        {
            if(CurrentSize < PoolConfiguration.Size) CurrentSize = PoolConfiguration.Size;
        }

        public GameObjectPool(TObject prefab) : this(new Config<TObject>(prefab)) { }

        public void Dispose()
        {
            PoolConfiguration.Fabric.Destroy(Root.gameObject);
        }

        public GameObjectPool<TObject> Warm()
        {
            if (IsPoolReady) return this;
            
            RefreshSize();
            Root = new GameObject().AddComponent<PoolInstanceRoot>();
            Root.Setup(PoolConfiguration.Name);
            
            if(PoolConfiguration.Persistent)
            {
                Game.G<SceneSwitcher>().MoveToPersistentScene(Root.gameObject);
            }

            TryPopulateUntilPoolWillBeFull();
            
            IsPoolReady = true;
            return this;
        }

        
        public async UniTask<GameObjectPool<TObject>> WarmAsync()
        {
            RefreshSize();
            if (IsPoolReady) return this;
            
            Root = new GameObject().AddComponent<PoolInstanceRoot>();
            Root.Setup(PoolConfiguration.Name);
            
            if(PoolConfiguration.Persistent)
            {
                Game.G<SceneSwitcher>().MoveToPersistentScene(Root.gameObject);
            }

            await TryPopulateUntilPoolWillBeFullAsync();
            
            IsPoolReady = true;
            return this;
        }
        
        public TObject Pick()
        {
            if (IsPoolReady && Available.Count == 0 && !TryExpandPoolAndPopulate()) return null;

            TObject instance = Available[0];
            Available.RemoveAt(0);
            CurrentlyInUse.Add(instance);
            instance.gameObject.SetActive(true);

            if (PoolConfiguration.MoveFromPersistentOnPick)
            {
                Game.G<SceneSwitcher>().MoveFromPersistentScene(instance.gameObject);
            }
            
            if(instance is IPoolable poolable) poolable.OnPicked();
            
            return instance;
        }

        public bool Release(TObject instance)
        {
            if (IsPoolReady && !CurrentlyInUse.Contains(instance)) return false;
            
            if(instance is IPoolable poolable) poolable.OnReleased(); 
            
            CurrentlyInUse.Remove(instance);
            Available.Add(instance);
            instance.gameObject.SetActive(false);

            if (PoolConfiguration.Persistent && PoolConfiguration.MoveFromPersistentOnPick)
            {
                instance.gameObject.transform.parent = Root.transform;
            }
            
            return true;
        }

        public void ReleaseAll()
        {
            foreach (var instance in CurrentlyInUse)
            {
                Release(instance);
            }
        }

        private bool TryExpandPoolAndPopulate() => TryExpandPool() && TryPopulateUntilPoolWillBeFull();

        private bool TryPopulateUntilPoolWillBeFull()
        {
            if (!IsConfigurationValid()) return false;

            var instantiateFrom = GetOriginToPopulateFrom();

            while (TotalCount < CurrentSize)
            {
                TObject newInstance = PoolConfiguration.Fabric.Instantiate(instantiateFrom, Vector3.zero,  Root.transform);
                
                ProcessPopulatedGameObject(newInstance);
            }

            return true;
        }

        private async UniTask<bool> TryPopulateUntilPoolWillBeFullAsync()
        {
            if (!IsConfigurationValid()) return false;

            TObject instantiateFrom = GetOriginToPopulateFrom();

            while (TotalCount < CurrentSize)
            {
                TObject newInstance = await PoolConfiguration.Fabric.InstantiateAsync(instantiateFrom, Vector3.zero,  Root.transform);

                ProcessPopulatedGameObject(newInstance);
            }

            return true;
        }

        private bool TryExpandPool()
        {
            if (!PoolConfiguration.Expandable) return false;

            CurrentSize += PoolConfiguration.Size;
            return true;
        }

        private TObject GetOriginToPopulateFrom()
        {
            TObject instantiateFrom = PoolConfiguration.Prefab;

            if (instantiateFrom == null)
            {
                instantiateFrom = new GameObject(PoolConfiguration.Name + " Instance").AddComponent<TObject>();
                
                instantiateFrom.transform.parent = Root.transform;
                instantiateFrom.gameObject.SetActive(false);
                Available.Add(instantiateFrom);
            }

            return instantiateFrom;
        }

        private void ProcessPopulatedGameObject(TObject newInstance)
        {
            newInstance.gameObject.SetActive(false);
            Available.Add(newInstance);

            if (newInstance is IPoolableGameObject<TObject> poolableGameObject)
            {
                poolableGameObject.OnPopulated(this);
            }
        }

        public bool IsConfigurationValid()
        {
            RefreshSize();
            
#if UNITY_EDITOR
            if (PoolConfiguration.Fabric == null)
            {
                LogHistory.PushAsError($"[GAME OBJECT POOL] NULL FABRIC WAS FOUND IN {PoolConfiguration.Name}");
                return false;
            }

            if (PoolConfiguration.Size == 0)
            {
                LogHistory.PushAsError($"[GAME OBJECT POOL] POOL SIZE CANNOT BE 0; {PoolConfiguration.Name}");
                return false;               
            }
#endif

            return PoolConfiguration.Fabric != null;
        }


        [Serializable]
        public sealed class Config<ConfigTObject>
            where ConfigTObject : Component
        {
            /// <summary>
            /// - Will be applied to root instance
            /// </summary>
            public string Name; 

            /// <summary>
            /// - If set as null, pool will be filled with empty game object with ConfigTObject component
            /// </summary>
            public ConfigTObject Prefab; 

            [field: SerializeField] public ushort Size { get; private set; } 

            /// <summary>
            /// - If true, when pool is full it will populate another count of SIZE objects
            /// </summary>
            public bool Expandable; 

            /// <summary>
            /// - If true, pool will be moved to persistent scene
            /// </summary>
            public bool Persistent;

            /// <summary>
            /// - If true, when you pick object from pool, it will be moved to current scene.
            /// On Release it will be moved back on persistent scene
            /// </summary>
            public bool MoveFromPersistentOnPick = true;

            /// <summary>
            /// - Custom fabric can be used in pool. By default pool will use PoolFabric
            /// </summary>
            public IFabric Fabric { get; private set; }
            
            public Config(ConfigTObject prefab, string name = "", ushort size = 32, bool expandable = true, bool persistent = false, IFabric fabric = null)
            {
                SetName(name);
                SetSize(size);
                IsExpandable(expandable);
                IsPersistent(persistent);
                SetPrefab(prefab);
                SetFabric(fabric);
            }
            public Config(string name)
            {
                SetName(name);
            }

            public Config<ConfigTObject> SetFabric(IFabric fabric)
            {
                Fabric = fabric;
                
                if (fabric == null)
                {
                    Fabric = new PoolFabric(false, true);
                }

                return this;
            }

            public Config<ConfigTObject> SetPrefab(ConfigTObject prefab)
            {
                Prefab = prefab;
                return this;
            }

            public Config<ConfigTObject> IsPersistent(bool persistent)
            {
                Persistent = persistent;
                return this;
            }

            public Config<ConfigTObject> IsExpandable(bool expandable)
            {
                Expandable = expandable;
                return this;
            }

            public Config<ConfigTObject> SetName(string name)
            {
                Name = name;
                
                switch (Name)
                {
                    case "" when Prefab != null: Name = $"{Prefab.name} Pool"; break; 
                    case "" when Prefab == null: Name = $"{nameof(ConfigTObject)} Pool"; break;
                }

                return this;
            }

            public Config<ConfigTObject> SetSize(ushort size)
            {
                if (size < 8) size = 8;

                Size = size;
                return this;
            }
        }
    }
}