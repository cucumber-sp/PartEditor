using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.UI.ModGUI;
using TMPro;
using UITools;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Type = SFS.UI.ModGUI.Type;

namespace PartEditor
{
    public static class GUI
    {
        public static readonly Part_Local CurrentPart = new ();

        static GameObject holder;
        static Window window;

        static bool setup;
        public static void Setup()
        {
            if (!setup)
            {
                CurrentPart.OnChange += RegenerateWindow;
                Config.settings.windowSize.OnChange += () => RegenerateWindow(CurrentPart.Value);
                Config.settings.stretchToFit.OnChange += () => RegenerateWindow(CurrentPart.Value);
            }
            else 
                RegenerateWindow(null);
            setup = true;
        }

        static async void RegenerateWindow(Part part)
        {
            Vector2Int size = Config.settings.windowSize.Value;
            
            if (holder == null)
            {
                holder = Builder.CreateHolder(Builder.SceneToAttach.CurrentScene, "PartEditor Holder");
                holder.transform.localScale = new Vector3(0.9f, 0.9f);
            }

            Vector2 canvasResolution = UIUtility.CanvasPixelSize;
            if (window == null || window.gameObject == null)
            {
                window = UIToolsBuilder.CreateClosableWindow(holder.transform, 0, size.x, size.y, (int)canvasResolution.x / 2 - 100,
                    (int)canvasResolution.y / 2 - 50, true, false, 1, "Part Editor");
                window.CreateLayoutGroup(Type.Vertical);
                window.EnableScrolling(Type.Vertical);
            }

            // Destroying window content
            for (int i = 0; i < window.ChildrenHolder.childCount; i++)
                Object.Destroy(window.ChildrenHolder.GetChild(i).gameObject);
            
            window.RegisterPermanentSaving("PartEditor.MainWindow");

            if (part == null)
            {
                window.Size = new Vector2(size.x, 100);
                Builder.CreateLabel(window, size.x - 50, 30, text: "Select a part").Opacity = 0.8f;
                return;
            }

            window.Size = size;
            
            // Position
            Box positionBox = CreateContentBox(window, size.x - 30, "Position");
            CreateNumberInput(positionBox, size.x - 50, 50, 0.8f, "X", part.Position.x, Config.settings.numberChangeStep, ApplyPositionX);
            CreateNumberInput(positionBox, size.x - 50, 50, 0.8f, "Y", part.Position.y, Config.settings.numberChangeStep, ApplyPositionY);
            
            // Orientation
            Box orientationBox = CreateContentBox(window, size.x - 30, "Orientation");
            CreateNumberInput(orientationBox, size.x - 50, 50, 0.8f, "X", part.orientation.orientation.Value.x, Config.settings.numberChangeStep, ApplyOrientationX);
            CreateNumberInput(orientationBox, size.x - 50, 50, 0.8f, "Y", part.orientation.orientation.Value.y, Config.settings.numberChangeStep, ApplyOrientationY);
            CreateNumberInput(orientationBox, size.x - 50, 50, 0.8f, "Z", part.orientation.orientation.Value.z, -Config.settings.degreeChangeStep, ApplyOrientationZ);

            if (part.variablesModule.doubleVariables.GetSaveDictionary().Count > 0)
            {
                Box doublesBox = CreateContentBox(window, size.x - 30, "Double Variables");
                foreach (KeyValuePair<string, double> save in part.variablesModule.doubleVariables.GetSaveDictionary())
                    CreateNumberInput(doublesBox, size.x - 50, 50, 0.55f, save.Key, (float) save.Value, Config.settings.numberChangeStep, f => ApplyDoubleVariable(save.Key, f), true, 18);
            }
            
            if (part.variablesModule.stringVariables.GetSaveDictionary().Count > 0)
            {
                Box stringsBox = CreateContentBox(window, size.x - 30, "String Variables");
                foreach (KeyValuePair<string, string> save in part.variablesModule.stringVariables.GetSaveDictionary())
                {
                    InputWithLabel input = Builder.CreateInputWithLabel(stringsBox, size.x - 50, 50, 0, 0, save.Key, save.Value, s => ApplyStringVariable(save.Key, s));
                    input.label.AutoFontResize = false;
                    input.label.FontSize = 18;
                }
            }
            
            if (part.variablesModule.boolVariables.GetSaveDictionary().Count > 0)
            {
                Box boolsBox = CreateContentBox(window, size.x - 30, "Bool Variables");
                foreach (KeyValuePair<string, bool> save in part.variablesModule.boolVariables.GetSaveDictionary())
                {
                    ToggleWithLabel toggle = Builder.CreateToggleWithLabel(boolsBox, size.x - 50, 50, () => part.variablesModule.boolVariables.GetValue(save.Key), () => InvertBoolVariable(save.Key), labelText:save.Key);
                    toggle.label.AutoFontResize = false;
                    toggle.label.FontSize = 18;
                }
            }

            if (Config.settings.stretchToFit.Value)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(window.ChildrenHolder as RectTransform);
                // Needs to wait 2 frames before UI fully drawn
                await UniTask.Yield();
                await UniTask.Yield();
                window.Size = new Vector2(size.x, (window.ChildrenHolder as RectTransform).rect.height + 60);
            }
            else
                LayoutRebuilder.ForceRebuildLayoutImmediate(window.ChildrenHolder as RectTransform);
            
