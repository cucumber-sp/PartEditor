using System;
using ModLoader;
using ModLoader.Helpers;
using SFS.Builds;
using UnityEngine;

namespace PartEditor
{
    public class Main : Mod
    {
        public Main() : base("parteditor", "Part Editor", "CucumberSpace", "0.5.7", "v1.0", "Edit all part stats!")
        {
        }

        public override void Load()
        {
            SceneHelper.OnSceneLoaded += (x) =>
            {
                GUI.window = null;
                if (x.name == "Build_PC")
                    new GameObject().AddComponent<Module_>();
                else
                    GUI.DestroyGUI();
            };
        }

        public override void Unload()
        {
        }
    }

    public class Module_ : MonoBehaviour
    {
        private void Update()
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