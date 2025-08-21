// #define TEST_BTS
// #define TEST_TO_JSON
// #define TEST_SAVE_JSON_1
// #define DEBUG_SAVE_JSON_FOUND_NODE
#define DISABLE_BACKUP

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor.Experimental.GraphView;

using CurseOfNaga.DialogueSystem.Runtime;
using UnityEditor;
using UnityEngine.UIElements;
using CurseOfNaga.Utils;
using CurseOfNaga.Global;
using System.Threading;

namespace CurseOfNaga.DialogueSystem.Editor
{
    public class GraphSaveUtility
    {
        private DialogueGraphView _targetGraphView;
        private List<Edge> _edges => _targetGraphView.edges.ToList();
        private List<Node> _nodes => _targetGraphView.nodes.ToList();//.Cast<DialogueNode>().ToList();

        private CancellationTokenSource _cts;
        private const int _NODE_BASE_OFFSET = 1;

        ~GraphSaveUtility()
        {
            _cts.Cancel();
        }

        public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
        {
            return new GraphSaveUtility
            {
                _cts = new CancellationTokenSource(),
                _targetGraphView = targetGraphView
            };
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
                    + $"| nodeID: {connectedPorts[i].output.node.viewDataKey.Substring(0, 1)}");
            }
            return;             //TEST
#endif


            int totalCount = connectedPorts.Length;
            int dialogueCount = 0, portCount = 0, leafAddedCount = 0;
            int dIndex = 0, pIndex = 0;
            bool portFound = false;
            int parentID;
            string parentIDStr;
            // int nodeID, nodeIdToCheck;
            // bool nodeExists;

            // Dictionary<string, CharacterData> characters = new Dictionary<string, CharacterData>();
            DialogueTemplate dialogueTemplateToSave = new DialogueTemplate();
            List<CharacterData> charactersList = new List<CharacterData>();
            CharacterData charData;

            Node outputNode, inputNode;

