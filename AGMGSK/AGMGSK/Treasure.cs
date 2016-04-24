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
        protected bool found;

        // Treasure's location
        protected Vector3 location;

        // Object3D representation for Treasure
        protected Object3D treasureObject = null;


        // Constructor Method
        public Treasure(Stage stage, string label, string meshFile, Vector3 position, Vector3 orientationAxis, float radians)
            : base(stage, label, meshFile)
        {
            // Create a Object3D for this treasure
            isCollidable = false;
            treasureObject = addObject(position, orientationAxis, radians);
            location = position;
            found = false;
        }

        
        public Object3D TreasureObject
        {
            get { return treasureObject; }
        }

        
        public bool Found
        {
            get { return found; }
            set { found = value; }
        }
        
        public Vector3 Location
        {
            get { return location; }
        }

        public override string ToString()
        {
            return "Name: " + treasureObject.Name + "Location: " + location.ToString() + " Found: " + found;
        }

        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
