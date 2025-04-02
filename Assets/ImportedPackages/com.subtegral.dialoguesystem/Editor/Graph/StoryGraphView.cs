using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.Client.Differences.Graphic;
using Subtegral.DialogueSystem.DataContainers;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace Subtegral.DialogueSystem.Editor
{
    public class StoryGraphView : GraphView
    {
        public readonly Vector2 DefaultNodeSize = new Vector2(200, 150);
        public readonly Vector2 DefaultCommentBlockSize = new Vector2(300, 200);
        public DialogueNode EntryPointNode;
        public List<DialogueNode> EndingNodes;
        public Blackboard Blackboard = new Blackboard();
        public List<ExposedProperty> ExposedProperties { get; private set; } = new List<ExposedProperty>();
        private NodeSearchWindow _searchWindow;

        public StoryGraphView(StoryGraph editorWindow)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("NarrativeGraph"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddElement(GetEntryPointNodeInstance());
            AddElement(GetEndingNodeInstance(1));
            AddElement(GetEndingNodeInstance(2));

            AddSearchWindow(editorWindow);
        }


        private void AddSearchWindow(StoryGraph editorWindow)
        {
            _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            _searchWindow.Configure(editorWindow, this);
            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }


        public void ClearBlackBoardAndExposedProperties()
        {
            ExposedProperties.Clear();
            Blackboard.Clear();
        }

        public Group CreateCommentBlock(Rect rect, CommentBlockData commentBlockData = null)
        {
            if (commentBlockData == null)
                commentBlockData = new CommentBlockData();
            var group = new Group
            {
                autoUpdateGeometry = true,
                title = commentBlockData.Title
            };
            AddElement(group);
            group.SetPosition(rect);
            return group;
        }

        public void AddPropertyToBlackBoard(ExposedProperty property, bool loadMode = false)
        {
            var localPropertyName = property.PropertyName;
            var localPropertyValue = property.PropertyValue;
            if (!loadMode)
            {
                while (ExposedProperties.Any(x => x.PropertyName == localPropertyName))
                    localPropertyName = $"{localPropertyName}(1)";
            }

            var item = ExposedProperty.CreateInstance();
            item.PropertyName = localPropertyName;
            item.PropertyValue = localPropertyValue;
            ExposedProperties.Add(item);

            var container = new VisualElement();
            var field = new BlackboardField { text = localPropertyName, typeText = "string" };
            container.Add(field);

            var propertyValueTextField = new TextField("Value:")
            {
                value = localPropertyValue
            };
            propertyValueTextField.RegisterValueChangedCallback(evt =>
            {
                var index = ExposedProperties.FindIndex(x => x.PropertyName == item.PropertyName);
                ExposedProperties[index].PropertyValue = evt.newValue;
            });
            var sa = new BlackboardRow(field, propertyValueTextField);
            container.Add(sa);
            Blackboard.Add(container);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            var startPortView = startPort;

            ports.ForEach((port) =>
            {
                var portView = port;
                if (startPortView != portView && startPortView.node != portView.node)
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        public void CreateNewDialogueNode(string nodeName, Vector2 position)
        {
            AddElement(CreateNode(nodeName, 0, DialogueNodeData.EmotionOverride.None, DialogueNodeData.LoveAmount.None, position));
        }

        public void CreateNewMonologueNode(string nodeName, Vector2 position)
        {
            AddElement(CreateNode(nodeName, 0, DialogueNodeData.EmotionOverride.None, DialogueNodeData.LoveAmount.None, position, true));
        }

        public DialogueNode CreateNode(string nodeName, int loveValue,DialogueNodeData.EmotionOverride emotionOverride, DialogueNodeData.LoveAmount loveAmount, Vector2 position, bool isMono = false)
        {
            var tempDialogueNode = new DialogueNode()
            {
                title = nodeName,
                DialogueText = nodeName,
                emotionOverride = emotionOverride,
                loveAmount = loveAmount,
                LoveValue = loveValue,
                GUID = Guid.NewGuid().ToString()
            };
            tempDialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            var inputPort = GetPortInstance(tempDialogueNode, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            tempDialogueNode.inputContainer.Add(inputPort);
            tempDialogueNode.RefreshExpandedState();
            tempDialogueNode.RefreshPorts();
            tempDialogueNode.SetPosition(new Rect(position,
                DefaultNodeSize)); //To-Do: implement screen center instantiation positioning

            tempDialogueNode.style.flexWrap = Wrap.Wrap;


            var textField = new TextField("Dialogue :");
            textField.RegisterValueChangedCallback(evt =>
            {
                tempDialogueNode.DialogueText = evt.newValue;
                tempDialogueNode.title = evt.newValue;
            });
            textField.SetValueWithoutNotify(tempDialogueNode.title);
            tempDialogueNode.mainContainer.Add(textField);
            FormatField(textField);

            EnumField enumField = new EnumField("Emotion Override", defaultValue: DialogueNodeData.EmotionOverride.Idle);
            enumField.RegisterValueChangedCallback(evt =>
            {
                tempDialogueNode.emotionOverride = (DialogueNodeData.EmotionOverride)evt.newValue;
            });
            enumField.SetValueWithoutNotify(tempDialogueNode.emotionOverride);
            tempDialogueNode.mainContainer.Add(enumField);
            FormatField(enumField);

            EnumField enumField2 = new EnumField("Love Amount", defaultValue: DialogueNodeData.LoveAmount.None);
            enumField2.RegisterValueChangedCallback(evt =>
            {
                tempDialogueNode.loveAmount = (DialogueNodeData.LoveAmount)evt.newValue;
            });
            enumField2.SetValueWithoutNotify(tempDialogueNode.loveAmount);
            tempDialogueNode.mainContainer.Add(enumField2);
            FormatField(enumField2);

            //IntegerField loveField = new IntegerField("Love Value :");
            //loveField.RegisterValueChangedCallback(evt =>
            //{
            //    tempDialogueNode.LoveValue = evt.newValue;
            //});
            //loveField.SetValueWithoutNotify(tempDialogueNode.LoveValue);
            //tempDialogueNode.mainContainer.Add(loveField);
            //FormatField(loveField);

            if (isMono)
            {
                var generatedPort = GetPortInstance(tempDialogueNode, Direction.Output);
                generatedPort.portName = "Output";
                tempDialogueNode.outputContainer.Add(generatedPort);
                tempDialogueNode.RefreshExpandedState();
                tempDialogueNode.RefreshPorts();

            }
            else
            {
                var button = new Button(() => { AddChoicePort(tempDialogueNode); })
                {
                    text = "Add Choice",
                };
                tempDialogueNode.titleButtonContainer.Add(button);
            }
            return tempDialogueNode;
        }

        private void FormatField(TextField field)
        {
            field.style.flexDirection = FlexDirection.Row;
            field.style.alignItems.Equals("left");
            field.style.alignContent.Equals("left");
            field.style.color = Color.blue;
        }

        //private void FormatField(IntegerField field)
        //{
        //    field.style.flexDirection = FlexDirection.Row;
        //    field.style.alignItems.Equals("left");
        //    field.style.alignContent.Equals("left");
        //    field.style.color = Color.blue;
        //}

        private void FormatField(EnumField field)
        {
            field.style.flexDirection = FlexDirection.Row;
            field.style.alignItems.Equals("left");
            field.style.alignContent.Equals("left");
            field.style.color = Color.blue;
        }


        public void AddChoicePort(DialogueNode nodeCache, string overriddenPortName = "")
        {
            var generatedPort = GetPortInstance(nodeCache, Direction.Output);
            var portLabel = generatedPort.contentContainer.Q<Label>("type");
            generatedPort.contentContainer.Remove(portLabel);

            var outputPortCount = nodeCache.outputContainer.Query("connector").ToList().Count();
            var outputPortName = string.IsNullOrEmpty(overriddenPortName)
                ? $"Option {outputPortCount + 1}"
                : overriddenPortName;


            var textField = new TextField()
            {
                name = string.Empty,
                value = outputPortName,
            };
            textField.style.flexDirection = FlexDirection.Column;
            textField.style.flexWrap = Wrap.WrapReverse;
            textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
            generatedPort.contentContainer.Add(new Label("  "));
            generatedPort.contentContainer.Add(textField);
            var deleteButton = new Button(() => RemovePort(nodeCache, generatedPort))
            {
                text = "X"
            };
            generatedPort.contentContainer.Add(deleteButton);
            generatedPort.portName = outputPortName;
            nodeCache.outputContainer.Add(generatedPort);
            nodeCache.RefreshPorts();
            nodeCache.RefreshExpandedState();
        }

        private void RemovePort(Node node, Port socket)
        {
            var targetEdge = edges.ToList()
                .Where(x => x.output.portName == socket.portName && x.output.node == socket.node);
            if (targetEdge.Any())
            {
                var edge = targetEdge.First();
                edge.input.Disconnect(edge);
                RemoveElement(targetEdge.First());
            }

            node.outputContainer.Remove(socket);
            node.RefreshPorts();
            node.RefreshExpandedState();
        }

        private Port GetPortInstance(DialogueNode node, Direction nodeDirection,
            Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, nodeDirection, capacity, typeof(float));
        }

        private DialogueNode GetEntryPointNodeInstance()
        {
            var nodeCache = new DialogueNode()
            {
                title = "START",
                GUID = Guid.NewGuid().ToString(),
                DialogueText = "ENTRYPOINT",
                EntryPoint = true
            };

            var generatedPort = GetPortInstance(nodeCache, Direction.Output);
            generatedPort.portName = "Next";
            nodeCache.outputContainer.Add(generatedPort);

            nodeCache.capabilities &= ~Capabilities.Movable;
            nodeCache.capabilities &= ~Capabilities.Deletable;

            nodeCache.RefreshExpandedState();
            nodeCache.RefreshPorts();
            nodeCache.SetPosition(new Rect(100, 200, 100, 150));
            return nodeCache;
        }

        private DialogueNode GetEndingNodeInstance(int endingInt)
        {
            var nodeCache = new EndingNode()
            {
                title = $"Ending{endingInt}",
                GUID = Guid.NewGuid().ToString(),
                DialogueText = $"Ending {endingInt}",
                EntryPoint = true,
                EntryNodeID = endingInt
            };

            var generatedPort = GetPortInstance(nodeCache, Direction.Output);
            generatedPort.portName = "Next";
            nodeCache.outputContainer.Add(generatedPort);

            nodeCache.capabilities &= ~Capabilities.Movable;
            nodeCache.capabilities &= ~Capabilities.Deletable;

            nodeCache.RefreshExpandedState();
            nodeCache.RefreshPorts();
            nodeCache.SetPosition(new Rect(100, 200 + 200 * endingInt, 100, 150));
            return nodeCache;
        }
    }
}