using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNA3D
{
    //rectangle with float values
    public struct RectangleF
    {

        public float x;
        public float Right => x + width;
        public float y;
        public float Top => y + height;
        public float width;
        public float height;

        //bottom left of the rectangle
        public Vector2 Position => new Vector2(x, y);
        public Vector2 Center => new Vector2(x + width / 2, y + height / 2);

        public RectangleF(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public override string ToString()
        {
            return x + ", " + y + " " + width + ":" + height;
        }

        //returns a rectangle representing the intersection of this and another rectangle (common area)
        public RectangleF GetIntersection(RectangleF other)
        {
            float dx = x - other.x;
            float dy = y - other.y;
            float widtht = width + other.width;
            float heightt = height + other.height;
            if (Math.Abs(dx) > widtht || Math.Abs(dy) > heightt) return new RectangleF(0, 0, 0, 0);
            if ((y > other.Top || Top < other.y) || (x > other.Right || Right < other.x)) return new RectangleF(0, 0, 0, 0);
            var ansX = dx < 0 ? other.x : x;
            var ansY = dy < 0 ? other.y : y;
            var ansW = Math.Min(Right, other.Right) - Math.Max(x, other.x);
            var ansH = Math.Min(Top, other.Top) - Math.Max(y, other.y);
            if (ansW < 0 || ansH < 0) return new RectangleF(0, 0, 0, 0);
            return new RectangleF(ansX, ansY, ansW, ansH);
        }

    }
}
