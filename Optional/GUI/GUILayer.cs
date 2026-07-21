
using MothDIed.Debug;

namespace MothDIed.GUI
{
    public abstract class GUILayer : GUIElement
    {
        public override void MoveUnder(GUIElement guiElement)
        {
            LogHistory.PushAsError($"You tried to set parent to GUILayer. Parent -> {guiElement.name}; Layer -> {this.name}");
        }
    }
}