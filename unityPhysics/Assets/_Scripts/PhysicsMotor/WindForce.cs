using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindForce : MonoBehaviour
{
    #region InEditorVariables
    [SerializeField] private Vector3 direction;

    [SerializeField] private Vector3 forceAngle;
    [SerializeField] private float changeTime;
    [SerializeField] private float minForce;
    [SerializeField] private float maxForce;
    #endregion
    [SerializeField] private float actualForce;
    private float remainTime;
    private float angleSpeed;
    private float forceSpeed;
    private float objectiveForce;
    [SerializeField] private Vector3 actualDirection;
    private Vector3 objectiveDirection;


    public void initialize()
    {
        this.direction = this.actualDirection = this.direction.normalized;
        this.actualForce = (this.maxForce + this.minForce) * 0.5f;
        newObjectiveForce();
    }
    public void updateRandomForce(float deltaTime)
    {
        this.remainTime -= deltaTime;
        if (this.remainTime <= deltaTime)
        {
            newObjectiveForce();
        }
        else
        {
            this.actualDirection = Vector3.RotateTowards(this.actualDirection, this.objectiveDirection, deltaTime * this.angleSpeed, 0.0f);
            this.actualForce += deltaTime * this.forceSpeed;
        }
    }
    private void newObjectiveForce()
    {
        this.objectiveDirection = Quaternion.Euler(
            Random.Range(-this.forceAngle.x, this.forceAngle.x),
            Random.Range(-this.forceAngle.y, this.forceAngle.y),
            Random.Range(-this.forceAngle.z, this.forceAngle.z)) * direction;
        this.objectiveForce = Random.Range(this.minForce, this.maxForce);
        this.angleSpeed = Mathf.Deg2Rad * Vector3.Angle(this.objectiveDirection, this.actualDirection) / this.changeTime;
        this.forceSpeed = (this.objectiveForce - this.actualForce) / this.changeTime;
        this.remainTime = this.changeTime;
    }
    public Vector3 getActualForce()
    {
        return this.actualForce * this.actualDirection;
    }
}