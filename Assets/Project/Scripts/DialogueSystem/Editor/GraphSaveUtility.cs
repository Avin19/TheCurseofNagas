#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor.Experimental.GraphView;

using CurseOfNaga.DialogueSystem.Runtime;
using UnityEditor;
using System;
using UnityEngine.UIElements;

namespace CurseOfNaga.DialogueSystem.Editor
{
    public class GraphSaveUtility
    {
        private DialogueGraphView _targetGraphView;
        private DialogueContainer _containerCache;
        private List<Edge> _edges => _targetGraphView.edges.ToList();
        private List<DialogueNode> _nodes => _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

        private const string _PARENT_FOLDER_PATH = "Assets/Project";
        private const string _RESOURCES_FOLDER_PATH = "Resources";

        public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
        {
            return new GraphSaveUtility
            {
                _targetGraphView = targetGraphView
            };
        }

        public void SaveGraph(string fileName)
        {
            if (!_edges.Any()) return;       //if there are no edges(no connections) then return
            Debug.Log($"Saving Graph");

            var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
            var connectedPorts = _edges.Where(e => e.input.node != null).ToArray();

            for (int i = 0; i < connectedPorts.Length; i++)
            {
                var outputNode = connectedPorts[i].output.node as DialogueNode;
                var inputNode = connectedPorts[i].input.node as DialogueNode;

                dialogueContainer.NodeLinks.Add(new DialogueNodeLinkData
                {
                    BaseNodeGUID = outputNode.GUID,
                    PortName = connectedPorts[i].output.name,
                    TargetNodeGUID = inputNode.GUID
                });
            }

            foreach (var dialogueNode in _nodes.Where(node => !node.EntryPoint))
            {
                dialogueContainer.DialogueNodeDatas.Add(new DialogueNodeData
                {
                    GUID = dialogueNode.GUID,
                    DialogueText = dialogueNode.DialogueText,
                    Position = dialogueNode.GetPosition().position
                });
            }

            //Check if the Reources folder exists in the project or not
            if (AssetDatabase.IsValidFolder(_PARENT_FOLDER_PATH + "/" + _RESOURCES_FOLDER_PATH))
                AssetDatabase.CreateFolder(_PARENT_FOLDER_PATH, _RESOURCES_FOLDER_PATH);

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
            _nodes.Find(node => node.EntryPoint).GUID = _containerCache.NodeLinks[0].BaseNodeGUID;

            for (int i = 1; i < _nodes.Count;)
            {
                // Remove all the edges connected to this graph
                _edges.Where(edge => edge.input.node == _nodes[i]).ToList()
                            .ForEach(edge => _targetGraphView.RemoveElement(edge));         //This is connected to the _nodes List

                //Then finally remove the node
                _targetGraphView.RemoveElement(_nodes[i]);
            }
        }

        private void GenerateNodes()
        {
            DialogueNode tempNode;
            List<DialogueNodeLinkData> nodePorts;

            for (int i = 0; i < _containerCache.DialogueNodeDatas.Count; i++)
            {
                tempNode = _targetGraphView.CreateDialogueNode(_containerCache.DialogueNodeDatas[i].DialogueText);
                tempNode.GUID = _containerCache.DialogueNodeDatas[i].GUID;
                _targetGraphView.AddElement(tempNode);

                nodePorts = _containerCache.NodeLinks.Where(edge => edge.BaseNodeGUID.Equals(_containerCache.DialogueNodeDatas[i].GUID)).ToList();
                nodePorts.ForEach(edge => _targetGraphView.AddChoicePort(tempNode, edge.PortName));
            }
        }

        private void ConnectNodes()
        {
            DialogueNodeLinkData[] connections;
            DialogueNode targetNode;

            for (int i = 0; i < _nodes.Count; i++)
            {
                connections = _containerCache.NodeLinks.Where(nodeLink => nodeLink.BaseNodeGUID.Equals(_nodes[i].GUID)).ToArray();
                for (int j = 0; j < connections.Length; j++)
                {
                    var targetNodeGuid = connections[j].TargetNodeGUID;
                    targetNode = _nodes.First(node => node.GUID.Equals(targetNodeGuid));
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
    }
}
#endif