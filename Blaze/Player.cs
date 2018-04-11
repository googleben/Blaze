using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNA3D
{
    public class Player
    {

        public Box box;

        const float gravity = .5f;

        public Vector3 velocity;

        bool hadYCollision = false; //whether the player is on top of a surface (can jump)

        public Vector3 Center => box.Center;

        static int lights = 2;

        public static SoundEffect jump;

        int walkTimer = 0;
        const int walkTime = 30;

        List<Box> ignored = new List<Box>(); //boxes that shouldn't be collided with (after a camera turn)

        List<Box> prevCollisions = new List<Box>();

        bool canCollide = true;

        public Player(float x, float y, float z)
        {
            box = new ColoredBox(x, y, z, 10, 10, 10, Color.Blue);
            switch(Blaze.instance.settings.difficulty)
            {
                case Difficulty.Easy:
                    lights = 4;
                    break;
                case Difficulty.Medium:
                    lights = 3;
                    break;
                case Difficulty.Hard:
                    lights = 2;
                    break;
            }
        }

        public void Update(List<Box> terrain)
        {
            Axis axis = Playing.Instance.axis;
            box.X += velocity.X;
            box.Y += velocity.Y;
            box.Z += velocity.Z;

            //handle walking sounds
            if (hadYCollision && (velocity.X != 0 || velocity.Z != 0)) {
                if (walkTimer == 0) {
                    walkTimer = walkTime;
                    jump.Play(1f, .5f, 0f);
                } else if (walkTimer == walkTime / 2) {
                    jump.Play(1f, .4f, 0f);
                    walkTimer--;
                } else walkTimer--;
            } else walkTimer = walkTime;

            HandleCollisions(terrain, axis);

            if (velocity.Y > -3) velocity.Y -= gravity;
            //handle turning lights on/off and drawing text prompts
            foreach (Light light in Playing.Instance.lights) {
                var intersection = box.Project(axis).GetIntersection(light.box.Project(axis));
                if (intersection.width==0 || intersection.height==0) {
                    light.drawText = false;
                } else {
                    
                    light.drawText = true;
                    if (Blaze.WasPressed(Keys.E)) {
                        if (!light.lit) {
                            if (lights>0) {
                                lights--;
                                light.lit = true;
                            }
                        } else {
                            lights++;
                            light.lit = false;
                        }
                    }
                }

            }
            var ex = box.Project(axis).GetIntersection(Playing.Instance.exit.box.Project(axis));
            if (ex.width != 0 && ex.height != 0 && canCollide) {
                if (Blaze.WasPressed(Keys.E)) {
                    Blaze.instance.settings.levelUnlocked = Math.Max(Blaze.instance.settings.levelUnlocked, Playing.Instance.levelNum + 1);
                    Blaze.instance.SaveSettings();
                    Playing.Instance.LoadLevel(Playing.Instance.levelNum + 1);
                }
                Playing.Instance.exit.drawText = true;
            } else Playing.Instance.exit.drawText = false;
        }

        private void HandleCollisions(List<Box> terrain, Axis axis)
        {
            
            RectangleF me = box.Project(axis);
            canCollide = Playing.Instance.lights.Where(l => l.lit)
                .Where(l => Vector2.Distance(me.Position, l.box.Project(axis).Position) < 100)
                .Count()>0;
            if (!canCollide) return;
            hadYCollision = false;
            List<Box> collisions = new List<Box>();
            foreach (Box b in terrain) {
                RectangleF other = b.Project(axis);
                var intersection = other.GetIntersection(me);
                if (intersection.width == 0 || intersection.height == 0) continue;
                collisions.Add(b);
                if (ignored.Contains(b)) continue;
                if (Playing.Instance.justTurned && !prevCollisions.Contains(b) && intersection.height>.5f) {
                    ignored.Add(b);
                    continue;
                }
                if (intersection.width < intersection.height) {
                    //intersection on the x axis
                    if (me.x < other.x) {
                        if (axis == Axis.X) box.X -= intersection.width;
                        else box.Z -= intersection.width;
                    } else {
                        if (axis == Axis.X) box.X += intersection.width;
                        else box.Z += intersection.width;
                    }
                    velocity.X = 0;
                    velocity.Z = 0;
                } else {
                    //intersection on the y axis
                    if (me.y < other.y) box.Y = other.y - me.height;
                    else {
                        box.Y = other.Top;
                        hadYCollision = true;
                    }
                    velocity.Y = 0;

                }
                //handle depth changes due to colliding with a non-algined box
                if (axis == Axis.X) {
                    if (box.Back < b.Z) {
                        box.Z = b.Z;
                    } else if (box.Z > b.Back) {
                        box.Z = b.Back-box.Depth;
                    }
                } else {
                    if (box.Left > b.Right) {
                        box.X = b.Right-box.Width;
                    } else if (box.Right < b.Left) {
                        box.X = b.X;
                    }
                }
                me = box.Project(axis);
            }
            //remove boxes that should no longer be ignored
            for (int i = 0; i < ignored.Count; i++) if (!collisions.Contains(ignored[i])) ignored.RemoveAt(i--);
            prevCollisions = collisions;
        }

        public void Jump()
        {
            if (!hadYCollision) return;
            jump.Play(1f, -.5f, 0);
            velocity.Y = 5;
        }

        public void Draw(GraphicsDevice device, Effect effect)
        {
            //box.Rotation = MathHelper.ToRadians(Playing.Instance.dir);
            box.Draw(device, effect);
        }

    }
}
