// #define TEST_BTS
// #define TEST_TO_JSON

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor.Experimental.GraphView;

using CurseOfNaga.DialogueSystem.Runtime;
using UnityEditor;
using UnityEngine.UIElements;
using CurseOfNaga.Utils;

namespace CurseOfNaga.DialogueSystem.Editor
{
    public class GraphSaveUtility
    {
        private DialogueGraphView _targetGraphView;
        private DialogueContainer _containerCache;
        private List<Edge> _edges => _targetGraphView.edges.ToList();
        private List<Node> _nodes => _targetGraphView.nodes.ToList();//.Cast<DialogueNode>().ToList();

        private const string _PARENT_FOLDER_PATH = "Assets/Project";        // Assets/Project/Prefabs/Player/Player.prefab
        private const string _RESOURCES_FOLDER_PATH = "Resources";
        private const string _DIALOGUE_JSON = "Dialogues_Test.json";

        public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
        {
            return new GraphSaveUtility
            {
                _targetGraphView = targetGraphView
            };
        }

        public void SaveGraph(string fileName)
        {
            //TEST
            /*{
                for (int i = 0; i < _nodes.Count; i++)
                    Debug.Log($"i: [{i}]| Node EntryPoint: {_nodes[i].EntryPoint}");
                return;
            }*/

            if (!_edges.Any()) return;       //if there are no edges(no connections) then return
            Debug.Log($"Saving Graph: {fileName}");

            var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
            var connectedPorts = _edges.Where(e => e.input.node != null).ToArray();

            int totalCount = connectedPorts.Length;
            for (int i = 0; i < totalCount; i++)
            {
                var outputNode = connectedPorts[i].output.node;
                var inputNode = connectedPorts[i].input.node;

                dialogueContainer.NodeLinks.Add(new DialogueNodeLinkData
                {
                    BaseNodeGUID = outputNode.viewDataKey,
                    PortName = connectedPorts[i].output.name,
                    TargetNodeGUID = inputNode.viewDataKey
                });
            }

            totalCount = _nodes.Count;
            for (int i = 1; i < totalCount; i++)
            {
                dialogueContainer.DialogueNodeDatas.Add(new DialogueNodeData
                {
                    GUID = _nodes[i].viewDataKey,
                    // DialogueText = _nodes[i].DialogueText,
                    Position = _nodes[i].GetPosition().position
                });
            }

            //Check if the Reources folder exists in the project or not
            if (!AssetDatabase.IsValidFolder(_PARENT_FOLDER_PATH + "/" + _RESOURCES_FOLDER_PATH))
            {
                Debug.Log($"Resources Folder not found | Creating folder at: "
                    + $"{_PARENT_FOLDER_PATH}/{_RESOURCES_FOLDER_PATH}");
                AssetDatabase.CreateFolder(_PARENT_FOLDER_PATH, _RESOURCES_FOLDER_PATH);
            }

            //TODO: Change this, change the data in the existing asset instead of creating a new one
            AssetDatabase.CreateAsset(dialogueContainer, $"{_PARENT_FOLDER_PATH}/{_RESOURCES_FOLDER_PATH}/{fileName}.asset");
            AssetDatabase.SaveAssets();
        }

        public void LoadGraph(string fileName)
        {
            _containerCache = Resources.Load<DialogueContainer>(fileName);
            if (_containerCache == null)
            {
                EditorUtility.DisplayDialog("File Not Found", "Target DialogueContainer file does not exist", "OK");
                return;
            }

            ClearGraph();
            GenerateNodes();
            ConnectNodes();
        }

        private void ClearGraph()
        {
            //Set the base EntryPoint node GUID first. Discard existing guid
            // _nodes.Find(node => node.EntryPoint).GUID = _containerCache.NodeLinks[0].BaseNodeGUID;
            _nodes[0].viewDataKey = DialogueGraphView.BASE_NODE;

            for (int i = 1; i < _nodes.Count;)
            {
                // Remove all the edges connected to this graph
                _edges.Where(edge => edge.input.node == _nodes[i]).ToList()
                            .ForEach(edge => _targetGraphView.RemoveElement(edge));

                //Then finally remove the node
                _targetGraphView.RemoveElement(_nodes[i]);         //This is connected to the _nodes List
            }
        }

        private void GenerateNodes()
        {
            Node tempNode;
            List<DialogueNodeLinkData> nodePorts;

            for (int i = 0; i < _containerCache.DialogueNodeDatas.Count; i++)
            {
                tempNode = _targetGraphView.CreateDialogueNode(_containerCache.DialogueNodeDatas[i].GUID);
                tempNode.viewDataKey = _containerCache.DialogueNodeDatas[i].GUID;
                _targetGraphView.AddElement(tempNode);

                nodePorts = _containerCache.NodeLinks.Where(edge => edge.BaseNodeGUID.Equals(_containerCache.DialogueNodeDatas[i].GUID)).ToList();
                nodePorts.ForEach(edge => _targetGraphView.AddChoicePort(tempNode, edge.PortName));
            }
        }

