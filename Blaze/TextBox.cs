using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNA3D
{
    //class representing an invisible box that displays text
    public class TextBox
    {

        public Box box;
        public string text;
        public int dir; //the angle at which to display this text, -1 for all angles

        public TextBox(float x, float y, float z, string text, int dir)
        {
            box = new ColoredBox(x, y, z, 0, 0, 0, Color.Red);
            this.text = text;
        }

        public TextBox(float x, float y, float z, string text) : this(x, y, z, text, -1) { }

        //draw the text
        public void Draw(SpriteBatch sb)
        {
            if (dir==-1 || dir==Playing.Instance.dir) box.DrawText(sb, Blaze.fonts["helpFont"], text);
        }

        //clone the textbox
        public TextBox Clone()
        {
            return new TextBox(box.X, box.Y, box.Z, text, dir);
        }

    }
}
