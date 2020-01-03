using UnityEngine;
using System.Collections.Generic;

public class PhysicsManager : MonoBehaviour
{
    #region InEditorVariables
    [SerializeField] private bool paused;
    [SerializeField] private float deltaTime;
    [SerializeField] private Vector3 gravity;
    [SerializeField] private Integration integrationMethod;
    [SerializeField] private List<SimulatedObject> simulatedObjects;
    [SerializeField] private List<CollisionZone> collisionZones;
    [SerializeField] private List<PropertiesDefineZone> propertiesZones;
    [SerializeField] private List<WindForce> windForces;
    [SerializeField] private int iterationsPerFrame;
    #endregion
    private int totalNodes = 0;

    public enum Integration
    {
        EXPLICIT = 0,
        SYMPLECTIC = 1,
    };

    public PhysicsManager()
    {
        paused = true;
        deltaTime = 0.01f;
        gravity = new Vector3(0.0f, -9.81f, 0.0f);
        integrationMethod = Integration.EXPLICIT;
        iterationsPerFrame = 1;
    }

    #region MonoBehaviour

    public void Start()
    {
        foreach (PropertiesDefineZone propertiesZone in propertiesZones)
        {
            propertiesZone.initialize();
        }
        foreach (CollisionZone colZone in collisionZones)
        {
            colZone.initialateBound();
        }
        foreach (SimulatedObject simulatedObject in simulatedObjects)
        {
            simulatedObject.loadData(this);
        }
        foreach (WindForce windForce in windForces)
        {
            windForce.initialize();
        }
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.P))
        {
            paused = !paused;
        }
        if (!paused)
        {
            foreach (SimulatedObject simulatedObject in simulatedObjects)
            {
                simulatedObject.updateMesh();
            }
        }
    }

    public void FixedUpdate()
    {
        if (paused)
        {
            return; // Not simulating
        }
        // Select integration method
        switch (integrationMethod)
        {
            case Integration.EXPLICIT:
                for (int i = 0; i < iterationsPerFrame; i++)
                {
                    updateWindForces();
                    stepExplicit();
                }
                break;
            case Integration.SYMPLECTIC:
                for (int i = 0; i < iterationsPerFrame; i++)
                {
                    updateWindForces();
                    stepSymplectic();
                }
                break;
            default:
                throw new System.Exception("[ERROR] Should never happen!");
        }
    }

    #endregion

    /// <summary>
    /// Performs a simulation step using Explicit integration.
    /// </summary>
    private void stepExplicit()
    {
        foreach (SimulatedObject simulatedObject in simulatedObjects)
        {
            simulatedObject.recalcVertexExplicit();
        }
    }

    /// <summary>
    /// Performs a simulation step using Symplectic integration.
    /// </summary>
    private void stepSymplectic()
    {
        foreach (SimulatedObject simulatedObject in simulatedObjects)
        {
            simulatedObject.recalcVertexSymplectic();
        }
    }

    private void updateWindForces()
    {
        foreach (WindForce windForce in windForces)
        {
            windForce.updateRandomForce(this.deltaTime);
        }
    }

    public int getNewNodeId()
    {
        return (++this.totalNodes);
    }

    public float getDeltaTime()
    {
        return this.deltaTime;
    }

    public Vector3 getGravity()
    {
        return this.gravity;
    }

    public List<CollisionZone> getCollisionsZones()
    {
        return this.collisionZones;
    }
    public List<PropertiesDefineZone> getPropertiesZones()
    {
        return this.propertiesZones;
    }
    public List<WindForce> getWindForces()
    {
        return this.windForces;
    }
}