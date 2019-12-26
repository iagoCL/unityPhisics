using UnityEngine;
using System.Collections.Generic;

public class PhysicsManager : MonoBehaviour
{
    #region InEditorVariables
    public bool paused;
    public float deltaTime;
    public Vector3 gravity;
    public Integration integrationMethod;
    public List<SimulatedObject> simulatedObjects;
    public List<CollisionZone> collisionZones;
    public List<PropertiesDefineZone> propertiesZones;
    public int iterationsPerFrame;
    public Vector3[] windForces;
    public float windRand = 0.0f;
    #endregion
    public int totalNodes = 0;

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
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.P))
            paused = !paused;
        foreach (SimulatedObject simulatedObject in simulatedObjects)
        {
            simulatedObject.updateMesh();
        }
    }

    public void FixedUpdate()
    {
        if (paused)
            return; // Not simulating
        // Select integration method
        switch (integrationMethod)
        {
            case Integration.EXPLICIT:
                for (int i = 0; i < iterationsPerFrame; i++)
                {
                    stepExplicit();
                }
                break;
            case Integration.SYMPLECTIC:
                for (int i = 0; i < iterationsPerFrame; i++)
                {
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

}