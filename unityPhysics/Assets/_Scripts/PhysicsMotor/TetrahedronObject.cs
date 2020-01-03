using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class TetrahedronObject : SimulatedObject
{
    #region InEditorVariables
    [SerializeField] private TextAsset nodeText;
    [SerializeField] private TextAsset elementText;
    [SerializeField] private TextAsset faceText;
    [SerializeField] private DebugTetrahedronsType debugTetrahedrons;
    [SerializeField] private ObtainProperties readPropertiesFrom;

    [SerializeField] private float stiffnessDensity;
    [SerializeField] private float massDensity;
    #endregion

    private Tetrahedron[] tetrahedrons;
    private Triangle[] triangles;
    private Mesh tetrahedronMesh;
    private PhysicVertex[] personalVertices;

    public enum DebugTetrahedronsType
    {
        NONE = 0,
        COMPLETE = 1,
        TRIANGLE_MESH = 2
    };
    public enum ObtainProperties
    {
        NONE = 0,
        FILE_ATTRIBUTES = 1,
        PROPERTIES_ZONES = 2
    };

    // Use this for initialization
    public override void loadData(PhysicsManager physicManager_)
    {
        this.physicManager = physicManager_;
        if (readPropertiesFrom == ObtainProperties.FILE_ATTRIBUTES)
        {
            //If activated recalculates the value of the attribute for each color unit 
            this.massDensity = this.massDensity / 255.0f;
            this.stiffnessDensity = this.stiffnessDensity / 255.0f;
        }
        //Parse and initialices nodes
        string[] lines = Regex.Split(nodeText.text, "\\s*\\n+\\s*");
        string[] elements = Regex.Split(lines[0], "\\s+");
        this.nodes = new Node[System.Int32.Parse(elements[0])];
        for (int i = 1; i < lines.Length; i++)
        {
            //Parse nodes
            if (!(lines[i].StartsWith("#")))//Not comment lines
            {
                elements = Regex.Split(lines[i], "\\s+");
                if (elements.Length >= 4)//Correct number of attributes
                {
                    //Obtain the node position
                    Vector3 nodePos = transform.TransformPoint(new Vector3(float.Parse(elements[1]), float.Parse(elements[2]), float.Parse(elements[3])));
                    //Initialices values at deffault value
                    bool nodeIsFixed = this.isFixed;
                    float nodeDensity = this.massDensity;
                    float nodeDamping = this.massDamping;
                    float nodeStiffness = this.stiffnessDensity;
                    if (readPropertiesFrom == ObtainProperties.PROPERTIES_ZONES)
                    {//obtain values from properties zones
                        foreach (PropertiesDefineZone propertiesZone in this.physicManager.getPropertiesZones())
                        {//checks if the node its in a propertiesDefineZone				
                            if (propertiesZone.contains(nodePos))
                            {
                                propertiesZone.getTetrahedronValues(ref nodeIsFixed, ref nodeDensity, ref nodeDamping, ref nodeStiffness);
                                break;
                            }
                        }
                    }
                    else if (readPropertiesFrom == ObtainProperties.FILE_ATTRIBUTES)
                    {//Obtain values from extra attributes in node file
                        if (elements.Length >= 7)
                        {//Checks the new attribute requirement
                            nodeDensity *= System.Int32.Parse(elements[4]);//component red
                            nodeStiffness *= System.Int32.Parse(elements[5]);//component green
                            if (System.Int32.Parse(elements[6]) > 128)
                            {//component blue
                                nodeIsFixed = !nodeIsFixed;
                            }
                        }
                        else
                        {
                            print("No attributes detected in line: " + i + ": " + lines[i]);
                        }
                    }
                    //Creates the new node
                    this.nodes[System.Int32.Parse(elements[0])] = new Node(nodePos, this.physicManager, nodeDensity, nodeDamping, nodeIsFixed, debugDraw, nodeStiffness);
                }
                else
                {
                    print("Unexpected format in line: " + i + ": " + lines[i]);
                }
            }
        }

        //Creates the map used to avoid duplicate strings
        SortedDictionary<Edge, Spring> mapEdges = new SortedDictionary<Edge, Spring>();

        //Parse and initialices triangles faces (if necessary)
        if (debugTetrahedrons == DebugTetrahedronsType.TRIANGLE_MESH || this.physicManager.getWindForces().Count > 0)
        {
            lines = Regex.Split(this.faceText.text, "\\s*\\n+\\s*");
            elements = Regex.Split(lines[0], "\\s+");
            this.triangles = new Triangle[System.Int32.Parse(elements[0])];
            int[] trianglesIndexes = new int[3 * this.triangles.Length];
            for (int lineNum = 1; lineNum < lines.Length; lineNum++)//Parse faces/triangles
            {
                if (!lines[lineNum].StartsWith("#"))
                {//Not comment line
                    elements = Regex.Split(lines[lineNum], "\\s+");
                    //debugingFileLine(i,lines[i],elements);
                    if (elements.Length >= 4)
                    {//correct number of attributes
                        int actualIndex = System.Int32.Parse(elements[0]);
                        int actualIndexTriple = actualIndex * 3;

                        Node[] triangleNodes = new Node[3];
                        for (int indexId = 0; indexId < 3; ++indexId)
                        {//each line element
                            trianglesIndexes[actualIndexTriple + indexId] = System.Int32.Parse(elements[3 - indexId]);
                            triangleNodes[indexId] = this.nodes[trianglesIndexes[indexId]];
                        }
                        Spring[] triangleSprings = {
                            assignSpring(trianglesIndexes[actualIndexTriple], trianglesIndexes[actualIndexTriple + 1], mapEdges),
                            assignSpring(trianglesIndexes[actualIndexTriple], trianglesIndexes[actualIndexTriple + 2], mapEdges),
                            assignSpring(trianglesIndexes[actualIndexTriple + 1], trianglesIndexes[actualIndexTriple + 2], mapEdges)
                        };

                        //Assign the new triangle
                        triangles[actualIndex] = new Triangle(triangleNodes, triangleSprings);
                    }
                    else
                    {
                        print("Unexpected format in line: " + lineNum + ": " + lines[lineNum]);
                    }
                }
            }
            if (debugTetrahedrons == DebugTetrahedronsType.TRIANGLE_MESH)
            {//If debug using mesh triangle creates a new mesh triangle
                GameObject tetrahedronObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tetrahedronObject.name = ("DebugTetrahedronMesh: " + this.transform.name);
                this.tetrahedronMesh = new Mesh();
                tetrahedronObject.GetComponent<MeshFilter>().mesh = this.tetrahedronMesh;

                Vector3[] trianglesVertices = new Vector3[this.nodes.Length];
                for (int i = 0; i < this.nodes.Length; i++)
                {
                    trianglesVertices[i] = this.nodes[i].getPos();
                }
                this.tetrahedronMesh.vertices = trianglesVertices;
                this.tetrahedronMesh.triangles = trianglesIndexes;
                this.tetrahedronMesh.RecalculateBounds();
                this.tetrahedronMesh.RecalculateNormals();
                this.tetrahedronMesh.RecalculateTangents();
            }

        }
        //Parse and initialices springs and tetrahedron
        lines = Regex.Split(elementText.text, "\\s*\\n+\\s*");
        elements = Regex.Split(lines[0], "\\s+");
        tetrahedrons = new Tetrahedron[System.Int32.Parse(elements[0])];
        for (int lineId = 1; lineId < lines.Length; ++lineId)//Parse tetrahedrons
        {
            if (!lines[lineId].StartsWith("#"))//Not a comment line
            {
                elements = Regex.Split(lines[lineId], "\\s+");
                if (elements.Length >= 4)//Correct number of elements
                {
                    int[] tetrahedronVertices = new int[4];
                    for (int elementId = 0; elementId < 4; elementId++)//each index vertex
                    {
                        tetrahedronVertices[elementId] = System.Int32.Parse(elements[elementId + 1]);
                    }
                    //Creates a new tetrahedron
                    Node[] tetrahedronNodes = new Node[4];
                    Spring[] tetrahedronSprings = new Spring[6];
                    int index = -1;
                    for (int elementId = 0; elementId < 4; ++elementId)//Assign the nodes and springs to the tetrahedron
                    {
                        tetrahedronNodes[elementId] = nodes[tetrahedronVertices[elementId]];
                        for (int vertexId = elementId + 1; vertexId < 4; ++vertexId)
                        {
                            tetrahedronSprings[++index] = assignSpring(tetrahedronVertices[elementId], tetrahedronVertices[vertexId], mapEdges);
                        }
                    }
                    tetrahedrons[System.Int32.Parse(elements[0])] = new Tetrahedron(tetrahedronNodes, tetrahedronSprings, debugTetrahedrons);
                }
                else
                {
                    print("Unexpected format in line: " + lineId + ": " + lines[lineId]);
                }
            }
        }

        //initialices vertex weights
        mesh = transform.GetComponent<MeshFilter>().mesh;
        personalVertices = new PhysicVertex[mesh.vertices.Length];
        vertices = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < personalVertices.Length; i++)
        {
            personalVertices[i] = assignPersonalVertex(transform.TransformPoint(mesh.vertices[i]));
        }
    }

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

        foreach (Spring spring in this.springs)
        {
            spring.calculateLength();
            spring.computeForceFactor();
            spring.computeDamping();
        }
        foreach (Tetrahedron tetrahedron in tetrahedrons)
        {
            tetrahedron.reCalculateForces();

        }
    }

    public override void updateMesh()
    {
        for (int i = 0; i < personalVertices.Length; i++)
        {
            vertices[i] = transform.InverseTransformPoint(personalVertices[i].recalcPos());
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        if (debugTetrahedrons == DebugTetrahedronsType.COMPLETE)
        {
            foreach (Tetrahedron tetrahedron in tetrahedrons)
            {
                tetrahedron.debugDraw();
            }
        }
        else if (debugTetrahedrons == DebugTetrahedronsType.TRIANGLE_MESH)
        {
            Vector3[] trianglesVertices = new Vector3[this.nodes.Length];
            for (int i = 0; i < this.nodes.Length; i++)
            {
                trianglesVertices[i] = this.nodes[i].getPos();
            }
            tetrahedronMesh.vertices = trianglesVertices;
            tetrahedronMesh.RecalculateBounds();
            tetrahedronMesh.RecalculateNormals();
            tetrahedronMesh.RecalculateTangents();
        }

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

    //return the correct spring for those vertices, avoiding duplicated springs
    private Spring assignSpring(int vertexA, int vertexB, SortedDictionary<Edge, Spring> edgesMap_)
    {
        Edge edge = new Edge(vertexA, vertexB);
        Spring returnSpring;

        if (edgesMap_.ContainsKey(edge))
        {//checks if the edge already exist                               
            edgesMap_.TryGetValue(edge, out returnSpring);//obtain the actual spring
        }
        else
        {//if the edge its not cointain creates a new edge and adds it to the dictionary 
            returnSpring = new Spring(this.nodes[edge.getId0()], this.nodes[edge.getId1()], rotationDamping, relativeDamping, debugDraw);
            this.springs.Add(returnSpring);
            edgesMap_.Add(edge, returnSpring);
        }
        return returnSpring;

    }

    //Assign a tetrahedron and nodes weights to a vertex
    private PhysicVertex assignPersonalVertex(Vector3 vertexPos)
    {
        foreach (Tetrahedron tetrahedron in this.tetrahedrons)
        {

            PhysicVertex personalVertex = tetrahedron.containedPoint(vertexPos);
            if (personalVertex != null)//Checks in which tetrahedron is contained the vertex
            {
                return personalVertex;
            }
        }
        print("Error vertex: " + vertexPos + " not contained");
        return new PhysicVertex();
    }

    //Debug method for show a parsed line
    private void debugingFileLine(int i, string line, string[] elements)
    {
        string printS = i + ": " + line + " with: " + elements.Length + "elements:{ ";
        for (int a = 0; a < elements.Length; a++)
        {
            printS += a + ": " + elements[a] + "\n";
        }
        print(printS + "\n}");
    }
}