using UnityEngine;
using System.Collections;

public class PhysicVertex
{

    //Class with a weigtened nodes to update a mesh vertex

    #region InEditorVariables
    #endregion

    private float[] weights;
    private Node[] nodes;

    public PhysicVertex()
    {
        this.weights = new float[4];
        this.nodes = new Node[4];
    }

    //Returns the new vertex position
    public Vector3 recalcPos()
    {
        Vector3 newPos = Vector3.zero;
        for (int nodeId = 0; nodeId < 4; nodeId++)
        {
            newPos += this.weights[nodeId] * this.nodes[nodeId].getPos();
        }
        return newPos;
    }

    //set a new weighted node
    public void setWeight(int nodeId, float newWeight, Node node)
    {
        this.weights[nodeId] = newWeight;
        this.nodes[nodeId] = node;
    }

    public override string ToString()
    {
        string returnString = "PhysicVertex: { ";
        for (int i = 0; i < 4; ++i){
            returnString += " node: " + this.nodes[i] + " weight: " + this.weights[i]; 
            }
            return returnString +"}";
    }
}