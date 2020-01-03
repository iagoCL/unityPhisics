using UnityEngine;

public class Node
{
    #region InEditorVariables
    #endregion

    [SerializeField] private float mass;
    [SerializeField] private float density;
    [SerializeField] private float damping;
    [SerializeField] private float stiffness;
    [SerializeField] private bool isFixed;
    private PhysicsManager physicManager;

    [SerializeField] private Vector3 position;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private Vector3 force;
    private GameObject sphere;
    private float sphereRadius;
    [SerializeField] private int nodeId;



    // Use this for initialization in fabrics (mass)
    public Node(Vector3 position_, PhysicsManager physicsManager_, float mass_, float damping_, bool isFixed_, SimulatedObject.DebugDrawType debugDraw)
    {
        this.mass = mass_;
        this.sphereRadius = 0.2f;
        createNode(position_, physicsManager_, damping_, isFixed_, debugDraw);
    }

    // Use this for initialization in tetrahedrons (density)
    public Node(Vector3 position_, PhysicsManager physicsManager_, float density_, float damping_, bool isFixed_, SimulatedObject.DebugDrawType debugDraw, float stiffness_)
    {
        this.density = density_;
        this.mass = 0.0f;
        this.stiffness = stiffness_;
        this.sphereRadius = 80.0f;
        createNode(position_, physicsManager_, damping_, isFixed_, debugDraw);
    }

    //Auxiliary method for initialices springs in both cases
    private void createNode(Vector3 position_, PhysicsManager physicsManager_, float damping_, bool isFixed_, SimulatedObject.DebugDrawType debugDraw)
    {
        this.position = position_;
        this.physicManager = physicsManager_;
        this.nodeId = this.physicManager.getNewNodeId();
        this.velocity = this.force = Vector3.zero;
        this.isFixed = isFixed_;
        this.damping = damping_;
        if (debugDraw == SimulatedObject.DebugDrawType.GAME_OBJECTS)
        {
            this.sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            this.sphere.transform.localScale = new Vector3(this.sphereRadius, this.sphereRadius, this.sphereRadius);
            this.sphere.name = "node-" + this.getStringId();
            debugDrawing();
        }
    }

    //Updates the debug sphere
    public void debugDrawing()
    {
        this.sphere.transform.position = this.position;
    }

    //Creates the debug gizmos
    public void debugGizmos()
    {
        Gizmos.DrawSphere(this.position, this.sphereRadius);
    }

    //Add gravity force and checks if its in a collision
    public void computeForces()
    {
        this.force += this.mass * (this.physicManager.getGravity() - this.velocity * this.damping);
        foreach (CollisionZone colZone in this.physicManager.getCollisionsZones())
        {
            this.force += colZone.calculatePenaltyForce(this.position);
        }
    }

    public void resetForces()
    {
        this.force = Vector3.zero;
    }

    public float getStiffness()
    {
        return this.stiffness;
    }

    public Vector3 getPos()
    {
        return this.position;
    }
    public Vector3 getVelocity()
    {
        return this.velocity;
    }
    public string getStringId()
    {
        return this.nodeId.ToString("00000");
    }
    public bool getFixed()
    {
        return this.isFixed;
    }
    public void addForce(Vector3 force_){
        this.force += force_;
    }
    public void addMassFromVolume(float volume){
        this.mass += this.density * volume;
    }
    public void updatePos()
    {
        this.position += this.velocity * this.physicManager.getDeltaTime();
    }
    public void updateVelocity()
    {
        this.velocity += this.force * this.physicManager.getDeltaTime() / this.mass;
    }
}