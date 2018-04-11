using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNA3D
{
    //class representing the "exit" in each level
    class Exit
    {

        public static Texture2D texture;

        //the box representing this exit
        public Box box;
        //whether or not to draw help text
        public bool drawText;

        public Exit(float x, float y, float z)
        {
            box = new TexturedBox(x, y, z, 32, 48, 32, texture);
        }

        //draw the exit
        public void Draw(GraphicsDevice gd, Effect effect, SpriteBatch sb)
        {
            box.Draw(gd, effect);
        }

        //draw help text
        public void DrawText(SpriteBatch sb)
        {
            if (drawText) box.DrawText(sb, Blaze.fonts["helpFont"], "Press E to enter");
        }

        //clone the exit
        public Exit Clone()
        {
            return new Exit(box.X, box.Y, box.Z);
        }

    }
}
