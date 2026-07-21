using MothDIed.Scenes;

namespace banging_code
{
    public class CommonScene : Scene
    {
        private string sceneName;

        public CommonScene(string sceneName)
        {
            this.sceneName = sceneName;
        }

        public override string GetSceneName()
        {
            return sceneName;
        }

        public override void UpdateScene()
        {
            base.UpdateScene();
            Modules.UpdateModules();
        }
    }
}