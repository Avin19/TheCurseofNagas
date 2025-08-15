// #define TEST_BTS
#define TEST_DIALOGUE_PARSER

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace CurseOfNaga.DialogueSystem.Editor
{
    public class DialogueGraphEditor : EditorWindow
    {
        internal enum DataOperation { LOAD_JSON, SAVE_JSON, CLEAR_GRAPH }

        private GraphSaveUtility graphSaveUtility;
        private DialogueGraphView _graphView;
        private string _fileName = "Dialogues_SerializeTest";

        [MenuItem("Graph/Dialogue Graph")]
        public static void OpenDialogueGraphWindow()
        {
            var window = GetWindow<DialogueGraphEditor>();
            window.titleContent = new GUIContent("Dialogue Graph");
        }

        private void OnDisable()
        {
            // Debug.Log($"Editor Disabled");
            rootVisualElement.Remove(_graphView);

            AssemblyReloadEvents.afterAssemblyReload -= OnBeforeAssemblyReload;
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
            // GenerateMiniMap();

            graphSaveUtility = GraphSaveUtility.GetInstance(_graphView);

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        //Prevent indexing error of Nodes removal 
        private void OnBeforeAssemblyReload()
        {
            _graphView.TotalReset = true;
        }

        private void GenerateMiniMap()
        {
            var minimap = new MiniMap { anchored = true };
            minimap.SetPosition(new Rect(10, 30, 120, 100));
            _graphView.Add(minimap);
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

#if TEST_BTS
            // toolbar.Add(new Button(() => CheckAssetFolder()) { text = "Check Asset Folder" });
#endif
            toolbar.Add(new Button(() => RequestDataOperation(DataOperation.CLEAR_GRAPH)) { text = "Clear All" });

#if TEST_DIALOGUE_PARSER
            toolbar.Add(new Button(() => RequestDataOperation(DataOperation.LOAD_JSON)) { text = "Load JSON" });
            toolbar.Add(new Button(() => RequestDataOperation(DataOperation.SAVE_JSON)) { text = "Save JSON" });
#endif

            var nodeCreateButton = new Button(() => { _graphView.CreateNode("Dialogue Node"); });
            nodeCreateButton.text = "Create Node";
            toolbar.Add(nodeCreateButton);

            rootVisualElement.Add(toolbar);
        }

        private void RequestDataOperation(DataOperation dataOpType)
        {
            graphSaveUtility = GraphSaveUtility.GetInstance(_graphView);
            switch (dataOpType)
            {
                case DataOperation.LOAD_JSON:
                    graphSaveUtility.LoadDialogueJson(_fileName);
                    break;

                case DataOperation.SAVE_JSON:
                    graphSaveUtility.SaveDialogueJson(_fileName);
                    break;

                case DataOperation.CLEAR_GRAPH:
                    bool clear = EditorUtility.DisplayDialog("Clear Graph?",
                        "Proceed to clear graph? No Progress save", "Clear", "Cancel");
                    if (clear) graphSaveUtility.ClearGraph();

                    break;
            }
        }

#if TEST_BTS
        private void CheckAssetFolder()
        {
            graphSaveUtility = GraphSaveUtility.GetInstance(_graphView);
            graphSaveUtility.CheckAssetFolder();
        }
#endif
    }
}
#endif