using UnityEngine;

public class Triangle
{
    //Class used in tetrahedron for draw the debug mesh and calculate the wind force
    #region InEditorVariables
    #endregion

    private Node[] nodes;
    private Spring[] springs;

    public Triangle(Node[] nodes_, Spring[] springs_)
    {
        this.nodes = nodes_;
        this.springs = springs_;
    }

    //Applies the corresponding wind force to all the nodes of the triangle
    public void computeWindForce(Vector3 windSpeed)
    {
        Vector3 triangleSurface = Vector3.Cross(this.springs[0].getDirection(), this.springs[1].getDirection());//*0.5
        Vector3 force = Vector3.Cross(-Vector3.Cross(windSpeed, triangleSurface), windSpeed) / 4.0f;
        foreach (Node node in this.nodes)
        {
            node.addForce(force);
        }
    }
}