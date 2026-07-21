using System;

namespace MothDIed.MonoSystems
{
    public abstract class MonoSystem : IDisposable
    {
        public MonoEntity Owner { get; private set; }
        public bool Enabled { get; private set; }

        public abstract bool EnableOnStart();
        
        public void Initialize(MonoEntity owner)
        {
            Owner = owner;
        }

        public virtual void ContainerStarted() { }
        public virtual void Dispose() { }

        public virtual void Enable()
        {
            Enabled = true;
        }

        public virtual void Disable()
        {
            Enabled = false;
        }
        
        public virtual void Update() {}
        public virtual void FixedUpdate() {}
        
    }
}