using UnityEngine;

public class PropertiesDefineZone : MonoBehaviour
{
    #region InEditorVariables
    public float mass;
    public float stiffness;
    public float antiStiffness;
    public bool isFixed;
    public float massDamping;
    public float rotationDamping;
    public float projectiveDamping;
    #endregion

    public Bounds bound;

    //Variant of a fixer object with extra properties

    // Use this for initialization
    public void initialize()
    {
        this.bound = this.transform.GetComponent<Collider>().bounds;
    }
}