            //Sorting
            int sortCount;
            DialogueData sortKeyDData;
            for (int cpIndex = 0; cpIndex < totalCount; cpIndex++)
            {
                portFound = false;
                outputNode = connectedPorts[cpIndex].output.node;
                inputNode = connectedPorts[cpIndex].input.node;

                if (outputNode.viewDataKey.Equals(DialogueGraphView.BASE_NODE))
                    continue;

                parentIDStr = outputNode.viewDataKey.Substring(0, 3);
                // Extract parent_id of the character to which the DialogueNode belongs
                if (!int.TryParse(parentIDStr, out parentID))
                {
                    EditorUtility.DisplayDialog("ABORTING!", "Failed to save. Check Error", "OK");
                    Debug.LogError($"Bad parentID: {parentIDStr} | viewDataKey: {outputNode.viewDataKey} "
                        + $"| Edge Index: {cpIndex} | Node Name: {outputNode.title}");
                    return;
                }

                // Found a new Character | size offset
                if ((parentID + 1) <= charactersList.Count)
                    charData = charactersList[parentID];
                // Character already exists
                else
                {
                    charData = new CharacterData()
                    {
                        parent_id = parentIDStr,
                        character_name = parentIDStr,
                        dialogues_list = new List<DialogueData>()
                    };
                    charactersList.Add(charData);
                }

#if DEBUG_SAVE_JSON_FOUND_NODE
                Debug.Log($"To Find | cpIndex: [{cpIndex}] "
                + $"| Output ID: {connectedPorts[cpIndex].output.node.viewDataKey}, {connectedPorts[cpIndex].output.node.title} "
                + $"| Input ID: {connectedPorts[cpIndex].input.node.viewDataKey}, {connectedPorts[cpIndex].input.node.title} ");
#endif
                // Find the node which is input of the current edge in the AddedDialogues
                // The DialogueData already has the number of ports added
                // All ports are set at creation to the base_id and name
                // We only need to set the target_id

                dialogueCount = _targetGraphView.AddedDialogues.Count;
                for (dIndex = 0; dIndex < dialogueCount && !portFound; dIndex++)
                {
                    if (_targetGraphView.AddedDialogues[dIndex].base_uid.Equals(outputNode.viewDataKey))
                    {
                        // int.TryParse(_targetGraphView.AddedDialogues[dIndex].base_uid.Substring(6, 3), out nodeID);
                        // int.TryParse(charData.dialogues_list[nodeID].base_uid.Substring(6, 3), out nodeIdToCheck);

                        // Check if the DialogueNode exists in the list as multiple edges can have same Node
                        // List Size Offset
                        // if ((nodeID + 1 + leafAddedCount) > charData.dialogues_list.Count)
                        // if (charData.dialogues_list.Exists((node) => { return node.nodeIndex == nodeID; }))      //Will not work | nodeID is total amount

                        //Find if node is already added
                        int i = 0;
                        for (; i < charData.dialogues_list.Count; i++)
                        {
                            if (charData.dialogues_list[i].nodeIndex
                                == _targetGraphView.AddedDialogues[dIndex].nodeIndex)
                                break;
                        }
                        if (i == charData.dialogues_list.Count)
                        {
                            charData.dialogues_list.Add(_targetGraphView.AddedDialogues[dIndex]);
                            _targetGraphView.AddedDialogues[dIndex].position = outputNode.GetPosition().position.ToVec2Srlz();
                        }

                        /*
                        nodeExists = charData.dialogues_list.Exists((node) =>
                        {
                            int.TryParse(node.base_uid.Substring(6, 3), out nodeIdToCheck);
                            return nodeIdToCheck == nodeID;
                        });
                        if (!nodeExists)
                        {
                            charData.dialogues_list.Add(_targetGraphView.AddedDialogues[dIndex]);
                            _targetGraphView.AddedDialogues[dIndex].position = outputNode.GetPosition().position.ToVec2Srlz();
                        }
                        // */

                        //Fill all the ports of the Node
                        portCount = _targetGraphView.AddedDialogues[dIndex].ports.Count;
                        for (pIndex = 0; pIndex < portCount; pIndex++)
                        {
                            if (_targetGraphView.AddedDialogues[dIndex].ports[pIndex]
                                .target_uid.Equals(UniversalConstant.NOT_SET_STR))
                            {
#if DEBUG_SAVE_JSON_FOUND_NODE
                                Debug.Log($"Found Node in AddedDialogues | dIndex: {dIndex} "
                                    + $"| target_uid: {_targetGraphView.AddedDialogues[dIndex].ports[pIndex].target_uid}"
                                    + $"| target outPut: {inputNode.outputContainer.childCount}"
                                    );
#endif
                                portFound = true;
                                _targetGraphView.AddedDialogues[dIndex].ports[pIndex].target_uid = inputNode.viewDataKey;        //Very Important Ignore previous comment

                                break;
                            }
                        }
                        break;
                    }
                }

                // Keep sorting while adding new
                SortDialoguesList(charData.dialogues_list);

                // Add to Leaf Nodes list for the nodes which do not have a target_uid
                // These nodes do not have an edge | End Nodes connected to previous nodes
                if (inputNode.outputContainer.childCount == 0)
                {
                    for (dIndex = 0; dIndex < dialogueCount; dIndex++)
                    {
                        //Since these nodes dont have a port, no need to look for it as there will be no edge for these
                        if (_targetGraphView.AddedDialogues[dIndex].base_uid.Equals(inputNode.viewDataKey))
                        {
                            leafAddedCount++;
                            charData.dialogues_list.Add(_targetGraphView.AddedDialogues[dIndex]);
                            _targetGraphView.AddedDialogues[dIndex].position = inputNode.GetPosition().position.ToVec2Srlz();
                            break;
                        }
                    }
                }

                // Keep sorting while adding new
                SortDialoguesList(charData.dialogues_list);
            }
            dialogueTemplateToSave.characters = charactersList;

