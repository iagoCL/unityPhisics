using UnityEngine;

public class Node
{
    #region InEditorVariables
    #endregion

    public float mass;
    public float density;
    public float damping;
    public float stiffness;
    public bool isFixed;
    public PhysicsManager physicManager;

    public Vector3 position;
    public Vector3 velocity;
    public Vector3 force;
    private GameObject sphere;
    private float sphereRadius;
    public int nodeId;



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
        this.nodeId = ++this.physicManager.totalNodes;
        this.velocity = this.force = Vector3.zero;
        this.isFixed = isFixed_;
        this.damping = damping_;
        if (debugDraw == SimulatedObject.DebugDrawType.GAME_OBJECTS)
        {
            this.sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            this.sphere.transform.localScale = new Vector3(this.sphereRadius, this.sphereRadius, this.sphereRadius);
            this.sphere.name = "node-" + this.nodeId.ToString("00000");
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
        this.force += this.mass * (this.physicManager.gravity - this.velocity * this.damping);
        if (physicManager.collisionZones.Count > 0)
        {
            foreach (CollisionZone colZone in this.physicManager.collisionZones)
            {
                this.force += colZone.calculatePenaltyForce(this.position);
            }
        }
    }
}