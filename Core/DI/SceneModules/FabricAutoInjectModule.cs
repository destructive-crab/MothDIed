using MothDIed.Core.GameObjects;

namespace MothDIed.DI
{
    public sealed class FabricAutoInjectModule : GameFabricSceneModule
    {
        public override void OnInstantiated<TObject>(TObject instance)
        {
            Game.G<DIKernel>().InjectWithBase(instance);
        }
    }
}