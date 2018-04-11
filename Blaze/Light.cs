using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XNA3D
{
    //class representing a light
    class Light
    {
        public Box box;
        //whether the light is lit currently
        public bool lit;
        //whether or not to draw help text
        public bool drawText;

        //center of the light
        public Vector3 Center => box.Center;

        public Light(float x, float y, float z, bool lit)
        {
            box = new ColoredBox(x, y, z, 5, 5, 5, Color.Yellow);
            this.lit = lit;
        }

        public Light(float x, float y, float z) : this(x, y, z, false) { }

        //draw the light
        public void Draw(GraphicsDevice device, Effect effect, SpriteBatch sb)
        {
            box.Draw(device, effect);
        }

        //clone the light
        public Light Clone()
        {
            return new Light(box.X, box.Y, box.Z, lit);
        }

    }
}
