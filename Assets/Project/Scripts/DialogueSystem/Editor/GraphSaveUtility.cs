#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor.Experimental.GraphView;

using CurseOfNaga.DialogueSystem.Runtime;
using UnityEditor;
using System;

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
                dialogueContainer.DialogueDatas.Add(new DialogueNodeData
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
            _nodes.Find(x => x.EntryPoint).GUID = _containerCache.NodeLinks[0].BaseNodeGUID;

            for (int i = 1; i < _nodes.Count; i++)
            {
                //Remove all the edges connected to this graph
                _edges.Where(edge => edge.input.node == _nodes[i]).ToList()
                            .ForEach(edge => _targetGraphView.RemoveElement(edge));

                //Then finally remove the node
                _targetGraphView.RemoveElement(_nodes[i]);
            }
        }

        private void GenerateNodes()
        {
            DialogueNode tempNode;
            List<DialogueNodeLinkData> nodePorts;

            for (int i = 0; i < _nodes.Count; i++)
            {
                tempNode = _targetGraphView.CreateDialogueNode(_nodes[i].DialogueText);
                tempNode.GUID = _nodes[i].GUID;
                _targetGraphView.AddElement(tempNode);

                nodePorts = _containerCache.NodeLinks.Where(edge => edge.BaseNodeGUID == _nodes[i].GUID).ToList();
                nodePorts.ForEach(edge => _targetGraphView.AddChoicePort(tempNode, edge.PortName));
            }
        }

        private void ConnectNodes()
        {
            throw new NotImplementedException();
        }


    }
}
#endif