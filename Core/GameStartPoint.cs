using System;
using Cysharp.Threading.Tasks;
using MohDIed.Audio;
using MothDIed;
using MothDIed.DI;
using UnityEngine;

public abstract class GameStartPoint : MonoBehaviour
{
    public abstract bool AllowDebug();
    public virtual IDependenciesProvider[] GetProviders()
    {
        return Array.Empty<IDependenciesProvider>();
    }

    private void Start()
    {
        StartGame();
    }

    private async void StartGame()
    {
        await Prepare();
        Game.StartGame(AllowDebug(), this);
    }

    protected virtual UniTask Prepare()
    {
        return UniTask.CompletedTask;
    }

    public virtual UniTask BuildModules(GMModulesStorage modulesStorage, GMModulesStorage debugStorage)
    {
        modulesStorage.AutoRegister<SceneSwitcher>(new SceneSwitcher());
        modulesStorage.AutoRegister<DIKernel>(new DIKernel());
        modulesStorage.AutoRegister<AudioSystem>(new AudioSystem());
        
        return UniTask.CompletedTask;
    }

    public virtual void Complete()
    {
        Game.G<DIKernel>().RegisterDependenciesToCore(this);
    }

}