using UnityEngine;

public class CollisionZoneBox : CollisionZone
{
    #region InEditorVariables
    #endregion

    public Vector3 corner0, corner7;
    public CollisionZonePlane[] planes = new CollisionZonePlane[6];
    Bounds bounds;

    //Initialices the box bound
    public override void initialateBound()
    {
        bounds = this.GetComponent<BoxCollider>().bounds;

        corner0 = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z);
        Vector3 corner1 = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z);
        //Vector3 corner2 = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z);
        Vector3 corner3 = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z);
        Vector3 corner4 = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z);
        //Vector3 corner5 = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z);
        Vector3 corner6 = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z);
        corner7 = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z);

        //y
        planes[0] = createPlane(corner0, corner1, corner4);//corner5
        planes[1] = createPlane(corner3, corner6, corner7);//corner2
        //x
        planes[2] = createPlane(corner3, corner1, corner0);//corner2
        planes[3] = createPlane(corner7, corner6, corner4);//corner5
        //z
        planes[4] = createPlane(corner0, corner4, corner6);//corner2
        planes[5] = createPlane(corner1, corner3, corner7);//corner5
    }

    //Given a vertex return its penalty force
    public override Vector3 calculatePenaltyForce(Vector3 vertexPenalty)
    {
        //Calculates only two vectors for detect all planes collisions
        Vector3 distance0 = corner0 - vertexPenalty;
        Vector3 distance1 = corner7 - vertexPenalty;
        Vector3 maxForce = Vector3.zero;

        for (int planeId = 0; planeId < 6; planeId += 2)
        {
            Vector3 force0 = planes[planeId].calculatePenaltyForceFromVector(distance0);
            Vector3 force1 = planes[planeId + 1].calculatePenaltyForceFromVector(distance1);
            if (force0.magnitude == 0.0f || force1.magnitude == 0.0f)
            {
                return Vector3.zero;
            }
            else
            {
                maxForce += this.maxForce(force0, force1);
            }
        }
        return Vector3.zero - maxForce;
    }

    //creates one of the collision planes of the box
    private CollisionZonePlane createPlane(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        CollisionZonePlane plane = gameObject.AddComponent<CollisionZonePlane>();
        plane.initPlane(v0, v1, v2, penaltyConstant, offset);
        return plane;
    }

    //Returns the maximum of two vectors
    private Vector3 maxForce(Vector3 force0, Vector3 force1)
    {
        if (force0.magnitude > force1.magnitude)
        {
            return force0;
        }
        else
        {
            return force1;
        }
    }
}