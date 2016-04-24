/*  
    Copyright (C) 2016 G. Michael Barnes
 
    The file NavNode.cs is part of AGMGSKv7 a port and update of AGXNASKv6 from
    MonoGames 3.2 to MonoGames 3.4  

    AGMGSKv7 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//#if MONOGAMES //  true, build for MonoGames
//   using Microsoft.Xna.Framework.Storage; 
//#endif
#endregion

namespace AGMGSKv7 {
   
/// <summary>
/// A WayPoint or Marker to be used in path following or path finding.
/// Four types of WAYPOINT:
/// <list type="number"> WAYPOINT, a navigatable terrain vertex </list>
/// <list type="number"> PATH, a node in a path (could be the result of A*) </list>
/// <list type="number"> OPEN, a possible node to follow in an A*path</list>
/// <list type="number"> CLOSED, a node that has been evaluated by A* </list>
 
/// 
/// 2/14/2012  last update
/// </summary>
public class NavNode : IComparable<NavNode> {
        public enum NavNodeEnum { WAYPOINT, PATH, OPEN, CLOSED, VERTEX };
        private double distance;  // can be used with A* path finding.
        private Vector3 translation;
        private NavNodeEnum navigatable;
        private Vector3 nodeColor;

        /// <summary>
        /// Variables added that are used for creation of the 
        /// graph and implementation of A* Algorithm
        /// </summary>
        private List<NavNode> adjacent;
        private NavNode pathPredecessor;
        private float distanceFromSource, distanceToGoal;
        private float cost;
        private float offset;
        private float x, z;
        //////////////////////////////////////////////////////////

        // constructors

        /// <summary>
        /// Make a VERTEX NavNode
        /// </summary>
        /// <param name="pos"> location of WAYPOINT</param>
        public NavNode(Vector3 pos)
        {
            translation = pos;
            Navigatable = NavNodeEnum.WAYPOINT;
            x = pos.X;
            z = pos.Z;
            distanceFromSource = 0.0f;
            distanceToGoal = 0.0f;
            offset = 0.0f;
            adjacent = new List<NavNode>();
        }

        /// <summary>
        /// Make a WAYPOINT and set its Navigational type
        /// </summary>
        /// <param name="pos"> location of WAYPOINT</param>
        /// <param name="nType"> Navigational type {VERTEX, WAYPOINT, A_STAR, PATH} </param>
        public NavNode(Vector3 pos, NavNodeEnum nType)
        {
            translation = pos;
            Navigatable = nType;
            x = pos.X;
            z = pos.Z;
            distanceFromSource = 0.0f;
            distanceToGoal = 0.0f;
            offset = 0.0f;
            adjacent = new List<NavNode>();
        }

        /// <summary>
        /// Make a WAYPOINT, set its Navigational type, and give it an offset
        /// </summary>
        /// <param name="pos"> location of WAYPOINT</param>
        /// <param name="nType"> Navigational type {VERTEX, WAYPOINT, A_STAR, PATH} </param>
        /// <param name="newOffset"> Maximum Distance between another WAYPOINT</param>
        public NavNode(Vector3 pos, NavNodeEnum nType, float newOffset)
        {
            translation = pos;
            Navigatable = nType;
            x = pos.X;
            z = pos.Z;
            distanceFromSource = 0.0f;
            distanceToGoal = 0.0f;
            offset = newOffset;
            adjacent = new List<NavNode>();
        }

        // properties

        /// <summary>
        /// Nodes color value
        /// </summary>
        public Vector3 NodeColor
        {
            get { return nodeColor; }
        }

        /// <summary>
        /// Get or Set the distance between nodes.
        /// </summary>
        public Double Distance
        {
            get { return distance; }
            set { distance = value; }
        }

        /// <summary>
        /// Get or Set the distance of the current node from the source
        /// of the start of the A* implementation
        /// </summary>
        public float DistanceFromSource
        {
            get { return distanceFromSource; }
            set { distanceFromSource = value; }
        }

        /// <summary>
        /// Get or Set the distance of the current node to the goal
        /// of the destination of the A* implementation
        /// </summary>
        public float DistanceToGoal
        {
            get { return distanceToGoal; }
            set { distanceToGoal = value; }
        }

        /// <summary>
        /// Get the adjacency list for the given node
        /// </summary>
        public List<NavNode> Adjacent
        {
            get { return adjacent; }
        }

        /// <summary>
        /// Get or Set the predecessor of a given node
        /// </summary>
        public NavNode PathPredecessor
        {
            get { return pathPredecessor; }
            set { pathPredecessor = value; }
        }

        /// <summary>
        /// Get or Set the cost of a node. The cost is with respect
        /// to distance. This distance is calculated in the A* Algorithm.
        /// </summary>
        public float Cost
        {
            get { return cost; }
            set { cost = value; }
        }

        /// <summary>
        /// Get or Set the x coordinate of a node
        /// </summary>
        public float X
        {
            get { return x; }
            set { x = value; }
        }

        /// <summary>
        /// Get or Set the z coordinate of a node
        /// </summary>
        public float Z
        {
            get { return z; }
            set { z = value; }
        }

        /// <summary>
        /// Get or Set the offset of a node
        /// </summary>
        public float Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        /// <summary>
        /// When changing the Navigatable type the WAYPOINT's nodeColor is 
        /// also updated.
        /// </summary>
        public NavNodeEnum Navigatable
        {
            get { return navigatable; }
            set
            {
                navigatable = value;
                switch (navigatable)
                {
                    case NavNodeEnum.WAYPOINT: nodeColor = Color.Yellow.ToVector3(); break;  // yellow
                    case NavNodeEnum.PATH: nodeColor = Color.White.ToVector3(); break;  // blue
                    case NavNodeEnum.OPEN: nodeColor = Color.Blue.ToVector3(); break;  // white
                    case NavNodeEnum.CLOSED: nodeColor = Color.Red.ToVector3(); break;  // red
                    case NavNodeEnum.VERTEX: nodeColor = Color.Black.ToVector3(); break;
                }
            }
        }

        /// <summary>
        /// Get the translation of a node.
        /// </summary>
        public Vector3 Translation
        {
            get { return translation; }
        }

        /////////////////////////////////////////////////////////////////////////
        // methods

        /// <summary>
        /// Useful in A* path finding 
        /// when inserting into an min priority queue open set ordered on distance
        /// </summary>
        /// <param name="n"> goal node </param>
        /// <returns> usual comparison values:  -1, 0, 1 </returns>
        public int CompareTo(NavNode n)
        {
            if (distance < n.Distance) return -1;
            else if (distance > n.Distance) return 1;
            else return 0;
        }

        /// <summary>
        /// Add a Node to a nodes adjacency list.
        /// There is a check for null values, only initialized
        /// nodes may be added to the list.
        /// </summary>
        /// <param name="adjacentNode">NavNode that will be added</param>
        public void addAdjacentNode(NavNode adjacentNode)
        {
            if (adjacentNode != null)
            {
                adjacent.Add(adjacentNode);
            }
        }

    }
}
