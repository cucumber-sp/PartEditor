using System;
using SFS.IO;
using SFS.UI.ModGUI;
using SFS.Variables;
using TMPro;
using UITools;
using UnityEngine;
using UnityEngine.UI;
using Type = SFS.UI.ModGUI.Type;

namespace PartEditor
{

    public class Config : ModSettings<Config.ConfigData>
    {
        static Config main;

        public static void Setup()
        {
            main = new Config();
            main.Initialize();
        }

        public class ConfigData
        {
            public Vector2Int_Local windowSize = new() { Value = new Vector2Int(350, 450) };
            public Bool_Local stretchToFit = new() { Value = false };
            public Float_Local numberChangeStep = new (){Value = 0.1f};
            public Float_Local degreeChangeStep = new (){Value = 1f};
        }

        protected override void RegisterOnVariableChange(Action onChange)
        {
            settings.windowSize.OnChange += onChange;
            settings.stretchToFit.OnChange += onChange;
            settings.numberChangeStep.OnChange += onChange;
            settings.numberChangeStep.OnChange += onChange;
            Application.quitting += onChange;
        }

        protected override FilePath SettingsFile => Main.main.ModFolder.ExtendToFile("config.txt");
    }

    public static class ConfigGUI
    {
        public static void Setup()
        {
            ConfigurationMenu.Add(null, new (string, Func<Transform, GameObject>)[] { ("Part Editor", CreateConfigGUI) });
        }

        static GameObject CreateConfigGUI(Transform parent)
        {
            Vector2Int size = ConfigurationMenu.ContentSize;

            Box box = Builder.CreateBox(parent, size.x, size.y);
            box.CreateLayoutGroup(Type.Vertical, TextAnchor.UpperCenter, padding: new RectOffset(0, 0, 5, 5));
            
            Builder.CreateLabel(box, size.x - 50, 40, text: "GUI Settings");

            Container widthContainer = Builder.CreateContainer(box);
            widthContainer.CreateLayoutGroup(Type.Horizontal, padding: new RectOffset(0, 0, 10, 10));
            Builder.CreateLabel(widthContainer, (size.x - 70) / 2, 35, text: "Window Width").TextAlignment = TextAlignmentOptions.MidlineLeft;
            Builder.CreateSlider(widthContainer, (size.x - 70) / 2, Config.settings.windowSize.Value.x / 10f, (30, 50),
                true,
                f => Config.settings.windowSize.Value = new Vector2Int((int)f * 10, Config.settings.windowSize.Value.y),
                f => ((int)(f * 10)).ToString());
            
            Container heightContainer = Builder.CreateContainer(box);
            heightContainer.CreateLayoutGroup(Type.Horizontal, padding: new RectOffset(0, 0, 5, 5));
            Builder.CreateLabel(heightContainer, (size.x - 70) / 2, 35, text: "Window Height").TextAlignment = TextAlignmentOptions.MidlineLeft;
            Builder.CreateSlider(heightContainer, (size.x - 70) / 2, Config.settings.windowSize.Value.y / 10f, (30, 80),
                true,
                f => Config.settings.windowSize.Value = new Vector2Int(Config.settings.windowSize.Value.x, (int)f * 10),
                f => ((int)(f * 10)).ToString());

            heightContainer.gameObject.SetActive(!Config.settings.stretchToFit.Value);
            Builder.CreateToggleWithLabel(box, size.x - 50, 35, () => Config.settings.stretchToFit.Value,
                () =>
                {
                    Config.settings.stretchToFit.Value ^= true;
                    heightContainer.gameObject.SetActive(!Config.settings.stretchToFit.Value);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(box.rectTransform);
                }, 0, 0, "Stretch Window In Height");

            Builder.CreateSeparator(box, size.x - 50);

            Builder.CreateLabel(box, size.x - 50, 40, text: "Editing Settings");
            GUI.CreateNumberInput(box, size.x - 50, 50, 0.5f, "Numbers Change Step", Config.settings.numberChangeStep, 0.1f, f => Config.settings.numberChangeStep.Value = f);
            GUI.CreateNumberInput(box, size.x - 50, 50, 0.5f, "Degrees Change Step", Config.settings.degreeChangeStep, 1f, f => Config.settings.degreeChangeStep.Value = f);

            return box.gameObject;
        }
    }
}