using System.Collections.Generic;
using UnityEngine;

public class Cloth : SimulatedObject
{
    #region InEditorVariables
    public float stiffness;
    public float antiStiffness;
    public float mass;
    #endregion

    //This extra class makes easy had more fabrics in the same scene sharing components

    // Use this for initialization
    public override void loadData(PhysicsManager physicManager_)
    {
        this.physicManager = physicManager_;
        SortedDictionary<Edge, List<int>> edgesMap = new SortedDictionary<Edge, List<int>>();
        this.mesh = transform.GetComponent<MeshFilter>().mesh;
        this.vertices = this.mesh.vertices;
        this.nodes = new Node[this.vertices.Length];
        for (int vertexId = 0; vertexId < this.vertices.Length; ++vertexId)
        {
            //iterates thought all mesh vertices
            float nodeMass = this.mass;
            float nodeDamping = this.massDamping;
            bool nodeIsFixed = this.isFixed;
            Vector3 globalVertexPos = transform.TransformPoint(this.vertices[vertexId]);
            foreach (PropertiesDefineZone propertiesZone in this.physicManager.propertiesZones)
            {//checks if the node its in a propertiesDefineZone				
                if (propertiesZone.bound.Contains(globalVertexPos))
                {
                    nodeIsFixed = propertiesZone.isFixed;
                    nodeMass = propertiesZone.mass;
                    nodeDamping = propertiesZone.massDamping;
                    break;
                }
            }
            //Creates and stores the new node;
            this.nodes[vertexId] = new Node(globalVertexPos, this.physicManager, nodeMass, nodeDamping, nodeIsFixed, debugDraw);
        }
        for (int triangleId = 0; triangleId < this.mesh.triangles.Length; triangleId += 3)//Iterates through all the triangles
        {
            int[] triangleVertex = new int[3];
            triangleVertex[0] = mesh.triangles[triangleId];
            triangleVertex[1] = mesh.triangles[triangleId + 1];
            triangleVertex[2] = mesh.triangles[triangleId + 2];

            for (int vertexId = 0; vertexId < 3; ++vertexId)
            {//creates 3 new edges for each triangle
                Edge triangleEdge = new Edge(triangleVertex[vertexId], triangleVertex[(vertexId + 1) % 3]);
                if (edgesMap.ContainsKey(triangleEdge))
                {//checks if the edge already exist
                    List<int> storedVertices;
                    edgesMap.TryGetValue(triangleEdge, out storedVertices);//obtain all triangles that use that mesh
                    if (!storedVertices.Contains(triangleVertex[(vertexId + 2) % 3]))
                    {//checks for repetitive triangles						
                        foreach (int storedVertex in storedVertices)
                        {//creates a new spring between this triangle and all the other triangles
                            float springStiffness = this.antiStiffness;
                            float rotationDamping = this.rotationDamping;
                            float projectiveDamping = this.relativeDamping;
                            Vector3 containedVertex = transform.TransformPoint(0.5f * (vertices[storedVertex] - vertices[triangleVertex[(vertexId + 2) % 3]]));
                            foreach (PropertiesDefineZone propertiesZone in physicManager.propertiesZones)
                            {// checks if the new spring its in a propertie define zone
                                if (propertiesZone.bound.Contains(containedVertex))
                                {
                                    springStiffness = propertiesZone.antiStiffness;
                                    rotationDamping = propertiesZone.rotationDamping;
                                    projectiveDamping = propertiesZone.projectiveDamping;
                                    break;
                                }
                            }
                            //Creates and stores the new spring between the opposite triangle vertex, this way avoid creating repetitive springs
                            springs.Add(new Spring(nodes[storedVertex], nodes[triangleVertex[(vertexId + 2) % 3]], springStiffness, rotationDamping, projectiveDamping, debugDraw));
                        }
                        storedVertices.Add(triangleVertex[(vertexId + 2) % 3]);//Adds the new vertex to the opposite triangles vertex of the spring
                    }
                    else
                    {
                        print("ERROR: triangle duplicated");//shows an error of repetitive triangles
                    }
                }
                else//if the edge its not cointain creates a new edge and adds it to the dictionary 
                {
                    float springStiffness = this.stiffness;
                    float dampingOfRotation = this.rotationDamping;
                    float dampingOfProjective = this.relativeDamping;
                    Vector3 containedVertex = transform.TransformPoint(0.5f * (vertices[triangleEdge.id0] - vertices[triangleEdge.id1]));
                    foreach (PropertiesDefineZone propertiesZone in physicManager.propertiesZones)
                    {// checks if the new spring its in a propertie define zone
                        if (propertiesZone.bound.Contains(containedVertex))
                        {
                            springStiffness = propertiesZone.stiffness;
                            dampingOfRotation = propertiesZone.rotationDamping;
                            dampingOfProjective = propertiesZone.projectiveDamping;
                            break;
                        }
                    }
                    //Creates and adds the new spring
                    springs.Add(new Spring(nodes[triangleEdge.id0], nodes[triangleEdge.id1], springStiffness, dampingOfRotation, dampingOfProjective, debugDraw));
                    //Adds the new edge to the dictionary
                    List<int> storedVertices = new List<int>();
                    storedVertices.Add(triangleVertex[(vertexId + 2) % 3]);
                    edgesMap.Add(triangleEdge, storedVertices);
                }
            }
        }
    }
    // DEBUG: method for show the dictionary
    protected string showDictionary(SortedDictionary<Edge, List<int>> edgesMap)
    {
        string edgesString = "Edges count+ " + edgesMap.Count;
        foreach (KeyValuePair<Edge, List<int>> entry in edgesMap)
        {
            edgesString += "\n" + entry.Key + ": [ ";
            int[] pairs = entry.Value.ToArray();
            foreach (int pair in pairs)
            {
                edgesString += pair + " ";
            }
            edgesString += " ]";
        }
        return edgesString;
    }

    // Update is called once per frame
    protected override void recalcVertex()
    {
        foreach (Node node in this.nodes)
        {
            node.force = Vector3.zero;
            node.computeForces();
        }
        foreach (Spring spring in springs)
        {
            spring.computeForces();
        }
    }

    public override void updateMesh()
    {
        for (int vertexId = 0; vertexId < vertices.Length; ++vertexId)
        {
            this.vertices[vertexId] = transform.InverseTransformPoint(this.nodes[vertexId].position);
        }
        this.mesh.vertices = this.vertices;
        this.mesh.RecalculateBounds();
        this.mesh.RecalculateNormals();
        this.mesh.RecalculateTangents();
        if (debugDraw == SimulatedObject.DebugDrawType.GAME_OBJECTS)
        {
            foreach (Node node in this.nodes)
            {
                node.debugDrawing();
            }
            foreach (Spring spring in this.springs)
            {
                spring.debugDrawing();
            }
        }
    }
}