namespace MothDIed.Scenes.SceneModules
{
    public abstract class SceneModule
    {
        public bool Stopped => !Active;
        public bool Active { get; private set; }

        //before OnSceneLoaded invocation
        public virtual void PrepareModule(Scene scene) { }
        
        //after OnSceneLoaded invocation
        public virtual void StartModule(Scene scene) => Active = true;

        public virtual void StopModule(Scene scene) => Active = false;
        
        public virtual void UpdateModule(Scene scene) { }
        
        public virtual void Dispose(Scene scene) { }
    }
}