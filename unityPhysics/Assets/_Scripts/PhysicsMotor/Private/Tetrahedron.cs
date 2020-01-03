using UnityEngine;

public class Tetrahedron
{
    //Class used in tetrahedron for draw the debug mesh and calculate the wind force

    #region InEditorVariables
    #endregion

    private Node[] nodes = new Node[4];
    /*
     * Spring order:
     * 0: 0 - 1
     * 1: 0 - 2
     * 2: 0 - 3
     * 3: 1 - 2
     * 4: 1 - 3
     * 5: 2 - 3
     */
    private Spring[] springs = new Spring[6];

    private GameObject tetrahedron;
    private Mesh tetrahedronMesh;
    private Vector3[] initialNormals;
    private Vector3[] initialNodesPositions;
    private Vector3[] initialSpringsLengths;
    private float[] initialPositions;
    private float volume;
    private Vector3[] vertices = new Vector3[4];

    //Initialices the tetrahedron
    public Tetrahedron(Node[] nodes_, Spring[] springs_, TetrahedronObject.DebugTetrahedronsType debugDrawing)
    {
        this.nodes = nodes_;
        this.springs = springs_;
        if (debugDrawing == TetrahedronObject.DebugTetrahedronsType.COMPLETE)
        {//Creates the object to debug draw it 
            this.tetrahedron = GameObject.CreatePrimitive(PrimitiveType.Quad);
            this.tetrahedron.name = ("Tetrahedron: { " + this.nodes[0].getStringId() + " - " + this.nodes[1].getStringId() + " - " + this.nodes[2].getStringId() + " - " + this.nodes[3].getStringId() + " }");
            this.tetrahedronMesh = new Mesh();

            this.tetrahedron.GetComponent<MeshFilter>().mesh = this.tetrahedronMesh;


            for (int vertexId = 0; vertexId < 4; ++vertexId)
            {
                this.vertices[vertexId] = this.nodes[vertexId].getPos();
            }
            this.tetrahedronMesh.vertices = this.vertices;

            int[] facesVertices = {
                0, 2, 1,
                2, 3, 1,
                0, 3, 2,
                0, 1, 3};


            this.tetrahedronMesh.triangles = facesVertices;
            this.tetrahedronMesh.RecalculateBounds();
            this.tetrahedronMesh.RecalculateNormals();
            this.tetrahedronMesh.RecalculateTangents();
        }
        this.initialSpringsLengths = new Vector3[] { this.springs[0].getDirection(), this.springs[1].getDirection(), this.springs[2].getDirection() };
        this.initialNodesPositions = new Vector3[] { this.nodes[0].getPos(), this.nodes[1].getPos(), this.nodes[2].getPos(), this.nodes[3].getPos() };

        //Initialices values used to weight vertex
        reCalculateVolume();
        this.initialNormals = new Vector3[4];
        this.initialPositions = new float[4];
        for (int vertexId = 0; vertexId < 4; ++vertexId)
        {
            this.initialNormals[vertexId] = Vector3.Cross(this.initialNodesPositions[(vertexId + 1) % 4] - this.initialNodesPositions[vertexId], this.initialNodesPositions[(vertexId + 2) % 4] - this.initialNodesPositions[vertexId]);
            this.initialPositions[vertexId] = Vector3.Dot(this.initialNormals[vertexId], this.initialNodesPositions[(vertexId + 3) % 4] - this.initialNodesPositions[vertexId]);
            this.nodes[vertexId].addMassFromVolume( this.volume * 0.25f);
        }
    }

    //Used for draw the mesh on completeTetrahedrons
    public void debugDraw()
    {
        for (int vertexId = 0; vertexId < 4; vertexId++)
        {
            this.vertices[vertexId] = this.nodes[vertexId].getPos();
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
        for (int vertexId = 0; vertexId < 4; ++vertexId)
        {
            tetrahedronLengths[vertexId] = point-this.nodes[vertexId].getPos();
            if (Mathf.Sign(Vector3.Dot(this.initialNormals[vertexId], tetrahedronLengths[vertexId])) != Mathf.Sign(this.initialPositions[vertexId]))
            {
                return null;
            }
        }
        PhysicVertex personalVertex = new PhysicVertex();


        personalVertex.setWeight(0, calculateVolume(tetrahedronLengths[1], tetrahedronLengths[2], tetrahedronLengths[3]) / this.volume, this.nodes[0]);
        personalVertex.setWeight(1, calculateVolume(tetrahedronLengths[0], tetrahedronLengths[2], tetrahedronLengths[3]) / this.volume, this.nodes[1]);
        personalVertex.setWeight(2, calculateVolume(tetrahedronLengths[0], tetrahedronLengths[1], tetrahedronLengths[3]) / this.volume, this.nodes[2]);
        personalVertex.setWeight(3, calculateVolume(tetrahedronLengths[0], tetrahedronLengths[1], tetrahedronLengths[2]) / this.volume, this.nodes[3]);

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
        this.volume = calculateVolume(this.springs[0].getDirection(), this.springs[1].getDirection(), this.springs[2].getDirection());
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