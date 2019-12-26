using System.Collections.Generic;
using UnityEngine;

public abstract class SimulatedObject : MonoBehaviour
{
    //This extra class makes easy had more objects in the same scene sharing components

    #region InEditorVariables
    public float massDamping;
    public float rotationDamping;
    public float relativeDamping;
    public bool isFixed;
    public DebugDrawType debugDraw;
    #endregion

    public PhysicsManager physicManager;

    protected Mesh mesh;
    protected Node[] nodes;
    protected List<Spring> springs = new List<Spring>();
    protected Vector3[] vertices;

    public enum DebugDrawType
    {
        NONE = 0,
        GIZMOS = 1,
        GAME_OBJECTS = 2
    };



    // Use this for initialization
    public abstract void loadData(PhysicsManager manager);

    //Call in update to update mesh
    public abstract void updateMesh();

    //Redraw debug gizmos
    void OnDrawGizmos()
    {
        if (this.debugDraw == SimulatedObject.DebugDrawType.GIZMOS && Application.isPlaying)
        {
            foreach (Node node in this.nodes)
            {
                node.debugGizmos();
            }
            foreach (Spring spring in this.springs)
            {
                spring.debugGizmos();
            }
        }
    }

    //Recalc nodes forces
    protected abstract void recalcVertex();

    //Recalc nodes positions explict
    public virtual void recalcVertexExplicit()
    {
        recalcVertex();
        for (int vertexId = 0; vertexId < this.nodes.Length; ++vertexId)
        {
            if (!nodes[vertexId].isFixed)
            {
                this.nodes[vertexId].position += this.nodes[vertexId].velocity * this.physicManager.deltaTime;
                this.nodes[vertexId].velocity += this.nodes[vertexId].force * this.physicManager.deltaTime / this.nodes[vertexId].mass;
            }
        }
    }

    //Recalc nodes positions symplectic
    public virtual void recalcVertexSymplectic()
    {
        recalcVertex();
        for (int vertexId = 0; vertexId < nodes.Length; ++vertexId)
        {
            if (!nodes[vertexId].isFixed)
            {
                this.nodes[vertexId].velocity += this.nodes[vertexId].force * this.physicManager.deltaTime / this.nodes[vertexId].mass;
                this.nodes[vertexId].position += this.nodes[vertexId].velocity * this.physicManager.deltaTime;
            }
        }
    }
}