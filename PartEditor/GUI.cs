using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SFS.Builds;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.UI;
using SFS.UI.ModGUI;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming
namespace PartEditor
{
    public static class GUI
    {
        private static GameObject holder;

        public static Window window;

        private static Part lastPart;

        private static TextInput positionX_Field;
        private static TextInput positionY_Field;
        private static TextInput orientationX_Field;
        private static TextInput orientationY_Field;
        private static TextInput orientationZ_Field;

        private static List<(string, TextInput)> doubleVariables;
        private static List<(string, TextInput)> stringVariables;

        public static void UpdateGUI(Part part = null)
        {
            if (part == lastPart && holder != null && window != null)
                return;

            DestroyGUI();
            lastPart = part;
            holder = new GameObject("PartEditor GUI Holder");

            // Scaling to 90%
            holder.transform.localScale = new Vector3(0.9f, 0.9f);

            Builder.AttachToCanvas(holder, Builder.SceneToAttach.CurrentScene);

            // Main window
            window = Builder.CreateWindow(holder, 350, 450, window != null ? Mathf.RoundToInt(window.Position.x) : 200,
                window != null ? Mathf.RoundToInt(window.Position.y) : 200, true, 1, "Part Editor");

            // Layout in window
            window.CreateLayoutGroup(LayoutType.Vertical).spacing = 20f;
            window.CreateLayoutGroup(LayoutType.Vertical).DisableChildControl();
            window.CreateLayoutGroup(LayoutType.Vertical).childAlignment = TextAnchor.MiddleCenter;

            if (part == null)
            {
                window.Size = new Vector2(350, 100);
                Builder.CreateLabel(window.ChildrenHolder, 300, 30, 0, 0, "Select a part").Opacity = 0.5f;
                return;
            }

            // Enable scrolling
            window.ChildrenHolder.GetComponent<ScrollElement>().vertical = true;

            // Position
            Box positionBox = CustomBox(320, "Position");
            positionX_Field = CreateNumberInput(positionBox.gameObject, 320, 50, 0.1f, "X:", 0.2f,
                part.Position.x, UpdateValues);
            positionY_Field = CreateNumberInput(positionBox.gameObject, 320, 50, 0.1f, "Y:", 0.2f,
                part.Position.y, UpdateValues);

            // Orientation
            Box orientationBox = CustomBox(320, "Orientation");
            orientationX_Field = CreateNumberInput(orientationBox.gameObject, 320, 50, 0.1f, "X:", 0.2f,
                part.orientation.orientation.Value.x, UpdateValues);
            orientationY_Field = CreateNumberInput(orientationBox.gameObject, 320, 50, 0.1f, "Y:", 0.2f,
                part.orientation.orientation.Value.y, UpdateValues);
            orientationZ_Field = CreateNumberInput(orientationBox.gameObject, 320, 50, -1, "Z:", 0.2f,
                part.orientation.orientation.Value.z, UpdateValues);

            // Double variables
            doubleVariables = new List<(string, TextInput)>();
            if (part.variablesModule.doubleVariables.saves.Any(x => x.save))
            {
                Box doubleVariablesBox = CustomBox(320, "Number Variables");
                foreach (string key in part.variablesModule.doubleVariables.GetSaveDictionary().Keys)
                    doubleVariables.Add((key,
                        CreateNumberInput(doubleVariablesBox.gameObject, 320, 50, 0.1f, key + ":", 0.3f,
                            (float)part.variablesModule.doubleVariables.GetValue(key), UpdateValues, 0.7f)));
            }

            // String variables
            stringVariables = new List<(string, TextInput)>();
            if (part.variablesModule.stringVariables.saves.Any(x => x.save))
            {
                Box stringVariablesBox = CustomBox(320, "String Variables");
                foreach (string key in part.variablesModule.stringVariables.GetSaveDictionary().Keys)
                    stringVariables.Add((key,
                        CreateStringInput(stringVariablesBox.gameObject, 320, 50, key + ":", 0.3f,
                            part.variablesModule.stringVariables.GetValue(key), 0.7f)));
            }

            if (part.variablesModule.boolVariables.saves.Any(x => x.save))
            {
                Box boolVariablesBox = CustomBox(320, "Bool Variables");
                foreach (string key in part.variablesModule.boolVariables.GetSaveDictionary().Keys)
                    CreateBoolInput(boolVariablesBox.gameObject, 320, 50, key + ":", 0.6f,
                        () => part.variablesModule.boolVariables.GetValue(key),
                        () => part.variablesModule.boolVariables.SetValue(key,
                            !part.variablesModule.boolVariables.GetValue(key)), 0.7f);
            }

            Builder.CreateButton(window.ChildrenHolder, 280, 60, 0, 0, UpdateValues, "Apply changes",
                Builder.Style.Blue);

            void UpdateValues()
            {
                part.Position =
                    new Vector2(float.Parse(positionX_Field.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                        float.Parse(positionY_Field.Text, NumberStyles.Any, CultureInfo.InvariantCulture));

                part.orientation.orientation.Value = new Orientation(
                    float.Parse(orientationX_Field.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    float.Parse(orientationY_Field.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    float.Parse(orientationZ_Field.Text, NumberStyles.Any, CultureInfo.InvariantCulture));

                if (doubleVariables.Count > 0)
                    foreach ((string, TextInput) variable in doubleVariables)
                        part.variablesModule.doubleVariables.SetValue(variable.Item1,
                            double.Parse(variable.Item2.Text, NumberStyles.Any, CultureInfo.InvariantCulture));

                if (stringVariables.Count > 0)
                    foreach ((string, TextInput) variable in stringVariables)
                        part.variablesModule.stringVariables.SetValue(variable.Item1, variable.Item2.Text);

                part.RegenerateMesh();
                AdaptModule.UpdateAdaptation(BuildManager.main.buildGrid.activeGrid.partsHolder.parts.ToArray());
            }
        }

        public static void DestroyGUI()
        {
            if (holder == null)
                return;
            Object.Destroy(holder);
            holder = null;
        }

        private static TextInput CreateNumberInput(GameObject parent, int width, int height, float changeStep,
            string title, float titleSizeMultiplier, float currentValue, Action applyChanges,
            float titleHeightMultiplier = 1)
        {
            int labelWidth = Mathf.RoundToInt((width - 20) * titleSizeMultiplier);
            int buttonWidth = Mathf.RoundToInt(height * 0.7f);
            int buttonHeight = Mathf.RoundToInt(height * 0.6f);
            int inputWidth = width - labelWidth - buttonWidth * 2 - 50;

            TextInput input = Builder.CreateTextInput(holder, inputWidth, height, 0, 0, currentValue.ToString("G",
                CultureInfo.InvariantCulture), style: Builder.Style.Blue);

            Container container = Builder.CreateContainer(parent, 0, 0);
            container.CreateLayoutGroup(LayoutType.Horizontal).spacing = 5f;
            container.CreateLayoutGroup(LayoutType.Horizontal).DisableChildControl();
            container.CreateLayoutGroup(LayoutType.Horizontal).childAlignment = TextAnchor.MiddleCenter;

            Builder.CreateLabel(container.gameObject, labelWidth,
                Mathf.RoundToInt(height * 0.8f * titleHeightMultiplier), 0, 0, title);

            void OnClick(float m)
            {
                input.ChangeAsNumber(changeStep * m);
                applyChanges.Invoke();
            }

            Builder.CreateButton(container.gameObject, buttonWidth, buttonHeight, 0, 0,
                () => OnClick(-1), "<", Builder.Style.Blue);
            input.rectTransform.SetParent(container.rectTransform);
            Builder.CreateButton(container.gameObject, buttonWidth, buttonHeight, 0, 0,
                () => OnClick(1), ">", Builder.Style.Blue);

            return input;
        }

        private static TextInput CreateStringInput(GameObject parent, int width, int height,
            string title, float titleSizeMultiplier, string currentValue, float titleHeightMultiplier = 1)
        {
            int labelWidth = Mathf.RoundToInt((width - 20) * titleSizeMultiplier);
            int inputWidth = width - labelWidth - 30;
            Container container = Builder.CreateContainer(parent, 0, 0);
            container.CreateLayoutGroup(LayoutType.Horizontal).spacing = 5f;
            container.CreateLayoutGroup(LayoutType.Horizontal).DisableChildControl();
            container.CreateLayoutGroup(LayoutType.Horizontal).childAlignment = TextAnchor.MiddleCenter;

            Builder.CreateLabel(container.gameObject, labelWidth,
                Mathf.RoundToInt(height * 0.8f * titleHeightMultiplier), 0, 0, title);
            TextInput input = Builder.CreateTextInput(container.gameObject, inputWidth, height, 0, 0, currentValue,
                style: Builder.Style.Blue);

            return input;
        }

        private static void CreateBoolInput(GameObject parent, int width, int height,
            string title, float titleSizeMultiplier, Func<bool> get, Action onChange, float titleHeightMultiplier = 1)
        {
            const int toggleWidth = 80;
            int labelWidth = Mathf.RoundToInt((width - 20) * titleSizeMultiplier);
            int space = width - 20 - toggleWidth - labelWidth;

            Container container = Builder.CreateContainer(parent, 0, 0);
            container.CreateLayoutGroup(LayoutType.Horizontal).spacing = space;
            container.CreateLayoutGroup(LayoutType.Horizontal).DisableChildControl();
            container.CreateLayoutGroup(LayoutType.Horizontal).childAlignment = TextAnchor.MiddleCenter;

            Builder.CreateLabel(container.gameObject, labelWidth,
                Mathf.RoundToInt(height * 0.8f * titleHeightMultiplier), 0, 0, title);

            Builder.CreateToggle(container.gameObject, 0, 0, get, onChange);
        }

        private static void ChangeAsNumber(this TextInput input, float change)
        {
            input.Text =
                (float.Parse(input.Text, NumberStyles.Any, CultureInfo.InvariantCulture) + change).ToString("G",
                    CultureInfo.InvariantCulture);
        }

        private static Box CustomBox(int width, string label)
        {
            Box box = Builder.CreateBox(window.ChildrenHolder, width, 10);
            
            // Auto resize for box
            box.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            box.CreateLayoutGroup(LayoutType.Vertical).spacing = 10f;
            box.CreateLayoutGroup(LayoutType.Vertical).DisableChildControl();
            box.CreateLayoutGroup(LayoutType.Vertical).childAlignment = TextAnchor.MiddleCenter;
            box.CreateLayoutGroup(LayoutType.Vertical).padding = new RectOffset(0, 0, 5, 5);
            Builder.CreateLabel(box.gameObject, width, 35, 0, 0, label);

            return box;
        }
    }
}