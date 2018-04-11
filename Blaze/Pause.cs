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
    //gamestate for being paused
    class Pause : GameState
    {

        Playing playing;

        Menu m;

        public Pause(Playing playing)
        {
            this.playing = playing;
            m = new PauseMenu();
        }

        //draw the pause menu on top of the current state of the game
        public void Draw(GraphicsDevice graphicsDevice, SpriteBatch sb)
        {
            playing.Draw(graphicsDevice, sb);
            sb.Begin();
            sb.Draw(Blaze.black, new Rectangle(0, 0, 1920, 1080), new Color(Color.White, .75f));
            sb.End();
            m.Draw(graphicsDevice, sb);
        }

        //run update cycle
        public GameState Update()
        {
            if (!Blaze.wasDown.IsKeyDown(Keys.Escape) && Blaze.down.IsKeyDown(Keys.Escape)) return playing;
            var x = m.Update();
            if (!(x is PauseMenu || x is SettingsMenu)) return x;
            m = (Menu)x;
            return this;
        }
    }
}
