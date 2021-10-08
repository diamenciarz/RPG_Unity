using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

public class DialogueNode : BaseNode
{

    [Input] public int entry;
    [Input] public int enablingEvents;
    [Input] public int disablingEvents;
    //Outputs
    [Output] public int Choice1;
    [Output] public int Choice2;
    [Output] public int Choice3;
    [Output] public int Choice4;
    [Output] public int Choice5;
    [Output] public int Choice6;

    public string speakerName;
    [TextArea(3, 100)]
    public string dialogue;
    [Header("Player's Answers")]
    public List<string> choiceTextMessages;

    //Private variables

    public override string GetString()
    {
        string returnString = "DialogueNode/" + speakerName + "/" + dialogue + "/";
        foreach (string text in choiceTextMessages)
        {
            returnString += (text + "/");
        }
        return returnString;
    }

    //Get enabled outputs block start
    public List<NodePort> GetEnabledConnectedOutputs(BaseNode node)
    {
        List<NodePort> outputList = GetConnectedOutputs(node);

        DeleteDisabledOutputsFromList(outputList);

        return outputList;
    }
    private void DeleteDisabledOutputsFromList(List<NodePort> outputList)
    {
        List<NodePort> portsToDelete = new List<NodePort>();
        foreach (NodePort port in outputList)
        {

            if (!IsNodeEnabled(port.Connection.node as BaseNode))
            {
                portsToDelete.Add(port);
            }
        }
        foreach (NodePort portToDelete in portsToDelete)
        {
            outputList.Remove(portToDelete);
        }

    }
    private bool IsNodeEnabled(BaseNode nodeToCheck)
    {
        List<NodePort> nodeInputList = GetConnectedInputs(nodeToCheck);
        //Debug.Log("Testing enabled. Input count:" + nodeInputList.Count + "Node name:" + nodeToCheck.name);
        foreach (NodePort port in nodeInputList)
        {
            if (port.fieldName == "enablingEvents")
            {
                ActionNode enablingEventActionNode = FindPreviousNodeUsingInputPortName(nodeToCheck, "enablingEvents") as ActionNode;
                if (!DoesCalledEventListContainAll(enablingEventActionNode.eventList))
                {
                    Debug.Log("Enabling event lacking");
                    return false;
                }
            }
            if (port.fieldName == "disablingEvents")
            {
                ActionNode disablingEventActionNode = FindPreviousNodeUsingInputPortName(nodeToCheck, "disablingEvents") as ActionNode;
                if (!DoesCalledEventListContainAtLeastOne(disablingEventActionNode.eventList))
                {
                    Debug.Log("One disabling event too many");
                    return false;
                }
            }
        }

        return true;
    }
    private BaseNode FindPreviousNodeUsingInputPortName(BaseNode currentNode, string outputName)
    {
        foreach (NodePort nodePort in currentNode.Inputs)
        {
            bool thisPortIsConnectedToSomeNode = nodePort.Connection != null;
            if (thisPortIsConnectedToSomeNode)
            {
                bool thisIsThePortIAmLookingFor = nodePort.fieldName == outputName;
                if (thisIsThePortIAmLookingFor)
                {
                    BaseNode returnNode = nodePort.Connection.node as BaseNode;
                    return returnNode;
                }
            }
        }
        return null;
    }
    private bool DoesCalledEventListContainAll(List<string> events)
    {
        foreach (string element in events)
        {
            NodeReader nodeReader = FindObjectOfType<NodeReader>();
            bool containsThisEvent = nodeReader.calledEventList.Contains(element);
            if (!containsThisEvent)
            {
                return false;
            }
        }
        return true;
    }
    private bool DoesCalledEventListContainAtLeastOne(List<string> events)
    {
        foreach (string element in events)
        {
            NodeReader nodeReader = FindObjectOfType<NodeReader>();
            bool containsThisEvent = nodeReader.calledEventList.Contains(element);
            if (containsThisEvent)
            {
                return true;
            }
        }
        return false;
    }
    public List<NodePort> GetConnectedOutputs(BaseNode node)
    {
        List<NodePort> outputList = node.Outputs.ToList();
        DeleteDisconnectedPortsFromList(outputList);
        return outputList;
    }
    private void DeleteDisconnectedPortsFromList(List<NodePort> outputCollection)
    {
        List<NodePort> savePortsToDelete = new List<NodePort>();
        foreach (NodePort port in outputCollection)
        {
            bool thisPortIsDisconnected = port.Connection == null;
            if (thisPortIsDisconnected)
            {
                savePortsToDelete.Add(port);
            }
        }
        foreach (NodePort portToDelete in savePortsToDelete)
        {
            outputCollection.Remove(portToDelete);
        }

    }
    private List<NodePort> GetConnectedInputs(BaseNode node)
    {
        List<NodePort> inputList = node.Inputs.ToList();
        DeleteDisconnectedPortsFromList(inputList);
        return inputList;
    }
    //Get enabled outputs block end
}