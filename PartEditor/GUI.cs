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
using Type = SFS.UI.ModGUI.Type;

// ReSharper disable InconsistentNaming
namespace PartEditor
{
    public static class GUI
    {
        public static bool forced;
        static GameObject holder;

        public static readonly int WindowID = Builder.GetRandomID();

        static Part lastPart;

        static TextInput positionX_Field;
        static TextInput positionY_Field;
        static TextInput orientationX_Field;
        static TextInput orientationY_Field;
        static TextInput orientationZ_Field;

        static List<(string, TextInput)> doubleVariables;
        static List<(string, TextInput)> stringVariables;

        public static void UpdateGUI(Part part = null)
        {
            if (part == lastPart && !forced)
                return;
            
            forced = false;

            DestroyGUI();
            lastPart = part;
            holder = Builder.CreateHolder(Builder.SceneToAttach.CurrentScene,"PartEditor GUI Holder");

            // Scaling to 90%
            holder.transform.localScale = new Vector3(0.9f, 0.9f);

            // Main window
            Window window = Builder.CreateWindow(holder.transform, WindowID, 350, 450, 200, 200, true, true, 1, "Part Editor");

            // Layout in window
            window.CreateLayoutGroup(Type.Vertical);
            
            if (part == null)
            {
                window.Size = new Vector2(350, 100);
                Builder.CreateLabel(window.ChildrenHolder, 300, 30, 0, 0, "Select a part").Opacity = 0.5f;
                return;
            }

            // Enable scrolling
            window.EnableScrolling(Type.Vertical);

            // Position
            Box positionBox = CustomBox(320, "Position", window);
            positionX_Field = CreateNumberInput(positionBox, 320, 50, 0.1f, "X:", 0.2f,
                part.Position.x, UpdateValues);
            positionY_Field = CreateNumberInput(positionBox, 320, 50, 0.1f, "Y:", 0.2f,
                part.Position.y, UpdateValues);

            // Orientation
            Box orientationBox = CustomBox(320, "Orientation", window);
            orientationX_Field = CreateNumberInput(orientationBox, 320, 50, 0.1f, "X:", 0.2f,
                part.orientation.orientation.Value.x, UpdateValues);
            orientationY_Field = CreateNumberInput(orientationBox, 320, 50, 0.1f, "Y:", 0.2f,
                part.orientation.orientation.Value.y, UpdateValues);
            orientationZ_Field = CreateNumberInput(orientationBox, 320, 50, -1, "Z:", 0.2f,
                part.orientation.orientation.Value.z, UpdateValues);

            // Double variables
            doubleVariables = new List<(string, TextInput)>();
            if (part.variablesModule.doubleVariables.saves.Any(x => x.save))
            {
                Box doubleVariablesBox = CustomBox(320, "Number Variables", window);
                foreach (string key in part.variablesModule.doubleVariables.GetSaveDictionary().Keys)
                    doubleVariables.Add((key,
                        CreateNumberInput(doubleVariablesBox, 320, 50, 0.1f, key + ":", 0.3f,
                            (float)part.variablesModule.doubleVariables.GetValue(key), UpdateValues, 0.7f)));
            }

            // String variables
            stringVariables = new List<(string, TextInput)>();
            if (part.variablesModule.stringVariables.saves.Any(x => x.save))
            {
                Box stringVariablesBox = CustomBox(320, "String Variables", window);
                foreach (string key in part.variablesModule.stringVariables.GetSaveDictionary().Keys)
                    stringVariables.Add((key,
                        CreateStringInput(stringVariablesBox, 320, 50, key + ":", 0.3f,
                            part.variablesModule.stringVariables.GetValue(key), 0.7f)));
            }

            if (part.variablesModule.boolVariables.saves.Any(x => x.save))
            {
                Box boolVariablesBox = CustomBox(320, "Bool Variables", window);
                foreach (string key in part.variablesModule.boolVariables.GetSaveDictionary().Keys)
                    CreateBoolInput(boolVariablesBox, 320, 50, key + ":", 0.6f,
                        () => part.variablesModule.boolVariables.GetValue(key),
                        () => part.variablesModule.boolVariables.SetValue(key,
                            !part.variablesModule.boolVariables.GetValue(key)), 0.7f);
            }

            Builder.CreateButton(window.ChildrenHolder, 280, 60, 0, 0, UpdateValues, "Apply changes");

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

        static TextInput CreateNumberInput(Transform parent, int width, int height, float changeStep,
            string title, float titleSizeMultiplier, float currentValue, Action applyChanges,
            float titleHeightMultiplier = 1)
        {
            int labelWidth = Mathf.RoundToInt((width - 20) * titleSizeMultiplier);
            int buttonWidth = Mathf.RoundToInt(height * 0.7f);
            int buttonHeight = Mathf.RoundToInt(height * 0.6f);
            int inputWidth = width - labelWidth - buttonWidth * 2 - 50;

            TextInput input = Builder.CreateTextInput(holder.transform, inputWidth, height, 0, 0, currentValue.ToString("G",
                CultureInfo.InvariantCulture));

            Container container = Builder.CreateContainer(parent, 0, 0);
            container.CreateLayoutGroup(Type.Horizontal, spacing: 5f);

            Builder.CreateLabel(container, labelWidth,
                Mathf.RoundToInt(height * 0.8f * titleHeightMultiplier), 0, 0, title);

            void OnClick(float m)
            {
                input.ChangeAsNumber(changeStep * m);
                applyChanges.Invoke();
            }

            Builder.CreateButton(container, buttonWidth, buttonHeight, 0, 0,
                () => OnClick(-1), "<");
            input.rectTransform.SetParent(container.rectTransform);
            Builder.CreateButton(container, buttonWidth, buttonHeight, 0, 0,
                () => OnClick(1), ">");

            return input;
        }

        static TextInput CreateStringInput(Transform parent, int width, int height,
            string title, float titleSizeMultiplier, string currentValue, float titleHeightMultiplier = 1)
        {
            int labelWidth = Mathf.RoundToInt((width - 20) * titleSizeMultiplier);
            int inputWidth = width - labelWidth - 30;
            Container container = Builder.CreateContainer(parent, 0, 0);
            container.CreateLayoutGroup(Type.Horizontal, spacing: 5f);

            Builder.CreateLabel(container, labelWidth,
                Mathf.RoundToInt(height * 0.8f * titleHeightMultiplier), 0, 0, title);
            TextInput input = Builder.CreateTextInput(container, inputWidth, height, 0, 0, currentValue);

            return input;
        }

        static void CreateBoolInput(Transform parent, int width, int height,
            string title, float titleSizeMultiplier, Func<bool> get, Action onChange, float titleHeightMultiplier = 1)
        {
            const int toggleWidth = 80;
            int labelWidth = Mathf.RoundToInt((width - 20) * titleSizeMultiplier);
            int space = width - 20 - toggleWidth - labelWidth;

            Container container = Builder.CreateContainer(parent, 0, 0);
            container.CreateLayoutGroup(Type.Horizontal, spacing: space);

            Builder.CreateLabel(container, labelWidth,
                Mathf.RoundToInt(height * 0.8f * titleHeightMultiplier), 0, 0, title);

            Builder.CreateToggle(container, get, onChange: onChange);
        }

        static void ChangeAsNumber(this TextInput input, float change)
        {
            input.Text =
                (float.Parse(input.Text, NumberStyles.Any, CultureInfo.InvariantCulture) + change).ToString("G",
                    CultureInfo.InvariantCulture);
        }

        static Box CustomBox(int width, string label, Window window)
        {
            Box box = Builder.CreateBox(window, width, 10);

            // Auto resize for box
            box.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            box.CreateLayoutGroup(Type.Vertical, spacing: 10f, padding: new RectOffset(0, 0, 5, 5));
            Builder.CreateLabel(box, width, 35, 0, 0, label);

            return box;
        }
    }
}