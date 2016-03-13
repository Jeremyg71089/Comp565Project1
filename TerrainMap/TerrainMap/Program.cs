/*
 * Program.cs is the starting point for AGMGSK applications.
 * 
 * For Windows users of the XNA 4 API (not MonoGames 3.4) 
 * you must select Project | Properties | XNA Game Studio
 * and select Game Profile:  "Use HiDef".  
 * Do this once at initial project configuration.
 *
 * For all projects submission you should:
 * 1.  Delete the file game1.cs  from your project
 * 2.  Edit the last lines in this comment appropriately
 *			so that each group members name and email are listed alphabetically
 *				for example:
 *					Group member:  Mike Barnes  renzo@csun.edu
 *					Group member:  Iam Smart	IamSmart@mycsun.edu 
 *			the project is labeled
 *       the class and semester are specified
 *
 * 
 * Group member:  Mike Barnes
 * Project 1  Terrain Map
 * Comp 565 Spring 2016
 */

using System;

namespace TerrainMap {
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new TerrainMap())
                game.Run();
        }
    }
#endif
}
