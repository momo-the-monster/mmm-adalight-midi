using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Helios.Settings
{
    public class SettingsWindow : EditorWindow
    {
        // the selected group tab
        private int activeGroupIndex = -1;
        private string[] groupNames;
        private string activeGroupName;
        public const string superGroupName = "SettingsWindow";

        // fields
        private ReorderableList fields;

        // Style properties
        GUIStyle styleTextField;

        /// <summary>
        /// Initialize window state.
        /// </summary>
        [MenuItem("Helios/Settings/Show", false, 0)]
        internal static void Init()
        {
            // EditorWindow.GetWindow() will return the open instance of the specified window or create a new
            // instance if it can't find one. The second parameter is a flag for creating the window as a
            // Utility window; Utility windows cannot be docked like the Scene and Game view windows.
            var window = GetWindowInstance();
            window.position = new Rect(Screen.width /2, Screen.height/2, 200f, 400f);
            window.SelectNewestGroup();
        }

        public static SettingsWindow GetWindowInstance()
        {
            return (SettingsWindow)GetWindow(typeof(SettingsWindow), false, "Helios.Settings");
        }

        [MenuItem("Helios/Settings/Generate Strings", false, 1)]
        internal static void GenerateStrings()
        {
            using (var process = new System.Diagnostics.Process())
            {
                var workingDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "_Vendor/Helios/Settings/Editor/SettingsGenerator"));
                process.StartInfo.FileName = $"{workingDirectory}/GenerateStrings.exe";
                process.StartInfo.WorkingDirectory = workingDirectory;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.OutputDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e)=> {
                    Debug.Log($"GenerateStrings:{e.Data}");
                };

                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
                process.Close();
            }

            AssetDatabase.Refresh();
        }

        private static void Process_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void Process_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Start()
        {
            SetupFields();
        }

        public void SetupFields()
        {
            List<(string, dynamic)> allFields = new List<(string, dynamic)>();
            foreach (var group in GetEditorWindowSupergroup().GetGroups())
            {
                if (group.Name == activeGroupName && group.GetFieldCount() > 0)
                {
                    foreach ((string name, dynamic field) in group.GetFields())
                    {
                        allFields.Add((name, field));
                    }
                }
            }

            fields = new ReorderableList(allFields, typeof(string), true, true, true, true);
            fields.drawElementCallback = DrawField;
            fields.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Fields");
            };
            fields.onAddDropdownCallback = AddDropdown;
            fields.onRemoveCallback = OnRemove;

            // Cache the new groupNames for use later.
            groupNames = SuperGroup.Get(superGroupName).Groups.ToArray();
        }

        public void SelectNewestGroup()
        {
            groupNames = SuperGroup.Get(superGroupName).Groups.ToArray();
            activeGroupIndex = groupNames.Length - 1;
            activeGroupName = groupNames[activeGroupIndex];
            SetupFields();
        }

        private void OnRemove(ReorderableList list)
        {
            (string name, dynamic) entry = ((string, dynamic))list.list[list.index];
            Group.Get(activeGroupName).DeleteField(entry.name);
            SetupFields();
        }

        private void AddDropdown(Rect buttonRect, ReorderableList list)
        {
            if(activeGroupName != null && activeGroupName.Length > 0)
            {
                var menu = new GenericMenu();

                // Generate Actions for each FieldType
                foreach (string item in Enum.GetNames(typeof(FieldType)))
                {
                    if (item.CompareTo(FieldType.Group.ToString()) == 0) continue;
                    menu.AddItem(
                        new GUIContent(item),
                        false,
                        AddItemHandler,
                        (FieldType)Enum.Parse(typeof(FieldType), item));
                }
                menu.ShowAsContext();
            }
            else
            {
                // Show dialog with warning about needing groups first
                this.ShowNotification(new GUIContent("You need a group first."));
            }
        }

        private void AddItemHandler(object target)
        {
            var wizard = ScriptableWizard.DisplayWizard<AddFieldWizard>("Add Field", "Confirm");
            wizard.fieldType = (FieldType)target;
            wizard.activeGroup = activeGroupName;
            wizard.position = GetRectForSize(300, 200);
        }

        private void DrawField(Rect rect, int index, bool isActive, bool isFocused)
        {
            // Cache element
            (string name, dynamic field) entry = ((string, dynamic))fields.list[index];
            var name = entry.name;
            var field = entry.field;

            // Calculate Column Sizes
            float space = 10;
            float rect1width = 120;
            float rect3width = 60;
            float rect2width = rect.width - rect1width - rect3width - (space * 3);

            Rect column1Rect = new Rect(rect.x, rect.y, rect1width, EditorGUIUtility.singleLineHeight);
            Rect column2Rect = new Rect(rect.x + rect1width + space, rect.y, rect2width, EditorGUIUtility.singleLineHeight);
            Rect column3Rect = new Rect(column2Rect.x + rect2width + space, rect.y, rect3width, EditorGUIUtility.singleLineHeight);

            // Do Layout
            rect.y += 2;
            // Name column, always a string
            EditorGUI.LabelField(column1Rect, name, GetTextFieldStyle());

            // Handle all drawable types here
            DrawField(field, column2Rect);

            // Type column
            EditorGUI.LabelField(column3Rect, field.InnerType.Name, GetTextFieldStyle());
        }

        private void DrawField<T>(Field<T> field, Rect rect)
        {
            dynamic currentValue = field.Get();
            dynamic newValue;

            switch (field)
            {
                case Field<string> typedField:
                    newValue = EditorGUI.DelayedTextField(rect, currentValue, GetTextFieldStyle());
                    if (newValue != currentValue) UpdateFieldInActiveGroup(typedField, newValue);
                    break;
                case Field<int> typedField:
                    newValue = EditorGUI.DelayedIntField(rect, currentValue, GetTextFieldStyle());
                    if (newValue != currentValue) UpdateFieldInActiveGroup(typedField, newValue);
                    break;
                case Field<float> typedField:
                    newValue = EditorGUI.DelayedFloatField(rect, currentValue, GetTextFieldStyle());
                    if (newValue != currentValue) UpdateFieldInActiveGroup(typedField, newValue);
                    break;
                case Field<Vector2> typedField:
                    newValue = EditorGUI.Vector2Field(rect, "", currentValue);
                    if (newValue != currentValue) UpdateFieldInActiveGroup(typedField, newValue);
                    break;
                case Field<Vector3> typedField:
                    newValue = EditorGUI.Vector3Field(rect, "", currentValue);
                    if (newValue != currentValue) UpdateFieldInActiveGroup(typedField, newValue);
                    break;
                case Field<bool> typedField:
                    newValue = EditorGUI.Toggle(rect, currentValue);
                    if (newValue != currentValue) UpdateFieldInActiveGroup(typedField, newValue);
                    break;
            }
        }

        private GUIStyle GetTextFieldStyle()
        {
            styleTextField = new GUIStyle(EditorStyles.textField);
            styleTextField.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            return styleTextField;
        }

        private void UpdateFieldInActiveGroup<T>(Field<T> field, T value)
        {
            field.Set(value);
            Group.Get(activeGroupName).Save();
        }

        [MenuItem("Helios/Settings/Add Example Groups", false, 2)]
        public static void AddExampleGroups()
        {
            var settingsWindowSuperGroup = SuperGroup.Get(superGroupName);

            var reach = settingsWindowSuperGroup.Set("Reach");
            var design = settingsWindowSuperGroup.Set("Design");

            reach.Set("Url", "http://reachstaging.herokuapp.com/api");
            reach.Set("Port", 8080);
            reach.Set("Public Key", "bun4qC3eU5Zh");
            reach.Set("API Key", "xDW4pN2thG8vh6AA");
            reach.Set("Activation ID", "e7dddc0e-1528-45bd-bc6c-9d6d57e10e8e");
            reach.Set("IsProduction", false);

            design.Set("Font Size", 26);
            design.Set("Font Name", "Comic Sans");
            design.Set("Corner Offset", new Vector2(0.5f, 0.5f));
            design.Set("Transition Duration", 0.75f);
            design.Set("3D Position", new Vector3(0.1f, 0.2f, 0.3f));

            settingsWindowSuperGroup.Save();
            reach.Save();
            design.Save();

            var window = GetWindowInstance();
            window.SetupFields();
            window.activeGroupIndex = -1;
        }

        private void OnGUI()
        {
            // cache the position of the mouse for this interaction
            latestMousePosition = Event.current.mousePosition;

            // Set groupIndex
            int currentGroupIndex = activeGroupIndex;
            if (activeGroupIndex == -1) activeGroupIndex = 0;

            // Start Header Buttons Layout
            EditorGUILayout.BeginHorizontal();
            GUILayout.ExpandWidth(true);
            Rect buttonRect = new Rect(0, 0, 10, EditorGUIUtility.singleLineHeight);

            // Show group chooser if we have groups
            if (groupNames != null && groupNames.Length > 0)
            {
                activeGroupIndex = GUILayout.Toolbar(activeGroupIndex, groupNames);
                activeGroupName = groupNames[activeGroupIndex];
            }
            else
            {
                GUILayout.Label("Create a Group:");
            }

            // Show + button to add a group
            if (GUILayout.Button("+", GUILayout.Width(25)))
            {
                var wizard = ScriptableWizard.DisplayWizard<AddFieldWizard>("Add Group", "Confirm");
                wizard.fieldType = FieldType.Group;
                wizard.position = GetRectForSize(300, 150);
            }

            // Show - button if we have groups
            using (new EditorGUI.DisabledScope(groupNames == null || groupNames.Length <= 0))
            {
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    string groupToDelete = groupNames[activeGroupIndex];
                    if (EditorUtility.DisplayDialog("U Sure?", $"Delete {groupToDelete}?", "Yes", "No"))
                    {
                        GetEditorWindowSupergroup().DeleteGroup(groupToDelete);
                        Group.Get(groupToDelete).DeleteGroup();
                        groupNames = SuperGroup.Get(superGroupName).Groups.ToArray();
                        activeGroupIndex = -1;
                        SetupFields();
                    }
                }

            }

            EditorGUILayout.EndHorizontal();
            // End Header layout

            if (currentGroupIndex != activeGroupIndex || fields == null)
            {
                // update fields list
                SetupFields();
            }

            EditorGUILayout.BeginVertical();

            if (fields != null && groupNames.Length > 0)
            {
                fields.DoLayoutList();
            }

            EditorGUILayout.EndVertical();
        }

        Vector2 latestMousePosition;

        public static SuperGroup GetEditorWindowSupergroup()
        {
            return SuperGroup.Get(SettingsWindow.superGroupName);
        }

        private Rect GetRectForSize(int width, int height)
        {
            Vector2 wSize = new Vector2(width, height);
            Vector2 mousePos = GUIUtility.GUIToScreenPoint(latestMousePosition);
            return new Rect(mousePos.x - wSize.x / 2, mousePos.y - wSize.y / 2, wSize.x, wSize.y);
        }

    }

    public enum FieldType
    {
        String, Int, Float, Bool, Vector2, Vector3, Group
    }

    public class AddFieldWizard : ScriptableWizard
    {
        public string itemName = "defaultName";
        public string activeGroup = "";
        public FieldType fieldType;
        public dynamic value;
        private bool initialFocusWasSet = false;

        // When create is pressed
        private void OnWizardCreate()
        {
            var window = SettingsWindow.GetWindowInstance();
            if (fieldType == FieldType.Group) // When creating a group we don't update the UI directly, but rather add the group to the list of available groups in the group context menu 
            {
                var superGroup = SettingsWindow.GetEditorWindowSupergroup();
                superGroup.Set(itemName).Save();
                superGroup.Save();
            }
            else
            {
                Group.Get(activeGroup).Set(itemName, value).Save();
            }
            window.SetupFields();
            AssetDatabase.Refresh();
        }

        protected override bool DrawWizardGUI()
        {
            EditorGUILayout.BeginVertical();

            GUI.SetNextControlName("nameField");
            itemName = EditorGUILayout.TextField(new GUIContent("name"), itemName);
            bool result = false;
            GUI.SetNextControlName("valueField");
            switch (fieldType)
            {
                case FieldType.String:
                    if (value == null) value = "";
                    value = EditorGUILayout.TextField("value", value);
                    break;
                case FieldType.Int:
                    if (value == null) value = 0;
                    value = EditorGUILayout.IntField("value", value);
                    break;
                case FieldType.Float:
                    if (value == null) value = 0;
                    value = EditorGUILayout.FloatField("value", value);
                    break;
                case FieldType.Bool:
                    if (value == null) value = false;
                    value = EditorGUILayout.Toggle("value", value);
                    break;
                case FieldType.Vector2:
                    if (value == null) value = Vector2.zero;
                    value = EditorGUILayout.Vector2Field("value", value);
                    break;
                case FieldType.Vector3:
                    if (value == null) value = Vector3.zero;
                    value = EditorGUILayout.Vector3Field("value", value);
                    break;
                case FieldType.Group:
                    break;
                default:
                    break;
            }

            EditorGUILayout.EndVertical();

            if (!initialFocusWasSet)
            {
                GUI.FocusControl("nameField");
                initialFocusWasSet = true;
            }

            return result;
        }

    }
}