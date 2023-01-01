using System.Collections.Generic;
using System.Linq;
using ModLoader;
using ModLoader.Helpers;
using SFS.Builds;
using SFS.IO;
using UITools;

namespace PartEditor
{
    public class Main : Mod, IUpdatable
    {
        public static Main main;
        public Main() => main = this;
        
        public override void Load()
        {
            Config.Setup();
            ConfigGUI.Setup();
            SceneHelper.OnBuildSceneLoaded += () =>
            {
                GUI.Setup();
                BuildManager.main.selector.onSelectedChange += () => GUI.CurrentPart.Value = BuildManager.main.selector.selected.Count == 1 ? BuildManager.main.selector.selected.First() : null;
            };
        }

        public override string ModNameID => "parteditor";
        public override string DisplayName => "Part Editor";
        public override string Author => "CucumberSpace";
        public override string MinimumGameVersionNecessary => "1.5.9.6";
        public override string ModVersion => "1.0";
        public override string Description => "Edit all part stats!";
        public override Dictionary<string, string> Dependencies { get; } = new () { {"UITools", "1.0"} };
        public new FolderPath ModFolder => new (base.ModFolder);
        
        public Dictionary<string, FilePath> UpdatableFiles => new () {{"https://github.com/cucumber-sp/PartEditor/releases/latest/download/PartEditor.dll", new FolderPath(ModFolder).ExtendToFile("PartEditor.dll")}};
    }
}