            void ApplyPositionX(float x) => part.Position = new Vector2(x, part.Position.y);
            void ApplyPositionY(float y) => part.Position = new Vector2(part.Position.x, y);
            
            void ApplyOrientationX(float x)
            {
                Orientation orientation = part.orientation.orientation.Value;
                part.orientation.orientation.Value = new Orientation(x, orientation.y, orientation.z);
                part.RegenerateMesh();
            }
            void ApplyOrientationY(float y)
            {
                Orientation orientation = part.orientation.orientation.Value;
                part.orientation.orientation.Value = new Orientation(orientation.x, y, orientation.z);
                part.RegenerateMesh();
            }
            void ApplyOrientationZ(float z)
            {
                Orientation orientation = part.orientation.orientation.Value;
                part.orientation.orientation.Value = new Orientation(orientation.x, orientation.y, z);
                part.RegenerateMesh();
            }

            void ApplyDoubleVariable(string name, float value)
            {
                part.variablesModule.doubleVariables.SetValue(name, value, (true, true));
                part.RegenerateMesh();
                AdaptModule.UpdateAdaptation(part);
            }
            void ApplyStringVariable(string name, string value)
            {
                part.variablesModule.stringVariables.SetValue(name, value, (true, true));
                part.RegenerateMesh();
                AdaptModule.UpdateAdaptation(part);
            }
            void InvertBoolVariable(string name)
            {
                part.variablesModule.boolVariables.SetValue(name, !part.variablesModule.boolVariables.GetValue(name), (true, true));
                part.RegenerateMesh();
                AdaptModule.UpdateAdaptation(part);
            }
        }

        static Box CreateContentBox(Transform parent, int width, string label)
        {
            Box box = Builder.CreateBox(parent, width, 10);
            box.CreateLayoutGroup(Type.Vertical, spacing: 5f, padding: new RectOffset(0, 0, 5, 5));
            // Enable auto-resizing
            box.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Builder.CreateLabel(box, width, 35, text: label);
            
            return box;
        }

        public static void CreateNumberInput(Transform parent, int width, int height, float inputWidthRatio, string label, float value, float step, Action<float> onChange, bool fixedFontSize = false, float fontSize = 0)
        {
            Container container = Builder.CreateContainer(parent);
            container.CreateLayoutGroup(Type.Horizontal);
            
            Label title = Builder.CreateLabel(container, (int)((width - 20) * (1 - inputWidthRatio)), (int)(height * 0.8f), text: label);
            title.TextAlignment = TextAlignmentOptions.MidlineLeft;
            if (fixedFontSize)
            {
                title.AutoFontResize = false;
                title.FontSize = fontSize;
            }
            
            UIToolsBuilder.CreateNumberInput(container, (int)((width - 20) * inputWidthRatio), height, value, step).OnValueChangedEvent += onChange;
        }
    }
}