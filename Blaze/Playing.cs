using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XNA3D
{
    //gamestate for playing the game
    class Playing : GameState
    {

        //the lowest point the player can go before respawning
        public float bottom;
        public Vector3 spawn;

        public int turning; //frames left of turning
        int turningDir; // 1 for right, -1 for left
        const int turningFrames = 90;
        public int dir = 0; //degrees around the player the camera is rotated

        public float zoom = 2;
        float baseZoom;
        int zoomDir;
        float zoomLeft;
        const float zoomFrames = 60;

        public static Effect effect;

        public List<Box> terrain;
        public List<Light> lights;
        public List<TextBox> text;

        public Player player;

        public Axis axis;
        public Axis oldAxis;

        Vector3 CameraPos;

        public static Playing Instance { get => instance==null ? new Playing() : instance; private set { instance = value; } }
        private static Playing instance;

        Level currentLevel;
        public int levelNum = 0;

        public bool justTurned = false; // whether or not the previous frame was a turning frame

        public Exit exit;

        public Playing(int level)
        {
            LoadLevel(level);

            Instance = this;
        }

        public Playing() : this(0) { }

        //load a level from its number
        public void LoadLevel(int level)
        {
            Program.log.Log("Loading level " + level);
            levelNum = level;
            if (level >= Level.levels.Count) return;
            LoadLevel(Level.levels[level]);
            Program.log.Log("Loaded level " + level);
        }

        //load a level from its data
        public void LoadLevel(Level level)
        {
            terrain = level.terrain.ConvertAll(b => b.Clone());
            lights = level.lights.ConvertAll(l => l.Clone());
            text = level.text.ConvertAll(t => t.Clone());
            spawn = level.spawn;
            exit = level.exit;
            currentLevel = level;
            bottom = terrain.ConvertAll(b => b.Bottom).Min()-100;

            dir = 0;
            zoom = 2;
            axis = Axis.X;

            player = new Player(spawn.X, spawn.Y, spawn.Z);
        }

        //update cycle
        public GameState Update()
        {
            if (levelNum >= Level.levels.Count) return new MainMenu();
            var state = Blaze.down;
            var wasDown = Blaze.wasDown;

            if (Blaze.WasPressed(Keys.Escape)) return new Pause(this);

            if (zoomLeft > 0) {
                zoomLeft--;
                if (zoomLeft == 0) zoom = baseZoom + zoomDir;
            }

            //zoom controls
            if (Blaze.WasPressed(Keys.Up) && zoomLeft == 0 && zoom < 4) {
                zoomLeft = zoomFrames;
                zoomDir = 1;
                baseZoom = zoom;
            }
            if (Blaze.WasPressed(Keys.Down) && zoomLeft == 0 && zoom > 1) {
                zoomLeft = zoomFrames;
                zoomDir = -1;
                baseZoom = zoom;
            }

            if (turning > 0) {
                turning--;
                dir += turningDir;
                if (dir == -1) dir = 359;
                if (dir == 360) dir = 0;
                justTurned = true;
                return this;
            }

            //player controls
            var v = 0f;
            if (state.IsKeyDown(Keys.A)) v = dir == 90 || dir == 180 ? 1 : -1;
            if (state.IsKeyDown(Keys.D)) v = dir == 90 || dir == 180 ? -1 : 1;
            if (axis == Axis.X) {
                player.velocity.X = v;
                player.velocity.Z = 0;
            }
            if (axis == Axis.Z) {
                player.velocity.X = 0;
                player.velocity.Z = v;
            }
            if (state.IsKeyDown(Keys.Space)) player.Jump();


            //camera controls
            if (state.IsKeyDown(Keys.Right)) {
                turningDir = 1;
                turning = turningFrames;
                oldAxis = axis;
                axis = axis == Axis.X ? Axis.Z : Axis.X;
                return this;
            }
            if (state.IsKeyDown(Keys.Left)) {
                turningDir = -1;
                turning = turningFrames;
                oldAxis = axis;
                axis = axis == Axis.X ? Axis.Z : Axis.X;
                return this;
            }

            wasDown = state;

            player.Update(terrain);

            justTurned = false;

            if (player.box.Bottom < bottom) LoadLevel(currentLevel);

            return this;
        }

        //draw cycle
        public void Draw(GraphicsDevice graphicsDevice, SpriteBatch sb)
        {
            float angle = MathHelper.ToRadians(dir);
            CameraPos = new Vector3((float)Math.Sin(angle) * 1000, 0, (float)Math.Cos(angle) * 1000) + player.Center;

            if (zoomLeft > 0) zoom = baseZoom + (float)Math.Cos(zoomLeft / zoomFrames * Math.PI / 2) * zoomDir;

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.CullClockwiseFace;
            graphicsDevice.RasterizerState = rs;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            effect.Parameters["xView"].SetValue(Matrix.CreateLookAt(CameraPos, player.Center, new Vector3(0, 1, 0)));
            //Perspective projection for debugging:
            //effect.Parameters["xProjection"].SetValue(Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, graphicsDevice.Viewport.AspectRatio, 0.1f, 10000));
            effect.Parameters["xProjection"].SetValue(Matrix.CreateOrthographic(1920 / (5 * zoom), 1080 / (5 * zoom), 0, 10000));
            effect.Parameters["xEnableLighting"].SetValue(true);
            effect.Parameters["xLightDirection"].SetValue(new Vector3(0f, 1f, 1f));
            effect.Parameters["xAmbient"].SetValue(0f);
            effect.Parameters["xEnablePointLight"].SetValue(true);
            effect.Parameters["xXLight"].SetValue(axis == Axis.X || turning > 0);
            effect.Parameters["xZLight"].SetValue(axis == Axis.Z || turning > 0);

            //set up light parameters
            int light = 0;
            foreach (Light l in lights) {
                if (l.lit) {
                    ++light;
                    effect.Parameters["xLight" + light + "Pos"].SetValue(l.Center);
                    effect.Parameters["xLight"+light+"Attenuation"].SetValue(110f);
                    effect.Parameters["xLight"+light+"Falloff"].SetValue(2f);
                }
            }
            light++;
            for (; light<=3; light++) {
                effect.Parameters["xLight" + light + "Pos"].SetValue(new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity));
            }

            //draw objects and player
            foreach (var t in terrain) t.Draw(graphicsDevice, effect);
            foreach (var l in lights) l.Draw(graphicsDevice, effect, sb);
            exit.Draw(graphicsDevice, effect, sb);

            effect.CurrentTechnique = effect.Techniques["Player"];
            player.Draw(graphicsDevice, effect);

            //draw text prompts
            sb.Begin();
            foreach (Light l in lights) if (l.drawText) l.box.DrawText(sb, Blaze.fonts["helpFont"], l.lit ? "Press E to quench" : "Press E to light");
            foreach (var box in text) box.Draw(sb);
            exit.DrawText(sb);
            sb.End();
        }

    }
}