            #region SaveJSON
            // fileName += ".json";
            // const string JSON_EXTENSION = ".json", JSON_BACKUP_EXTENSION = "_bkp.json";
            string pathToJson = System.IO.Path.Join(Application.streamingAssetsPath, fileName);
            Debug.Log($"Saving Json to: {pathToJson + UniversalConstant.JSON_EXTENSION}");

            System.IO.FileStream saveStream = null;
#if DISABLE_BACKUP
            saveStream = System.IO.File.Create(pathToJson + UniversalConstant.JSON_EXTENSION);
#else
            if (!System.IO.File.Exists(pathToJson + UniversalConstant.JSON_EXTENSION))
                saveStream = System.IO.File.Create(pathToJson + UniversalConstant.JSON_EXTENSION);
            else
            {
                if (System.IO.File.Exists(pathToJson + UniversalConstant.JSON_BACKUP_EXTENSION))
                    System.IO.File.Delete(pathToJson + UniversalConstant.JSON_BACKUP_EXTENSION);

                System.IO.File.Move(pathToJson + UniversalConstant.JSON_EXTENSION,
                    pathToJson + UniversalConstant.JSON_BACKUP_EXTENSION);
                saveStream = System.IO.File.Create(pathToJson + UniversalConstant.JSON_EXTENSION);
                // Debug.Log($"Moved to : {pathToJson + UniversalConstant.JSON_BACKUP_EXTENSION}");
                // saveStream = new System.IO.FileStream(pathToJson, System.IO.FileMode.Truncate, System.IO.FileAccess.Write);
            }
#endif

