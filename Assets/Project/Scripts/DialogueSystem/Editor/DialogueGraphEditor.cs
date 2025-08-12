// #define TEST_BTS

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace CurseOfNaga.DialogueSystem.Editor
{
    public class DialogueGraphEditor : EditorWindow
    {
        private DialogueGraphView _graphView;
        private string _fileName = "New Narrative";

        [MenuItem("Graph/Dialogue Graph")]
        public static void OpenDialogueGraphWindow()
        {
            var window = GetWindow<DialogueGraphEditor>();
            window.titleContent = new GUIContent("Dialogue Graph");
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
            // GenerateMiniMap();
        }

        private void GenerateMiniMap()
        {
            var minimap = new MiniMap { anchored = true };
            minimap.SetPosition(new Rect(10, 30, 200, 140));
            _graphView.AddElement(minimap);
        }

        private void ConstructGraphView()
        {
            _graphView = new DialogueGraphView
            {
                name = "Dialogue Graph"
            };

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            var fileNameTextField = new TextField("File Name");
            fileNameTextField.SetValueWithoutNotify(_fileName);
            fileNameTextField.MarkDirtyRepaint();
            fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
            toolbar.Add(fileNameTextField);

            toolbar.Add(new Button(() => RequestDataOperation(true)) { text = "Save Data" });
            toolbar.Add(new Button(() => RequestDataOperation(false)) { text = "Load Data" });
#if TEST_BTS
            toolbar.Add(new Button(() => TestBt(0)) { text = "Check Asset Folder" });
#endif

            var nodeCreateButton = new Button(() => { _graphView.CreateNode("Dialogue Node"); });
            nodeCreateButton.text = "Create Node";
            toolbar.Add(nodeCreateButton);

            rootVisualElement.Add(toolbar);
        }

        private void RequestDataOperation(bool save)
        {
            if (string.IsNullOrEmpty(_fileName))
            {
                EditorUtility.DisplayDialog("Invalid file name", "Please enter a valid file name", "OK");
                return;
            }

            var saveutility = GraphSaveUtility.GetInstance(_graphView);
            if (save)
                saveutility.SaveGraph(_fileName);
            else
                saveutility.LoadGraph(_fileName);
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }

#if TEST_BTS
        private void TestBt(int btTestIndex)
        {
            var saveutility = GraphSaveUtility.GetInstance(_graphView);
            switch (btTestIndex)
            {
                case 0:
                    saveutility.CheckAssetFolder();
                    break;
            }
        }
#endif
    }
}
#endif