using System;
using ModLoader;
using ModLoader.Helpers;
using SFS.Builds;
using UnityEngine;

namespace PartEditor
{
    public class Main : Mod
    {
        public override void Load()
        {
            SceneHelper.OnSceneLoaded += (x) =>
            {
                if (x.name == "Build_PC")
                    new GameObject().AddComponent<Module_>();
            };
        }

        public override string ModNameID => "parteditor";
        public override string DisplayName => "Part Editor";
        public override string Author => "CucumberSpace";
        public override string MinimumGameVersionNecessary => "1.5.7";
        public override string ModVersion => "1.0";
        public override string Description => "Edit all part stats!";
    }

    public class Module_ : MonoBehaviour
    {
        void Start()
        {
            GUI.forced = true;
        }

        void Update()
        {
            try
            {
                GUI.UpdateGUI(BuildManager.main.selector.selected.Count > 0 ?BuildManager.main.selector.selected[0] : null);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            
        }
    }
}