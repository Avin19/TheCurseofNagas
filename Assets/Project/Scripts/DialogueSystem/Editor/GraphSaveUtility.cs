// #define TEST_BTS
// #define TEST_TO_JSON
// #define TEST_SAVE_JSON_1
#define DEBUG_SAVE_JSON_FOUND_NODE

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
        private const string _LEAF_NODES = "LEAF_NODES";

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

                dialogueContainer.NodePorts.Add(new DialogueNodePortData
                {
                    BaseNodeGUID = outputNode.viewDataKey,
                    PortName = connectedPorts[i].output.name,
                    TargetNodeGUID = inputNode.viewDataKey
                });
            }

            totalCount = _nodes.Count;
            for (int i = 1; i < totalCount; i++)
            {
                dialogueContainer.NodeDatas.Add(new DialogueNodeData
                {
                    GUID = _nodes[i].viewDataKey,
                    // DialogueText = _nodes[i].DialogueText,
                    Position = _nodes[i].GetPosition().position,
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

        public async void SaveDialogueJson(string fileName)
        {
            if (!_edges.Any()) return;       //if there are no edges(no connections) then return

            var connectedPorts = _edges.Where(e => e.input != null).ToArray();


#if TEST_SAVE_JSON_1
            for (int i = 0; i < connectedPorts.Length; i++)
            {
                Debug.Log($"i: [{i}] | Output ID: {connectedPorts[i].output.node.viewDataKey}, {connectedPorts[i].output.node.title} "
                    + $"| Input ID: {connectedPorts[i].input.node.viewDataKey}, {connectedPorts[i].input.node.title} "
                    + $"| nodeID: {connectedPorts[i].output.node.viewDataKey.Substring(0, 4)}");
            }

            return;             //TEST
#endif

            #region SaveJSON
            fileName += ".json";
            string pathToJson = System.IO.Path.Join(Application.streamingAssetsPath, fileName);
            Debug.Log($"Saving Json to: {pathToJson}");

            DialogueTemplate dialogueTemplateToSave = new DialogueTemplate();
            dialogueTemplateToSave.CharactersDict = new Dictionary<string, CharacterData>();
            // Dictionary<string, CharacterData> characters = new Dictionary<string, CharacterData>();
            CharacterData charData;

            // CharactersDict.Add(_LEAF_NODES, new CharacterData()
            // {
            //     parent_id = _LEAF_NODES,
            //     character_name = _LEAF_NODES,
            //     dialogues_list = new List<DialogueData>()
            // });

            int totalCount = connectedPorts.Length;
            int dialogueCount = 0, portCount = 0;
            int dIndex = 0, pIndex = 0;
            bool portFound = false;
            string parentID;
            for (int cpIndex = 0; cpIndex < totalCount; cpIndex++)
            {
                portFound = false;
                var outputNode = connectedPorts[cpIndex].output.node;
                var inputNode = connectedPorts[cpIndex].input.node;

                parentID = outputNode.viewDataKey.Substring(0, 2);
                if (!dialogueTemplateToSave.CharactersDict.ContainsKey(parentID))
                {
                    charData = new CharacterData()
                    {
                        parent_id = parentID,
                        character_name = parentID,
                        dialogues_list = new List<DialogueData>()
                    };
                    dialogueTemplateToSave.CharactersDict.Add(parentID, charData);
                }
                else
                {
                    dialogueTemplateToSave.CharactersDict.TryGetValue(parentID, out charData);
                }

#if DEBUG_SAVE_JSON_FOUND_NODE
                Debug.Log($"To Find | cpIndex: [{cpIndex}] "
                    + $"| Output ID: {connectedPorts[cpIndex].output.node.viewDataKey}, {connectedPorts[cpIndex].output.node.title} "
                    + $"| Input ID: {connectedPorts[cpIndex].input.node.viewDataKey}, {connectedPorts[cpIndex].input.node.title} ");
#endif

                //Find the current port in the AddedDialogues
                dialogueCount = _targetGraphView.AddedDialogues.Count;
                for (dIndex = 0; dIndex < dialogueCount && !portFound; dIndex++)
                {
                    portCount = _targetGraphView.AddedDialogues[dIndex].ports.Count;
                    for (pIndex = 0; pIndex < portCount; pIndex++)
                    {
                        if (_targetGraphView.AddedDialogues[dIndex].ports[pIndex]
                            .base_uid.Equals(outputNode.viewDataKey))
                        {
                            // _targetGraphView.AddedDialogues[dIndex].ports[pIndex].target_uid = inputNode.viewDataKey;        //No point, This will be the same
                            charData.dialogues_list.Add(_targetGraphView.AddedDialogues[dIndex]);

                            // Add to Leaf Nodes list for the nodes which do not have a target_uid
                            if (inputNode.outputContainer.childCount == 0)
                            {

                            }

                            portFound = true;

#if DEBUG_SAVE_JSON_FOUND_NODE
                            Debug.Log($"Found Node in AddedDialogues | dIndex: {dIndex} "
                                + $"| target_uid: {_targetGraphView.AddedDialogues[dIndex].ports[pIndex].target_uid}"
                                + $"| target outPut: {inputNode.outputContainer.childCount}"
                                );
#endif
                            break;
                        }
                    }
                }
                // if (!portFound)
                // {
                //     _targetGraphView.AddedDialogues[dIndex].ports[pIndex].target_uid = null;
                //      charData.dialogues_list.Add(_targetGraphView.AddedDialogues[dIndex]);
                // }


                // var nodeLinks = _targetGraphView.AddedDialogues.Where(node => 
                //          node.ports.Where(port => port.base_uid.Equals(outputNode.viewDataKey)))

                // dialogueContainer.NodePorts.Add(new DialogueNodePortData
                // {
                //     BaseNodeGUID = outputNode.viewDataKey,
                //     PortName = connectedPorts[i].output.name,
                //     TargetNodeGUID = inputNode.viewDataKey
                // });
            }
            return;     //TEST
            // dialogueTemplateToSave.characters = characters.Values.ToList();

            System.IO.FileStream saveStream;
            if (!System.IO.File.Exists(pathToJson))
            {
                saveStream = System.IO.File.Create(pathToJson);
            }
            else
                saveStream = System.IO.File.Open(pathToJson, System.IO.FileMode.Open);

            string dialogueData = JsonUtility.ToJson(dialogueTemplateToSave);
            byte[] dialogueDataInBytes = System.Text.Encoding.ASCII.GetBytes(dialogueData);
            try
            {
                await saveStream.WriteAsync(dialogueDataInBytes, 0, dialogueDataInBytes.Length);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error ocurred! | Failed to Save Data to path : {pathToJson} | Error : {ex.Message}");
            }
            finally
            {
                saveStream.Close();
            }

            #endregion SaveJSON
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

        public void ClearGraph()
        {
            //Set the base EntryPoint node GUID first. Discard existing guid
            // _nodes.Find(node => node.EntryPoint).GUID = _containerCache.NodeLinks[0].BaseNodeGUID;
            _nodes[0].viewDataKey = DialogueGraphView.BASE_NODE;

            for (int i = 1; i < _nodes.Count;)
            {
                // Remove all the edges connected to this graph
                // _edges.Where(edge => edge.input.node == _nodes[i]).ToList()
                //             .ForEach(edge => _targetGraphView.RemoveElement(edge));

                //Then finally remove the node
                _targetGraphView.RemoveElement(_nodes[i]);         //This is connected to the _nodes List
            }

            // Remove all the edges connected to this graph
            for (int i = 0; i < _edges.Count;)
                _targetGraphView.RemoveElement(_edges[i]);

            _targetGraphView.ResetGraph();
        }

        private void GenerateNodes()
        {
            Node tempNode;
            List<DialogueNodePortData> nodePorts;

            for (int i = 0; i < _containerCache.NodeDatas.Count; i++)
            {
                tempNode = _targetGraphView.CreateDialogueNode(_containerCache.NodeDatas[i].GUID);
                tempNode.viewDataKey = _containerCache.NodeDatas[i].GUID;
                _targetGraphView.AddElement(tempNode);

                nodePorts = _containerCache.NodePorts.Where(edge => edge.BaseNodeGUID.Equals(_containerCache.NodeDatas[i].GUID)).ToList();
                nodePorts.ForEach(edge => _targetGraphView.AddChoicePort(tempNode, i, edge.PortName));          //TODO: FIX THISD
            }
        }

        private void ConnectNodes()
        {
            DialogueNodePortData[] connections;
            Node targetNode;

            for (int i = 0; i < _nodes.Count; i++)
            {
                connections = _containerCache.NodePorts.Where(nodeLink => nodeLink.BaseNodeGUID.Equals(_nodes[i].viewDataKey)).ToArray();
                for (int j = 0; j < connections.Length; j++)
                {
                    var targetNodeGuid = connections[j].TargetNodeGUID;
                    targetNode = _nodes.First(node => node.viewDataKey.Equals(targetNodeGuid));
                    LinkNodesViaEdge(_nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                    targetNode.SetPosition(new Rect(
                        _containerCache.NodeDatas.First(nodeData => nodeData.GUID.Equals(targetNodeGuid)).Position,
                        _targetGraphView._defaultNodeSize
                    ));
                }
            }
        }

        private void LinkNodesViaEdge(Port output, Port input)
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
            #region LoadJSON
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
            // Debug.Log($"Dialogue Template: \n{dialogueTemplate} | jsonData: {jsonData}"); return;         //TEST
            #endregion LoadJSON

            ClearGraph();

            Node targetNode;
            List<DialoguePort> nodePorts;
            int totalLoopCount;

            #region CreateNode
            DialogueData dialogueData;
            totalLoopCount = dialogueTemplate.characters[0].dialogues_list.Count;
            // totalLoopCount = 3;
            for (int dialogueIndex = 0; dialogueIndex < totalLoopCount; dialogueIndex++)
            {
                dialogueData = dialogueTemplate.characters[0].dialogues_list[dialogueIndex];

                targetNode = _targetGraphView.CreateDialogueNode(dialogueData.base_uid, dialogueData);
                targetNode.viewDataKey = dialogueData.base_uid;
                _targetGraphView.AddElement(targetNode);

                nodePorts = dialogueTemplate.characters[0].dialogues_list[dialogueIndex].ports;
                for (int portIndex = 0; portIndex < nodePorts.Count; portIndex++)
                    _targetGraphView.AddChoicePort(targetNode, dialogueIndex, nodePorts[portIndex].name, portIndex);
                // nodePorts.ForEach(edge => _targetGraphView.AddChoicePort(targetNode, dialogueIndex, edge.name));

            }
            #endregion CreateNode

            #region ConnectNode

            //Connecting BASE NODE to 1st Node
            targetNode = _nodes[1];
            LinkNodesViaEdge(_nodes[0].outputContainer.Q<Port>(), (Port)_nodes[1].inputContainer[0]);
            targetNode.SetPosition(new Rect(dialogueTemplate.characters[0].dialogues_list[0].position.ToVec2(),
                    _targetGraphView._defaultNodeSize));

            // totalLoopCount--;
            for (int dialogueIndex = 0; dialogueIndex < totalLoopCount; dialogueIndex++)
            {
                nodePorts = dialogueTemplate.characters[0].dialogues_list[dialogueIndex].ports;

                for (int j = 0; j < nodePorts.Count; j++)
                {
                    var targetNodeGuid = nodePorts[j].target_uid;
                    targetNode = _nodes.First(node => node.viewDataKey.Equals(targetNodeGuid));
                    LinkNodesViaEdge(_nodes[dialogueIndex + 1].outputContainer[j].Q<Port>(),
                        (Port)targetNode.inputContainer[0]);

                    targetNode.SetPosition(new Rect(
                        dialogueTemplate.characters[0].dialogues_list.First(nodeData =>
                             nodeData.base_uid.Equals(targetNodeGuid)).position.ToVec2(),
                        _targetGraphView._defaultNodeSize
                    ));
                }
            }
            #endregion ConnectNode
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