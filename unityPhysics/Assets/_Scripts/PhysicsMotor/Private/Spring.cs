using UnityEngine;

public class Spring
{
    #region InEditorVariables
    public float stiffness;
    #endregion

    public Node node0;
    public Node node1;
    public float initLength;
    public float length;
    public float rotationDamping;
    public float projectiveDamping;
    public Vector3 direction;
    public Vector3 forceFactor;

    private GameObject cylinder;
    private float lengthMin;
    private float lengthMax;
    private float thickness;

    // Use this for initialization in fabrics (Stiffness)
    public Spring(Node node0_, Node node1_, float stiffness_, float rotationDamping_, float projectiveDamping_, SimulatedObject.DebugDrawType debugDraw)
    {
        this.thickness = 0.1f;
        createSpring(node0_, node1_, rotationDamping_, projectiveDamping_, debugDraw);
        this.stiffness = stiffness_;
    }

    // Use this for initialization in tetrahedrons (StiffnessDensity of nodes)
    public Spring(Node node0_, Node node1_, float rotationDamping_, float projectiveDamping_, SimulatedObject.DebugDrawType debugDraw)
    {
        this.thickness = 12.5f;
        createSpring(node0_, node1_, rotationDamping_, projectiveDamping_, debugDraw);
        this.stiffness = (node0_.stiffness + node1_.stiffness) * 0.5f;
    }

    //Auxiliary method for initialices springs in both cases
    private void createSpring(Node node0_, Node node1_, float rotationDamping_, float projectiveDamping_, SimulatedObject.DebugDrawType debugDraw)
    {
        this.node0 = node0_;
        this.node1 = node1_;
        this.direction = this.node0.position - this.node1.position;
        this.initLength = this.length = this.direction.magnitude;
        this.lengthMin = 0.3f * this.initLength;
        this.lengthMax = 3.0f * this.initLength;
        this.rotationDamping = rotationDamping_;
        this.projectiveDamping = projectiveDamping_;

        if (debugDraw == SimulatedObject.DebugDrawType.GAME_OBJECTS)//If necessary, creates the debug cylinder
        {
            this.cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            this.cylinder.name = "Spring-" + this.node0.nodeId.ToString("00000") + "-" + this.node1.nodeId.ToString("00000");
            debugDrawing();
        }
    }

    //Updates the debug cylinder
    public void debugDrawing()
    {
        this.cylinder.transform.position = 0.5f * (this.node0.position + this.node1.position);
        this.cylinder.transform.localScale = new Vector3(this.thickness, this.length * 0.5f, this.thickness);//The default length of a cylinder in Unity is 2.0
        this.cylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, this.direction.normalized);
    }

    //Creates the debug gizmos
    public void debugGizmos()
    {
        Gizmos.DrawLine(this.node0.position, this.node1.position);
    }

    //Compute deformative forces and damping in 1D spring
    public void computeForces()
    {
        calculateLength();
        Vector3 velDifference = this.node0.velocity - this.node1.velocity;
        Vector3 force = stiffness * ((this.initLength - this.length) * this.direction.normalized - this.rotationDamping * velDifference - this.projectiveDamping * Vector3.Dot(this.direction.normalized, velDifference) * this.direction.normalized);
        node0.force += force;
        node1.force -= force;
    }

    //Compute deformative forces in 1D spring
    public void computeForceFactor()
    {
        this.forceFactor = (this.initLength - this.length) * this.direction * this.stiffness / (6 * this.initLength * this.initLength * this.length);
    }

    //Compute deformative forces in springs of volumetric tetrahedrons
    public void addForces(float volume)
    {
        Vector3 force = volume * forceFactor;
        this.node0.force += force;
        this.node1.force -= force;
    }

    //Compute Damping forces
    public void computeDamping()
    {
        Vector3 velDifference = this.node0.velocity - this.node1.velocity;
        Vector3 force = this.stiffness * (this.rotationDamping * velDifference + this.projectiveDamping * Vector3.Dot(this.direction.normalized, velDifference) * this.direction.normalized);
        this.node0.force -= force;
        this.node1.force += force;
    }

    //Recalculate spring Length
    public void calculateLength()
    {
        this.direction = this.node0.position - this.node1.position;
        this.length = this.direction.magnitude;
        //Sets a range of values to avoid problems
        if (this.length < this.lengthMin)
        {
            this.direction = this.direction.normalized * this.lengthMin;
            this.length = this.lengthMin;
        }
        else if (this.length > this.lengthMax)
        {
            this.direction = this.direction.normalized * this.lengthMax;
            this.length = this.lengthMax;
        }
    }
}