        private void ConnectNodes()
        {
            DialogueNodeLinkData[] connections;
            Node targetNode;

            for (int i = 0; i < _nodes.Count; i++)
            {
                connections = _containerCache.NodeLinks.Where(nodeLink => nodeLink.BaseNodeGUID.Equals(_nodes[i].viewDataKey)).ToArray();
                for (int j = 0; j < connections.Length; j++)
                {
                    var targetNodeGuid = connections[j].TargetNodeGUID;
                    targetNode = _nodes.First(node => node.viewDataKey.Equals(targetNodeGuid));
                    LinkNodes(_nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                    targetNode.SetPosition(new Rect(
                        _containerCache.DialogueNodeDatas.First(nodeData => nodeData.GUID.Equals(targetNodeGuid)).Position,
                        _targetGraphView._defaultNodeSize
                    ));
                }
            }
        }

        private void LinkNodes(Port output, Port input)
        {
            var tempEdge = new Edge
            {
                output = output,
                input = input
            };
            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);
            _targetGraphView.Add(tempEdge);
        }

        public async void LoadDialogueJson()
        {
            string pathToJson = System.IO.Path.Join(Application.streamingAssetsPath, _DIALOGUE_JSON);
            Debug.Log($"Loadiing Json from: {pathToJson}");

            if (!System.IO.File.Exists(pathToJson))
            {
                EditorUtility.DisplayDialog("Invalid file path", "Please enter a valid file path to Dialogues.json", "OK");
                return;
            }

            // TestToJson(); return;            //TEST

            FileDataHelper fileDataHelper = new FileDataHelper();
            string jsonData = await fileDataHelper.GetFileData_Async(pathToJson);

            DialogueTemplate dialogueTemplate;
            dialogueTemplate = JsonUtility.FromJson<DialogueTemplate>(jsonData);
            // Debug.Log($"Dialogue Template: \n{dialogueTemplate} | jsonData: {jsonData}");

            Node tempNode;
            DialogueData dialogueData;
            // for (int i = 0; i < dialogueTemplate.characters[0].dialogues_list.Count; i++)
            for (int i = 0; i < 2; i++)
            {
                dialogueData = dialogueTemplate.characters[0].dialogues_list[i];

                tempNode = _targetGraphView.CreateDialogueNode(dialogueData.link.id, dialogueData);
                tempNode.viewDataKey = dialogueData.link.id;
                _targetGraphView.AddElement(tempNode);
            }
        }

#if TEST_TO_JSON
        private void TestToJson()
        {
            string data = "";
            DialogueTemplate dialogueTemplate = new DialogueTemplate();
            dialogueTemplate.characters = new List<CharacterData>();

            CharacterData character = new CharacterData();
            character.character_name = "Test1";
            character.parent_id = "TS";
            character.dialogues_list = new List<DialogueData>();

            DialogueData dialogue = new DialogueData();
            dialogue.dialogue_flags = 124;
            dialogue.dialogue_type = 1;
            dialogue.dialogueLink.dialogue_id = "TS_0";
            dialogue.dialogueLink.next_dialogue_id = "TS_1";
            dialogue.dialogue = "This is a test dialogue";
            character.dialogues_list.Add(dialogue);

            dialogue = new DialogueData();
            dialogue.dialogue_flags = 2667;
            dialogue.dialogue_type = 2;
            dialogue.dialogueLink.dialogue_id = "TS_1";
            dialogue.dialogueLink.next_dialogue_id = "EOC";
            dialogue.dialogue = "This is a test dialogue 2";
            dialogue.choices = new List<DialogueLink>
            {
                new DialogueLink("PL_1", "TS_1_0"),
                new DialogueLink("PL_2", "TS_1_1")
            };
            character.dialogues_list.Add(dialogue);

            dialogueTemplate.characters.Add(character);

            data = JsonUtility.ToJson(dialogueTemplate);
            Debug.Log($"Json Data: {data}");
        }
#endif

#if TEST_BTS
        public void CheckAssetFolder()
        {
            // Debug.Log($"Current Directory: {System.IO.Directory.GetCurrentDirectory()}");

            /*
            string[] paths2 = AssetDatabase.GetSubFolders("Assets");
            Debug.Log($"Paths2 Length: {paths2.Length}");
            for (int i = 0; i < paths2.Length; i++)
            {
                Debug.Log($"i: [{i}] | path: {paths2[i]}");
            }
            */

            // string pathToFolder = System.IO.Path.Join(_PARENT_FOLDER_PATH, _RESOURCES_FOLDER_PATH);
            string pathToFolder = "Assets/Project";
            if (!System.IO.Directory.Exists(pathToFolder))
            {
                Debug.Log($"Resources Folder not found: {pathToFolder}");
                // AssetDatabase.CreateFolder("Assets", "Project");
                return;
            }
            Debug.Log($"Valid Path: {pathToFolder}");

            /*
            string[] paths = AssetDatabase.GetAllAssetPaths();
            int i = 0;
            foreach (var path in paths)
            {
                Debug.Log($"i : [{i}] | path: {path}");
                i++;
            }
            */
        }
#endif
    }
}
#endif