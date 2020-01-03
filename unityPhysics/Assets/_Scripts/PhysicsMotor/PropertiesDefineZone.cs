using UnityEngine;

public class PropertiesDefineZone : MonoBehaviour
{
    #region InEditorVariables
    [SerializeField] private float mass;
    [SerializeField] private float stiffness;
    [SerializeField] private float antiStiffness;
    [SerializeField] private bool isFixed;
    [SerializeField] private float massDamping;
    [SerializeField] private float rotationDamping;
    [SerializeField] private float projectiveDamping;
    #endregion

    private Bounds bound;

    //Variant of a fixer object with extra properties

    // Use this for initialization
    public void initialize()
    {
        this.bound = this.transform.GetComponent<Collider>().bounds;
    }

    public bool contains(Vector3 globalPoint)
    {
        return this.bound.Contains(globalPoint);
    }
    public void getNodeValues(ref bool nodeFixed_, ref float nodeMass_, ref float nodeDamping_)
    {
        nodeFixed_ = this.isFixed;
        nodeMass_ = this.mass;
        nodeDamping_ = this.massDamping;
    }
    public void getAntiStiffnessValues(ref float antiStiffness_, ref float rotationDamping_, ref float projectiveDamping_)
    {
        antiStiffness_ = this.antiStiffness;
        rotationDamping_ = this.rotationDamping;
        projectiveDamping_ = this.projectiveDamping;
    }
    public void getStiffnessValues(ref float stiffness_, ref float rotationDamping_, ref float projectiveDamping_)
    {
        stiffness_ = this.stiffness;
        rotationDamping_ = this.rotationDamping;
        projectiveDamping_ = this.projectiveDamping;
    }
    public void getTetrahedronValues(ref bool nodeFixed_, ref float nodeMass_, ref float nodeDamping_, ref float stiffness_)
    {
        nodeFixed_ = this.isFixed;
        nodeMass_ = this.mass;
        nodeDamping_ = this.massDamping;
        stiffness_ = this.stiffness;
    }
}