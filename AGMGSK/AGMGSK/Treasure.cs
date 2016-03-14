using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AGMGSKv7
{
    public class Treasure : Model3D
    {
        public Treasure(Stage theStage, string label, string meshFile)  : base(theStage, label, meshFile) {
              // origin of wall on terrain
        }
        public Treasure(Stage theStage, string label, string meshFile, int xOffset, int zOffset)  : base(theStage, label, meshFile) 
	    {  }
    }
}
