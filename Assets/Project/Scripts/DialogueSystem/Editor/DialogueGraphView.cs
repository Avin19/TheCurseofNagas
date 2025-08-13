#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using CurseOfNaga.DialogueSystem.Runtime;

namespace CurseOfNaga.DialogueSystem.Editor
{
    public class DialogueGraphView : GraphView
    {
        public readonly Vector2 _defaultNodeSize = new Vector2(150, 200);
        private const string _NOT_SET = "NOT_SET";
        public const string BASE_NODE = "BASE_NODE";

        public List<DialogueData> AddedDialogues;
        private int _nodeCount = 0;

        public DialogueGraphView()
        {
            AddedDialogues = new List<DialogueData>();

            styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraphEditor"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddElement(GenerateEntryPointNode());
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node)
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        private Port GeneratePort(Node node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));        //Arbitrary type
        }

        private Node GenerateEntryPointNode()
        {
            // var node = new DialogueNode
            // {
            //     title = "Start",
            //     GUID = BASE_NODE,
            //     // GUID = Guid.NewGuid().ToString(),
            //     // DialogueText = "BANANA",
            //     // EntryPoint = true
            // };

            var node = new Node
            {
                title = "Start",
                viewDataKey = BASE_NODE
            };

            var generatedPort = GeneratePort(node, Direction.Output);
            generatedPort.portName = "Next";
            node.outputContainer.Add(generatedPort);

            node.capabilities = ~Capabilities.Movable;

            node.RefreshExpandedState();
            node.RefreshPorts();

            node.SetPosition(new Rect(100, 200, 100, 150));

            return node;
        }

        public void CreateNode(string nodeName)
        {
            AddElement(CreateDialogueNode(nodeName));
        }

        public void ResetGraph()
        {
            _nodeCount = 0;
            AddedDialogues.Clear();
        }

        private void RemoveNodeFromList(DetachFromPanelEvent detachEvent, int nodeIndex)
        {
            // AddedDialogues.RemoveAt(nodeIndex);
            AddedDialogues[nodeIndex] = null;
        }

        //TODO: Update DialogueData only when the user clicks on the save button
        public Node CreateDialogueNode(string nodeName, DialogueData dialogueData = null)
        {
            Node dialogueNode = new Node();
            if (dialogueData == null)
            {
                dialogueData = new DialogueData();
                dialogueData.nodeIndex = _nodeCount;

                // dialogueNode.DialogueText = nodeName;
                // dialogueNode.GUID = Guid.NewGuid().ToString();
                dialogueNode.title = $"GRAPH_NODE_{_nodeCount}";
                dialogueNode.viewDataKey = $"GRAPH_NODE_{_nodeCount}";
                // Debug.Log($"_noedCount: {_nodeCount} | GUID: {dialogueNode.viewDataKey}");
                _nodeCount++;
            }
            else
            {
                dialogueNode.title = dialogueData.base_uid;
                // dialogueNode.DialogueText = dialogueData.dialogue;
                dialogueNode.viewDataKey = dialogueData.base_uid;
            }
            dialogueNode.RegisterCallback<DetachFromPanelEvent>((evt) =>
            {
                RemoveNodeFromList(evt, dialogueData.nodeIndex);
            });
            AddedDialogues.Add(dialogueData);
            dialogueData.base_uid = dialogueNode.viewDataKey;

            var inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
            inputPort.name = "Input";
            dialogueNode.inputContainer.Add(inputPort);

            var newChoiceBt = new Button(() =>
            {
                AddChoicePort(dialogueNode, dialogueData.nodeIndex);
                // AddChoicePort(dialogueNode);
            });
            newChoiceBt.text = "New Choice";
            dialogueNode.titleContainer.Add(newChoiceBt);

            var intField = new IntegerField
            {
                label = "Flags",
                value = dialogueData.flags
            };
            intField.RegisterValueChangedCallback(evt => { dialogueData.flags = evt.newValue; });
            dialogueNode.mainContainer.Add(intField);

            intField = new IntegerField
            {
                label = "Type",
                value = dialogueData.type
            };
            intField.RegisterValueChangedCallback(evt => { dialogueData.type = evt.newValue; });
            dialogueNode.mainContainer.Add(intField);

            var dialogueField = new TextField
            {
                label = "Dialogue",
                value = dialogueData.dialogue
            };
            dialogueField.RegisterValueChangedCallback(evt =>
            {
                dialogueData.dialogue = evt.newValue;
            });
            // dialogueField.SetValueWithoutNotify(dialogueNode.title);
            dialogueNode.mainContainer.Add(dialogueField);

            dialogueField = new TextField
            {
                label = "ID",
                value = dialogueData.base_uid
            };
            dialogueField.RegisterValueChangedCallback(evt =>
            {
                dialogueData.base_uid = evt.newValue;
                dialogueNode.title = evt.newValue;
            });
            dialogueField.SetValueWithoutNotify(dialogueNode.title);
            dialogueNode.mainContainer.Add(dialogueField);

            // var foldoutField = new Foldout() { text = "Dialogue" };
            // foldoutField.Add(dialogueField);
            // dialogueNode.mainContainer.Add(foldoutField);

            dialogueNode.RefreshExpandedState();
            dialogueNode.RefreshPorts();

            dialogueNode.SetPosition(new Rect(Vector2.zero, _defaultNodeSize));

            return dialogueNode;
        }

        //TODO: Can offset the UID on the name to contain both the Base UID and the name in one place
        public void AddChoicePort(Node dialogueNode, int nodeIndex, string overridenPortName = "")
        // public void AddChoicePort(Node dialogueNode, string overridenPortName = "")
        {
            var generatedPort = GeneratePort(dialogueNode, Direction.Output);

            var oldLabel = generatedPort.contentContainer.Q<Label>("type");
            generatedPort.contentContainer.Remove(oldLabel);

            var outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;
            var choicePortName = string.IsNullOrEmpty(overridenPortName) ?
                    $"Choice {outputPortCount + 1}" : overridenPortName;

            int portCount = AddedDialogues[nodeIndex].ports.Count;
            DialoguePort choicePort = new DialoguePort
            {
                base_uid = AddedDialogues[nodeIndex].base_uid,
                name = choicePortName,
                target_uid = _NOT_SET
            };
            AddedDialogues[nodeIndex].ports.Add(choicePort);

            var choiceTextField = new TextField
            {
                name = string.Empty,
                value = choicePortName
            };
            choiceTextField.RegisterValueChangedCallback(evt =>
            {
                generatedPort.portName = evt.newValue;
                choicePort.name = evt.newValue;
            });
            generatedPort.contentContainer.Add(new Label("  "));
            generatedPort.contentContainer.Add(choiceTextField);

            var deleteBt = new Button(() => RemovePort(dialogueNode, generatedPort)) { text = "X" };
            generatedPort.contentContainer.Add(deleteBt);

            generatedPort.portName = choicePortName;
            dialogueNode.outputContainer.Add(generatedPort);
            dialogueNode.RefreshExpandedState();
            dialogueNode.RefreshPorts();
        }

        private void RemovePort(Node dialogueNode, Port generatedPort)
        {
            var targetEdge = edges.ToList().Where(edge => edge.output.portName == generatedPort.portName
                    && edge.output.node == generatedPort.node);

            // Debug.Log($"targetEdge | input: {targetEdge.First().input.node.viewDataKey} "
            //     + $"| output: {targetEdge.First().output.node.viewDataKey}");
            if (targetEdge.Count() > 0)
            {
                var edge = targetEdge.First();
                edge.input.Disconnect(edge);
                RemoveElement(edge);
            }

            dialogueNode.outputContainer.Remove(generatedPort);
            dialogueNode.RefreshPorts();
            dialogueNode.RefreshExpandedState();
        }
    }
}
#endif