using System.Collections.Generic;
using UnityEngine;

public class Cloth : SimulatedObject
{
    #region InEditorVariables
    [SerializeField] private float stiffness;
    [SerializeField] private float antiStiffness;
    [SerializeField] private float mass;
    #endregion
    private Triangle[] triangles;

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
            foreach (PropertiesDefineZone propertiesZone in this.physicManager.getPropertiesZones())
            {//checks if the node its in a propertiesDefineZone				
                if (propertiesZone.contains(globalVertexPos))
                {
                    propertiesZone.getNodeValues(ref nodeIsFixed, ref nodeMass, ref nodeDamping);
                    break;
                }
            }
            //Creates and stores the new node;
            this.nodes[vertexId] = new Node(globalVertexPos, this.physicManager, nodeMass, nodeDamping, nodeIsFixed, debugDraw);
        }
        this.triangles = new Triangle[this.mesh.triangles.Length / 3];
        for (int triangleId = 0; triangleId < this.mesh.triangles.Length; triangleId += 3)//Iterates through all the triangles
        {
            int[] triangleVertex = new int[3];
            triangleVertex[0] = mesh.triangles[triangleId];
            triangleVertex[1] = mesh.triangles[triangleId + 1];
            triangleVertex[2] = mesh.triangles[triangleId + 2];

            Spring[] triangleSprings = new Spring[3];
            Node[] triangleNodes = new Node[3];
            for (int vertexId = 0; vertexId < 3; ++vertexId)//creates 3 new edges for each triangle
            {
                Edge triangleEdge = new Edge(triangleVertex[vertexId], triangleVertex[(vertexId + 1) % 3]);
                triangleNodes[vertexId] = nodes[vertexId];
                if (edgesMap.ContainsKey(triangleEdge))//checks if the edge already exist
                {
                    List<int> storedVertices;
                    edgesMap.TryGetValue(triangleEdge, out storedVertices);//obtain all triangles that use that mesh
                    if (!storedVertices.Contains(triangleVertex[(vertexId + 2) % 3]))//checks for repetitive triangles
                    {
                        foreach (int storedVertex in storedVertices)//creates a new spring between this triangle and all the other triangles
                        {
                            float springStiffness = this.antiStiffness;
                            float rotationDamping = this.rotationDamping;
                            float projectiveDamping = this.relativeDamping;
                            Vector3 containedVertex = transform.TransformPoint(0.5f * (vertices[storedVertex] - vertices[triangleVertex[(vertexId + 2) % 3]]));
                            foreach (PropertiesDefineZone propertiesZone in physicManager.getPropertiesZones())// checks if the new spring its in a propertie define zone
                            {
                                if (propertiesZone.contains(containedVertex))
                                {
                                    propertiesZone.getAntiStiffnessValues(ref springStiffness, ref rotationDamping, ref projectiveDamping);
                                    break;
                                }
                            }
                            //Creates and stores the new spring between the opposite triangle vertex, this way avoid creating repetitive springs
                            triangleSprings[vertexId] = new Spring(nodes[storedVertex], nodes[triangleVertex[(vertexId + 2) % 3]], springStiffness, rotationDamping, projectiveDamping, debugDraw);
                            springs.Add(triangleSprings[vertexId]);
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
                    Vector3 containedVertex = transform.TransformPoint(0.5f * (vertices[triangleEdge.getId0()] - vertices[triangleEdge.getId1()]));
                    foreach (PropertiesDefineZone propertiesZone in physicManager.getPropertiesZones())
                    {// checks if the new spring its in a propertie define zone
                        if (propertiesZone.contains(containedVertex))
                        {
                            propertiesZone.getStiffnessValues(ref springStiffness, ref dampingOfRotation, ref dampingOfProjective);
                            break;
                        }
                    }
                    //Creates and adds the new spring
                    triangleSprings[vertexId] = new Spring(nodes[triangleEdge.getId0()], nodes[triangleEdge.getId1()], springStiffness, dampingOfRotation, dampingOfProjective, debugDraw);
                    springs.Add(triangleSprings[vertexId]);
                    //Adds the new edge to the dictionary
                    List<int> storedVertices = new List<int>();
                    storedVertices.Add(triangleVertex[(vertexId + 2) % 3]);
                    edgesMap.Add(triangleEdge, storedVertices);
                }
            }
            this.triangles[triangleId / 3] = new Triangle(triangleNodes, triangleSprings);
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
            node.resetForces();
            node.computeForces();
        }
        foreach (WindForce windForce in this.physicManager.getWindForces())
        {
            Vector3 newWindForce = windForce.getActualForce();
            foreach (Triangle triangle in this.triangles)
            {
                triangle.computeWindForce(newWindForce);
            }
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
            this.vertices[vertexId] = transform.InverseTransformPoint(this.nodes[vertexId].getPos());
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