using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNA3D
{

    public enum Axis
    {
        X, Y, Z
    }

    //Parent class for 3D Boxes
    public abstract class Box
    {

        internal static readonly int[] indices = new int[] {
            0, 1, 2, 2, 3, 0,
            4, 5, 1, 1, 0, 4,
            6, 5, 4, 4, 7, 6,
            2, 6, 7, 7, 3, 2,
            1, 5, 6, 6, 2, 1,
            7, 4, 0, 0, 3, 7
        };
        internal static readonly Vector3 up = new Vector3(0, 1, 0);
        internal static readonly Vector3 right = new Vector3(1, 0, 0);
        internal static readonly Vector3 back = new Vector3(0, 0, 1);

        public float Width { get; }
        public float Height { get; }
        public float Depth { get; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public float Left => X;
        public float Right => X + Width;
        public float Top => Y + Height;
        public float Bottom => Y;
        public float Front => Z;
        public float Back => Z + Depth;

        //rotation in radians
        public float Rotation { get; set; } = 0;

        //center of the box
        public Vector3 Center => new Vector3(X + Width / 2, Y + Height / 2, Z + Depth / 2);

        //world matrix for drawing purposes
        public Matrix WorldMatrix => Matrix.CreateRotationY(Rotation)*Matrix.CreateTranslation(X, Y, Z);


        public Box(float x, float y, float z, float width, float height, float depth)
        {
            X = x;
            Y = y;
            Z = z;
            Width = width;
            Height = height;
            Depth = depth;
        }

        //draw the box
        public abstract void Draw(GraphicsDevice device, Effect effect);

        //return a rectangle projected onto the viewing axis for collision detection
        public RectangleF Project(Axis axis)
        {
            return axis == Axis.X ? new RectangleF(Rotation >= MathHelper.PiOver2 && Rotation <= MathHelper.Pi  ? X-Width : X, Y, Width, Height) 
                : new RectangleF(Rotation >= MathHelper.PiOver2 && Rotation <= MathHelper.Pi ? Z-Depth : Z, Y, Depth, Height);
        }

        //draw text centered on this block
        public void DrawText(SpriteBatch sb, SpriteFont font, string text)
        {
            if (Playing.Instance.turning > 0) return;
            bool flip = Playing.Instance.dir == 90 || Playing.Instance.dir == 180;
            var player = Playing.Instance.player.box.Project(Playing.Instance.axis);
            float z = (5 * Playing.Instance.zoom);
            Vector2 center = Project(Playing.Instance.axis).Center;
            center.Y *= -1;
            if (flip) center.X *= -1;
            center.X -= player.Center.X * (flip ? -1 : 1);
            center.Y += player.Center.Y;
            center.X *= z;
            center.Y *= z;
            center += new Vector2(1920 / 2, 1080 / 2);
            var size = font.MeasureString(text)*(z/10f);
            sb.DrawString(font, text, new Vector2(center.X-size.X/2, center.Y-size.Y/2), Color.Red, 0, Vector2.Zero, (z/10f), SpriteEffects.None, 0);
        }

        public override string ToString()
        {
            return $"X: {X}, Y: {Y}, Z: {Z}, Width: {Width}, Height: {Height}, Depth: {Depth}";
        }

        //clone the box to create a new reference
        public abstract Box Clone();

    }

    //Box drawn using a solid color
    public class ColoredBox : Box
    {

        private VertexPositionColorNormal[] vertices;
        private ref VertexPositionColorNormal FrontBottomLeft => ref vertices[0];
        private ref VertexPositionColorNormal FrontTopLeft => ref vertices[1];
        private ref VertexPositionColorNormal FrontTopRight => ref vertices[2];
        private ref VertexPositionColorNormal FrontBottomRight => ref vertices[3];
        private ref VertexPositionColorNormal BackBottomLeft => ref vertices[4];
        private ref VertexPositionColorNormal BackTopLeft => ref vertices[5];
        private ref VertexPositionColorNormal BackTopRight => ref vertices[6];
        private ref VertexPositionColorNormal BackBottomRight => ref vertices[7];

        //the color to draw this box
        public Color color { get; }

        public ColoredBox(float x, float y, float z, float width, float height, float depth, Color color) : base(x, y, z, width, height, depth)
        {
            vertices = new VertexPositionColorNormal[]
            {
                new VertexPositionColorNormal(0, 0, 0, color, -1, -1, -1),
                new VertexPositionColorNormal(0, height, 0, color, -1, 1, -1),
                new VertexPositionColorNormal(width, height, 0, color, 1, 1, -1),
                new VertexPositionColorNormal(width, 0, 0, color, 1, -1, -1),
                new VertexPositionColorNormal(0, 0, depth, color, -1, -1, 1),
                new VertexPositionColorNormal(0, height, depth, color, -1, 1, 1),
                new VertexPositionColorNormal(width, height, depth, color, 1, 1, 1),
                new VertexPositionColorNormal(width, 0, depth, color, 1, -1, 1)
            };
            this.color = color;
        }

        //draw the box
        public override void Draw(GraphicsDevice device, Effect effect)
        {
            effect.Parameters["xWorld"].SetValue(WorldMatrix);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 8, indices, 0, 12, VertexPositionColorNormal.VertexDeclaration);
            }
        }

        //clone the box
        public override Box Clone()
        {
            return new ColoredBox(X, Y, Z, Width, Height, Depth, color);
        }

    }

    //Box drawn using a Texture2D for each face, evenly repeated across the surface
    public class TexturedBox : Box
    {
        private VertexPositionNormalTexture[] vertices;
        private ref VertexPositionNormalTexture FrontBottomLeft => ref vertices[0];
        private ref VertexPositionNormalTexture FrontTopLeft => ref vertices[1];
        private ref VertexPositionNormalTexture FrontTopRight => ref vertices[2];
        private ref VertexPositionNormalTexture FrontBottomRight => ref vertices[3];
        private ref VertexPositionNormalTexture BackBottomLeft => ref vertices[4];
        private ref VertexPositionNormalTexture BackTopLeft => ref vertices[5];
        private ref VertexPositionNormalTexture BackTopRight => ref vertices[6];
        private ref VertexPositionNormalTexture BackBottomRight => ref vertices[7];

        private Texture2D texture;

        private new int[] indices = new int[]
        {
            0,1,2,1,3,2, //back
            6,5,4,6,7,5, //front
            10,9,8,8,11,10, //right
            12,13,14,15,12,14 //left
        };

        public TexturedBox(float x, float y, float z, float width, float height, float depth, Texture2D texture) : base(x, y, z, width, height, depth)
        {
            var w = width / texture.Width;
            var h = height / texture.Height;
            var d = depth / texture.Width;
            vertices = new VertexPositionNormalTexture[] {
                new VertexPositionNormalTexture(new Vector3(0, height, 0), new Vector3(-1, 1, -1), new Vector2(0,0)),
                new VertexPositionNormalTexture(new Vector3(width, height, 0), new Vector3(1, 1, -1), new Vector2(w, 0)),
                new VertexPositionNormalTexture(new Vector3(0, 0, 0), new Vector3(-1, -1, -1), new Vector2(0, h)),
                new VertexPositionNormalTexture(new Vector3(width, 0, 0), new Vector3(1, -1, -1), new Vector2(w, h)),

                new VertexPositionNormalTexture(new Vector3(0, height, depth), new Vector3(-1, 1, 1), new Vector2(0,0)),
                new VertexPositionNormalTexture(new Vector3(width, height, depth), new Vector3(1, 1, 1), new Vector2(w, 0)),
                new VertexPositionNormalTexture(new Vector3(0, 0, depth), new Vector3(-1, -1, 1), new Vector2(0, h)),
                new VertexPositionNormalTexture(new Vector3(width, 0, depth), new Vector3(1, -1, 1), new Vector2(w, h)),

                new VertexPositionNormalTexture(new Vector3(width, 0, depth), new Vector3(1, -1, -1), new Vector2(0, h)),
                new VertexPositionNormalTexture(new Vector3(width, height, depth), new Vector3(1, 1, -1), new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(width, height, 0), new Vector3(1, 1, 1), new Vector2(d, 0)),
                new VertexPositionNormalTexture(new Vector3(width, 0, 0), new Vector3(1, -1, 1), new Vector2(d, h)),

                new VertexPositionNormalTexture(new Vector3(0, 0, depth), new Vector3(1, -1, -1), new Vector2(0, h)),
                new VertexPositionNormalTexture(new Vector3(0, height, depth), new Vector3(1, 1, -1), new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(0, height, 0), new Vector3(1, 1, 1), new Vector2(d, 0)),
                new VertexPositionNormalTexture(new Vector3(0, 0, 0), new Vector3(1, -1, 1), new Vector2(d, h)),
            };
            this.texture = texture;
        }

        //draw the box
        public override void Draw(GraphicsDevice device, Effect effect)
        {
            effect.CurrentTechnique = effect.Techniques["Textured"];
            effect.Parameters["xWorld"].SetValue(WorldMatrix);
            effect.Parameters["xTexture"].SetValue(texture);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                    vertices, 0, vertices.Length, indices, 0, 
                    indices.Length/3, VertexPositionNormalTexture.VertexDeclaration);
            }
            effect.CurrentTechnique = effect.Techniques["Colored"];
        }

        //clone the box
        public override Box Clone()
        {
            return new TexturedBox(X, Y, Z, Width, Height, Depth, texture);
        }

    }
}