            string dialogueData = JsonUtility.ToJson(dialogueTemplateToSave);
            // Debug.Log($"dialogueData: {dialogueData}"); return;     //TEST
            byte[] dialogueDataInBytes = System.Text.Encoding.ASCII.GetBytes(dialogueData);
            try
            {
                // using (System.IO.StreamWriter writer = new System.IO.StreamWriter(pathToJson + JSON_EXTENSION, false))
                //     await writer.BaseStream.WriteAsync(dialogueDataInBytes, 0, dialogueDataInBytes.Length, _cts.Token);

                await saveStream.WriteAsync(dialogueDataInBytes, 0, dialogueDataInBytes.Length);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error ocurred! | Failed to Save Data to path : "
                        + $"{pathToJson + UniversalConstant.JSON_EXTENSION} | Error : {ex.Message}");
            }
            finally
            {
                saveStream.Close();
            }

            #endregion SaveJSON
        }

        private void SortDialoguesList(List<DialogueData> dialogueList)
        {
            int sortCount = dialogueList.Count - 2;              //2nd last element
            DialogueData sortKeyDData;
            if (sortCount >= 0)
            {
                //Start from last element
                sortKeyDData = dialogueList[sortCount + 1];

                while (sortCount >= 0 && dialogueList[sortCount].nodeIndex > sortKeyDData.nodeIndex)
                {
                    dialogueList[sortCount + 1] = dialogueList[sortCount];
                    sortCount--;
                }
                dialogueList[sortCount + 1] = sortKeyDData;
            }
        }

        public void ClearGraph()
        {
            //Set the base EntryPoint node GUID first. Discard existing guid
            // _nodes.Find(node => node.EntryPoint).GUID = _containerCache.NodeLinks[0].BaseNodeGUID;
            _nodes[0].viewDataKey = DialogueGraphView.BASE_NODE;

            _targetGraphView.TotalReset = true;
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

        public async void LoadDialogueJson(string fileName)
        {
            #region LoadJSON
            // fileName += UniversalConstant.JSON_EXTENSION;
            string pathToJson = System.IO.Path.Join(Application.streamingAssetsPath, fileName + UniversalConstant.JSON_EXTENSION);
            Debug.Log($"Loadiing Json from: {pathToJson}");

            if (!System.IO.File.Exists(pathToJson))
            {
                EditorUtility.DisplayDialog("Invalid file path", "Please enter a valid file path to Dialogues json file", "OK");
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

            GenerateNodes(dialogueTemplate);
            ConnectNodes(dialogueTemplate);
        }

        private void GenerateNodes(DialogueTemplate dialogueTemplate)
        {
            Node targetNode;
            List<DialoguePort> nodePorts;
            int totalLoopCount, charCount;

            DialogueData dialogueData;

            charCount = dialogueTemplate.characters.Count;
            int charIndex = 1, totalNodeCount = 0;
            for (charIndex = 0; charIndex < charCount; charIndex++)
            {
                totalLoopCount = dialogueTemplate.characters[charIndex].dialogues_list.Count;
                // totalLoopCount = 3;

                for (int dialogueIndex = 0; dialogueIndex < totalLoopCount; dialogueIndex++, totalNodeCount++)
                {
                    dialogueData = dialogueTemplate.characters[charIndex].dialogues_list[dialogueIndex];

                    targetNode = _targetGraphView.CreateDialogueNode(dialogueData.base_uid, dialogueData);
                    targetNode.viewDataKey = dialogueData.base_uid;
                    _targetGraphView.AddElement(targetNode);

                    nodePorts = dialogueTemplate.characters[charIndex].dialogues_list[dialogueIndex].ports;
                    for (int portIndex = 0; portIndex < nodePorts.Count; portIndex++)
                        _targetGraphView.AddChoicePort(targetNode, totalNodeCount, nodePorts[portIndex].name, portIndex);
                    // nodePorts.ForEach(edge => _targetGraphView.AddChoicePort(targetNode, dialogueIndex, edge.name));

                }
            }
        }

        private void ConnectNodes(DialogueTemplate dialogueTemplate)
        {
            List<DialoguePort> nodePorts;
            Node targetNode;

            //Connecting BASE NODE to 1st Node
            targetNode = _nodes[1];
            LinkNodesViaEdge(_nodes[0].outputContainer.Q<Port>(), (Port)_nodes[1].inputContainer[0]);
            targetNode.SetPosition(new Rect(dialogueTemplate.characters[0].dialogues_list[0].position.ToVec2(),
                    _targetGraphView._defaultNodeSize));

            int charCount = dialogueTemplate.characters.Count;
            int totalLoopCount, nodeCount = 0;
            int targetCharIndex, targetDialogueIndex;
            for (int charIndex = 0; charIndex < charCount; charIndex++)
            {
                totalLoopCount = dialogueTemplate.characters[charIndex].dialogues_list.Count;
                // totalLoopCount--;

                for (int dialogueIndex = 0; dialogueIndex < totalLoopCount; dialogueIndex++)
                {
                    nodePorts = dialogueTemplate.characters[charIndex].dialogues_list[dialogueIndex].ports;

                    for (int j = 0; j < nodePorts.Count; j++)
                    {
                        var targetNodeGuid = nodePorts[j].target_uid;

                        //Link Node
                        targetNode = _nodes.First(node => node.viewDataKey.Equals(targetNodeGuid));
                        // int tempInt1 = 0;

                        // As we load node from the JSON file, they will be in the order of the loop one after the other
                        //
                        LinkNodesViaEdge(_nodes[nodeCount + _NODE_BASE_OFFSET].outputContainer[j].Q<Port>(),
                            (Port)targetNode.inputContainer[0]);

                        //Set Postion of Node
                        int.TryParse(targetNodeGuid.Substring(0, 3), out targetCharIndex);
                        int.TryParse(targetNodeGuid.Substring(6, 3), out targetDialogueIndex);
                        targetNode.SetPosition(new Rect(
                            dialogueTemplate.characters[targetCharIndex].dialogues_list[targetDialogueIndex].position.ToVec2(),
                            // dialogueTemplate.characters[charIndex].dialogues_list.First(nodeData =>
                            //      nodeData.base_uid.Equals(targetNodeGuid)).position.ToVec2(),
                            _targetGraphView._defaultNodeSize
                        ));
                    }
                    nodeCount++;
                }
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