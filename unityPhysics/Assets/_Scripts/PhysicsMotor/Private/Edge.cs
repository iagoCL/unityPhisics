using System;

public class Edge : IComparable<Edge>
{
    //Class that stores a triangle edge to be added to the map (faster than strings)

    #region InEditorVariables
    #endregion

    public int id0;
    public int id1;
    public Edge(int id0_, int id1_)
    {
        if (id0_ < id1_)
        {//always stores the lowest in id0
            id0 = id0_;
            id1 = id1_;
        }
        else
        {
            id1 = id0_;
            id0 = id1_;
        }
    }
    //Compare methods for sorting in the dictionary
    public int CompareTo(Edge other)
    {
        return Compare(this, other);
    }
    public int Compare(Edge edge1, Edge edge2)
    {
        if ((edge1.id0 == edge2.id0 && edge1.id1 == edge2.id1) || (edge1.id0 == edge2.id1 && edge1.id1 == edge2.id0))
        {
            return 0;
        }
        else if ((edge1.id0 - edge2.id0) != 0)
            return edge1.id0 - edge2.id0;
        else
            return edge1.id1 - edge2.id1;
    }

    //DEBUG method:
    public override string ToString()
    {
        return ("Edge: {id0: " + id0 + " id1: " + id1 + "}");
    }
}