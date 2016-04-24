/*  
    Copyright (C) 2016 G. Michael Barnes
 
    The file NPAgent.cs is part of AGMGSKv7 a port and update of AGXNASKv6 from
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
    /// A non-playing character that moves.  Override the inherited Update(GameTime)
    /// to implement a movement (strategy?) algorithm.
    /// Distribution NPAgent moves along an "exploration" path that is created by the
    /// from int[,] pathNode array.  The exploration path is traversed in a reverse path loop.
    /// Paths can also be specified in text files of Vector3 values, see alternate
    /// Path class constructors.
    /// 
    /// 1/20/2016 last changed
    /// </summary>
    public class NPAgent : Agent
    {
        private NavNode nextGoal;
        private Path path;
        private int snapDistance = 20;  // this should be a function of step and stepSize
        // If using makePath(int[,]) set WayPoint (x, z) vertex positions in the following array
        private int[,] pathNode = { {505, 490}, {500, 500}, {490, 505},  // bottom, right
										 {435, 505}, {425, 500}, {420, 490},  // bottom, middle
										 {420, 450}, {425, 440}, {435, 435},  // middle, middle
                               {490, 435}, {500, 430}, {505, 420},  // middle, right
										 {505, 105}, {500,  95}, {490,  90},  // top, right
                               {110,  90}, {100,  95}, { 95, 105},  // top, left
										 { 95, 480}, {100, 490}, {110, 495},  // bottom, left
										 {495, 480} };								  // loop return

        /// <summary>
        /// Added Variables to the NPAgent Class of the Starter Kit
        /// </summary>
        private bool treasureHunting;
        private Treasure targetTreasure;
        private int numberOfTaggedTreasures;
        private Path treasurePath;
        private NavNode nextNodeToTreasure;
        private float tagDistance = 200.0f;

        /// <summary>
        /// Added Variables to the NPAgent Class for Project 2
        /// </summary>
        private NavGraph terrainGraph;
        private NavNode previousGoal;
        private bool onOriginalPath = true;
        private bool aStarCompleted = false;
        int count = 0;

        ///////////////////////////////////////////////////////////

        /// <summary>
        /// Create a NPC. 
        /// AGXNASK distribution has npAgent move following a Path.
        /// </summary>
        /// <param name="theStage"> the world</param>
        /// <param name="label"> name of </param>
        /// <param name="pos"> initial position </param>
        /// <param name="orientAxis"> initial rotation axis</param>
        /// <param name="radians"> initial rotation</param>
        /// <param name="meshFile"> Direct X *.x Model in Contents directory </param>
        public NPAgent(Stage theStage, string label, Vector3 pos, Vector3 orientAxis,
           float radians, string meshFile)
            : base(theStage, label, pos, orientAxis, radians, meshFile)
        {  // change names for on-screen display of current camera
            first.Name = "npFirst";
            follow.Name = "npFollow";
            above.Name = "npAbove";
            // path is built to work on specific terrain, make from int[x,z] array pathNode
            path = new Path(stage, pathNode, Path.PathType.LOOP); // continuous search path
            stage.Components.Add(path);
            nextGoal = path.NextNode;  // get first path goal
            previousGoal = nextGoal;
            agentObject.turnToFace(nextGoal.Translation);  // orient towards the first path goal
            // set snapDistance to be a little larger than step * stepSize
            snapDistance = (int)(1.5 * (agentObject.Step * agentObject.StepSize));
            treasureHunting = false;
            numberOfTaggedTreasures = 0;
            isCollidable = true;
        }

        //////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Check or Set if the NP Agent is Treasure Hunting
        /// </summary>
        public bool TreasureHunting
        {
            get { return treasureHunting; }
            set { treasureHunting = value; }
        }

        /// <summary>
        /// Check and Update the number of tagged treasures for the NPAgent
        /// </summary>
        public int NumberOfTaggedTreasures
        {
            get { return numberOfTaggedTreasures; }
            set { numberOfTaggedTreasures = value; }
        }

        /// <summary>
        /// Get or Set wether the NPAgent is on its default original path
        /// or on a path generated by A* Path Algorithm
        /// </summary>
        public bool OnOriginalPath
        {
            get { return onOriginalPath; }
            set { onOriginalPath = value; }
        }

        /// <summary>
        /// Method to set the Treasure object the NPAgent will be looking
        /// for in the simulated world.
        /// </summary>
        /// <param name="treasure"></param>
        public void lookForTreasure(ref Treasure treasure, Path newPath)
        {
            targetTreasure = treasure;
            treasurePath = newPath;
            nextNodeToTreasure = treasurePath.NextNode;
        }

        /// <summary>
        /// Set the Navigational Graph that the NPAgent will
        /// be used to navigate through the terrain. 
        /// </summary>
        /// <param name="graph"></param>
        public void setTerrainGraph(ref NavGraph graph)
        {
            terrainGraph = graph;
        }

        /////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Simple path following.  If within "snap distance" of a the nextGoal (a NavNode) 
        /// move to the NavNode, get a new nextGoal, turnToFace() that goal.  Otherwise 
        /// continue making steps towards the nextGoal.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            /////////////////////////////////////////////////////////////////////////////////////////
            //////// Modified this Method to allow a change of state to the NPAgent /////////////////

            float distance;
            float distanceToTreasure;

            // Check if the NPAgent is in Treasure Hunting Mode
            // If not in Treasure Hunting Mode, then in Pathfinding Mode
            if (!treasureHunting)
            {
                // If the NPAgent is not on its original path, find a A* path to the
                // next node of the NPAgents original path.
                if (!onOriginalPath)
                {
                    // If the A* Algorithm path has not been created, create one. 
                    if (!aStarCompleted)
                    {
                        NavNode closestNodeToNextGoal = terrainGraph.findClosestNavNodeInGraph(nextGoal.Translation);
                        List<NavNode> listPath = terrainGraph.aStarPathFinding(nextNodeToTreasure, closestNodeToNextGoal);
                        treasurePath = new Path(stage, listPath, Path.PathType.LOOP);
                        nextNodeToTreasure = treasurePath.NextNode;
                        aStarCompleted = true;
                    }

                    // Once the NPAgent navigates through all the NavNodes in the A* path to its next goal
                    // NavNode then the NPAgent is on its original path.
                    if (count >= treasurePath.Count)
                    {
                        onOriginalPath = true;
                        count = 0;
                    }

                    agentObject.turnToFace(nextNodeToTreasure.Translation);  // adjust to face nextGoal every move
                    // See if at or close to nextGoal, distance measured in 2D xz plane
                    distance = Vector3.Distance(
                        new Vector3(nextNodeToTreasure.Translation.X, 0, nextNodeToTreasure.Translation.Z),
                        new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z));
                    stage.setInfo(15,
                       string.Format("npAvatar:  location ({0:f0}, {1:f0}, {2:f0})  looking at ({3:f2}, {4:f2}, {5:f2})",
                          agentObject.Translation.X, agentObject.Translation.Y, agentObject.Translation.Z,
                          agentObject.Forward.X, agentObject.Forward.Y, agentObject.Forward.Z));
                    stage.setInfo(16,
                          string.Format("npAvatar:  nextGoal ({0:f0}, {1:f0}, {2:f0})  distance to next goal = {3,5:f2})",
                              nextNodeToTreasure.Translation.X, nextNodeToTreasure.Translation.Y, nextNodeToTreasure.Translation.Z, distance));
                    if (distance <= snapDistance)
                    {
                        // snap to nextGoal and orient toward the new nextGoal 
                        nextNodeToTreasure = treasurePath.NextNode;
                        count++;
                    }
                }

                // Otherwise the NPAgent is considered on its original path, and no A* path needs to be created to
                // get the NPAgent to its original path. The NPAgent just continues to move along its original path.
                else
                {
                    aStarCompleted = false;
                    agentObject.turnToFace(nextGoal.Translation);  // adjust to face nextGoal every move
                    // See if at or close to nextGoal, distance measured in 2D xz plane
                    distance = Vector3.Distance(
                        new Vector3(nextGoal.Translation.X, 0, nextGoal.Translation.Z),
                        new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z));
                    stage.setInfo(15,
                       string.Format("npAvatar:  location ({0:f0}, {1:f0}, {2:f0})  looking at ({3:f2}, {4:f2}, {5:f2})",
                          agentObject.Translation.X, agentObject.Translation.Y, agentObject.Translation.Z,
                          agentObject.Forward.X, agentObject.Forward.Y, agentObject.Forward.Z));
                    stage.setInfo(16,
                          string.Format("npAvatar:  nextGoal ({0:f0}, {1:f0}, {2:f0})  distance to next goal = {3,5:f2})",
                              nextGoal.Translation.X, nextGoal.Translation.Y, nextGoal.Translation.Z, distance));
                    if (distance <= snapDistance)
                    {
                        // snap to nextGoal and orient toward the new nextGoal
                        // keep track of the previous node in the path
                        previousGoal = nextGoal;
                        nextGoal = path.NextNode;
                    }
                }
            }

            // If the NPAgent is in treasure hunting mode, move along the treasure path to a treasure that has
            // been detected.
            else
            {
                // If the NPAgent's targeted treasure has not found treasure, look for treasure
                // and check if the treasure can be tagged.
                if (!targetTreasure.Found)
                {
                    // Orient towards treasure
                    //agentObject.turnToFace(targetTreasure.Location);
                    agentObject.turnToFace(nextNodeToTreasure.Translation);

                    // See if at or close to nextGoal, distance measured in 2D xz plane
                    distance = Vector3.Distance(
                        new Vector3(nextNodeToTreasure.Translation.X, 0, nextNodeToTreasure.Translation.Z),
                        new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z));

                    stage.setInfo(15,
                   string.Format("npAvatar:  location ({0:f0}, {1:f0}, {2:f0})  looking at ({3:f2}, {4:f2}, {5:f2})",
                      agentObject.Translation.X, agentObject.Translation.Y, agentObject.Translation.Z,
                      agentObject.Forward.X, agentObject.Forward.Y, agentObject.Forward.Z));
                    stage.setInfo(16,
                          string.Format("npAvatar:  nextGoal ({0:f0}, {1:f0}, {2:f0})  distance to next goal = {3,5:f2})",
                              nextNodeToTreasure.Translation.X, nextNodeToTreasure.Translation.Y, nextNodeToTreasure.Translation.Z, distance));

                    if (distance <= snapDistance)
                    {
                        // snap to nextGoal and orient toward the new nextGoal 
                        nextNodeToTreasure = treasurePath.NextNode;
                    }

                    // determine the distance between NP Agent and treasure
                    distanceToTreasure = Vector3.Distance(targetTreasure.Location, agentObject.Translation);

                    // If NP Agent is within tag distance, set treasure found, NP Agent to pathfinding
                    // mode (Not Treasure Hunting), and the number of tagged treasures increases.
                    if (distanceToTreasure <= tagDistance)
                    {
                        // Tag treasure
                        treasureHunting = false;
                        targetTreasure.Found = true;
                        numberOfTaggedTreasures++;
                    }
                }
                // If treasure is found, then set NP Agent to Pathfinding Mode
                else
                {
                    treasureHunting = false;
                }
            }

            base.Update(gameTime);  // Agent's Update();
        }
    }
}
