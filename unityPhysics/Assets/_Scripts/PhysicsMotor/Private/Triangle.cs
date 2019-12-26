using UnityEngine;

public class Triangle
{
    //Class used in tetrahedron for draw the debug mesh and calculate the wind force
    #region InEditorVariables
    #endregion

    public Node[] nodes = new Node[3];
    public Spring[] springs = new Spring[3];

    public Triangle() { }

    //Applies the corresponding wind force to all the nodes of the triangle
    public void computeWindForce(Vector3 windSpeed)
    {
        Vector3 triangleSurface = Vector3.Cross(this.springs[0].direction, this.springs[1].direction);//*0.5
        Vector3 force = Vector3.Cross(Vector3.Cross(windSpeed, triangleSurface), triangleSurface.normalized) / 6.0f;
        foreach (Node node in this.nodes)
        {
            //Wind_Force = Wind_Speed * Triangle_Normal * Triangle_Surface * Wind_Intensity * Triangle_Normal * Velocity_Magnitude.
            node.force += force * Mathf.Min(node.velocity.magnitude, 2.0f);
        }
    }
}