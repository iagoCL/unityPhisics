using UnityEngine;

public class Tetrahedron
{
    //Class used in tetrahedron for draw the debug mesh and calculate the wind force

    #region InEditorVariables
    #endregion

    public Node[] nodes = new Node[4];
    public Spring[] springs = new Spring[6];
    /*
     * Spring order:
     * 0 - 1
     * 0 - 2
     * 0 - 3
     * 1 - 2
     * 1 - 3
     * 2 - 3
     */

    private GameObject tetrahedron;
    private Mesh tetrahedronMesh;
    private Vector3[] initialNormals;
    private float[] initialPositions;
    private float volume;
    private Vector3[] vertices = new Vector3[4];

    public Tetrahedron() { }

    //Initialices the tetrahedron
    public void initData(TetrahedronObject.DebugTetrahedronsType debugDrawing)
    {
        if (debugDrawing == TetrahedronObject.DebugTetrahedronsType.COMPLETE)
        {//Creates the object to debug draw it 
            this.tetrahedron = GameObject.CreatePrimitive(PrimitiveType.Quad);
            this.tetrahedron.name = ("Tetrahedron: { " + this.nodes[0].position.ToString("00000") + " - " + this.nodes[1].position.ToString("00000") + " - " + this.nodes[2].position.ToString("00000") + " - " + this.nodes[3].position.ToString("00000") + " }");
            this.tetrahedronMesh = new Mesh();

            this.tetrahedron.GetComponent<MeshFilter>().mesh = this.tetrahedronMesh;


            for (int vertexId = 0; vertexId < 4; ++vertexId)
            {
                this.vertices[vertexId] = this.nodes[vertexId].position;
            }
            this.tetrahedronMesh.vertices = this.vertices;

            int[] facesVertices = new int[12];

            facesVertices[0] = 0;
            facesVertices[1] = 2;
            facesVertices[2] = 1;

            facesVertices[3] = 2;
            facesVertices[4] = 3;
            facesVertices[5] = 1;

            facesVertices[6] = 0;
            facesVertices[7] = 3;
            facesVertices[8] = 2;

            facesVertices[9] = 0;
            facesVertices[10] = 1;
            facesVertices[11] = 3;


            this.tetrahedronMesh.triangles = facesVertices;

            this.tetrahedronMesh.RecalculateBounds();
            this.tetrahedronMesh.RecalculateNormals();
            this.tetrahedronMesh.RecalculateTangents();
        }

        //Initialices values used to weight vertex
        reCalculateVolume();
        this.initialNormals = new Vector3[4];
        this.initialPositions = new float[4];
        for (int vertexId = 0; vertexId < 4; ++vertexId)
        {
            this.initialNormals[vertexId] = Vector3.Cross(this.nodes[(vertexId + 1) % 4].position - this.nodes[vertexId].position, this.nodes[(vertexId + 2) % 4].position - this.nodes[vertexId].position);
            this.initialPositions[vertexId] = Vector3.Dot(this.initialNormals[vertexId], this.nodes[(vertexId + 3) % 4].position - this.nodes[vertexId].position);
            this.nodes[vertexId].mass += this.nodes[vertexId].density * this.volume * 0.25f;
        }
    }

    //Used for draw the mesh on completeTetrahedrons
    public void debugDraw()
    {
        for (int vertexId = 0; vertexId < 4; vertexId++)
        {
            this.vertices[vertexId] = this.nodes[vertexId].position;
        }
        this.tetrahedronMesh.vertices = this.vertices;
        this.tetrahedronMesh.RecalculateBounds();
        this.tetrahedronMesh.RecalculateNormals();
        this.tetrahedronMesh.RecalculateTangents();

    }

    //If contained in the tetrahedron return a weighted vertex, else return null
    public PhysicVertex containedPoint(Vector3 point)
    {

        Vector3[] tetrahedronLengths = new Vector3[4];
        for (int vertexId = 0; vertexId < 4; vertexId++)
        {
            tetrahedronLengths[vertexId] = point - this.nodes[vertexId].position;
            if (Mathf.Sign(Vector3.Dot(this.initialNormals[vertexId], tetrahedronLengths[vertexId])) != Mathf.Sign(this.initialPositions[vertexId]))
            {
                return null;
            }
        }
        PhysicVertex personalVertex = new PhysicVertex();

        personalVertex.setWeight(0, calculateVolume(-tetrahedronLengths[1], -tetrahedronLengths[2], -tetrahedronLengths[3]) / this.volume, this.nodes[0]);
        personalVertex.setWeight(1, calculateVolume(tetrahedronLengths[0], this.springs[1].direction, this.springs[2].direction) / this.volume, this.nodes[1]);
        personalVertex.setWeight(2, calculateVolume(this.springs[0].direction, tetrahedronLengths[0], this.springs[2].direction) / this.volume, this.nodes[2]);
        personalVertex.setWeight(3, calculateVolume(this.springs[0].direction, this.springs[1].direction, tetrahedronLengths[0]) / this.volume, this.nodes[3]);

        return personalVertex;
    }

    //Given 3 vectors calculate the volume of the tetrahedron within them
    private float calculateVolume(Vector3 distance1, Vector3 distance2, Vector3 distance3)
    {
        return Mathf.Abs(Vector3.Dot(distance1, Vector3.Cross(distance2, distance3))) / 6.0f;
    }

    //Recalculate the tetrahedron volume
    private void reCalculateVolume()
    {
        this.volume = calculateVolume(this.springs[0].direction, this.springs[1].direction, this.springs[2].direction);
    }

    //Apply the corresponding deformity forces to each tetrahedron
    public void reCalculateForces()
    {
        reCalculateVolume();
        foreach (Spring spring in this.springs)
        {
            spring.addForces(volume);
        }
    }
}