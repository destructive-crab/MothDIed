using MothDIed.Scenes.SceneModules;

namespace MothDIed.DI
{
    public class SceneDependenciesModule : SceneModule
    {
        public IDependenciesProvider[] GetDependencies { get; private set; }

        public SceneDependenciesModule(params IDependenciesProvider[] getDependencies)
        {
            GetDependencies = getDependencies;
        }
    }
}