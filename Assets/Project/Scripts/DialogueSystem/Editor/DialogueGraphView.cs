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

        private int _nodeCount = 0;

        public DialogueGraphView()
        {
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
                title = "Start"
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

        public DialogueNode CreateDialogueNode(string nodeName, DialogueData dialogueData = null)
        {
            DialogueNode dialogueNode = new DialogueNode();
            if (dialogueData == null)
            {
                dialogueData = new DialogueData();

                // dialogueNode.DialogueText = nodeName;
                // dialogueNode.GUID = Guid.NewGuid().ToString();
                dialogueNode.title = $"GRAPH_NODE_{_nodeCount}";
                dialogueNode.GUID = $"GRAPH_NODE_{_nodeCount}";
                Debug.Log($"_noedCount: {_nodeCount} | GUID: {dialogueNode.GUID}");
                _nodeCount++;
            }
            else
            {
                dialogueNode.title = dialogueData.link.id;
                // dialogueNode.DialogueText = dialogueData.dialogue;
                dialogueNode.GUID = dialogueData.link.id;
            }

            var inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
            inputPort.name = "Input";
            dialogueNode.inputContainer.Add(inputPort);

            var button = new Button(() => { AddChoicePort(dialogueNode); }) { text = "New Choice" };
            dialogueNode.titleContainer.Add(button);

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
                // dialogueNode.DialogueText = evt.newValue;
                // dialogueNode.title = evt.newValue;
            });
            // dialogueField.SetValueWithoutNotify(dialogueNode.title);
            dialogueNode.mainContainer.Add(dialogueField);

            // var foldoutField = new Foldout() { text = "Dialogue" };
            // foldoutField.Add(dialogueField);
            // dialogueNode.mainContainer.Add(foldoutField);

            dialogueNode.RefreshExpandedState();
            dialogueNode.RefreshPorts();

            dialogueNode.SetPosition(new Rect(Vector2.zero, _defaultNodeSize));

            return dialogueNode;
        }

        public void AddChoicePort(DialogueNode dialogueNode, string overridenPortName = "")
        {
            var generatedPort = GeneratePort(dialogueNode, Direction.Output);

            var oldLabel = generatedPort.contentContainer.Q<Label>("type");
            generatedPort.contentContainer.Remove(oldLabel);

            var outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;
            var choicePortName = string.IsNullOrEmpty(overridenPortName) ?
                    $"Choice {outputPortCount + 1}" : overridenPortName;

            var choiceTextField = new TextField
            {
                name = string.Empty,
                value = choicePortName
            };
            choiceTextField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
            generatedPort.contentContainer.Add(new Label("  "));
            generatedPort.contentContainer.Add(choiceTextField);

            var deleteBt = new Button(() => RemovePort(dialogueNode, generatedPort)) { text = "X" };
            generatedPort.contentContainer.Add(deleteBt);

            generatedPort.portName = choicePortName;
            dialogueNode.outputContainer.Add(generatedPort);
            dialogueNode.RefreshExpandedState();
            dialogueNode.RefreshPorts();
        }

        private void RemovePort(DialogueNode dialogueNode, Port generatedPort)
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