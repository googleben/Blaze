using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace XNA3D
{
    //game state for splash screens
    class Splash : GameState
    {

        public static Texture2D publisherSplash;
        public static Texture2D developerSplash;

        const int splashFrames = 360; //total frames per splash
        const int fadeFrames = 60; //frames at the beginning and end to fade in/out
        const int endFrames = 10; //frames of dark at the end of each splash
        int frames;
        bool firstOver = false; //whether the first splash is finished and we should draw the second splash instead

        //draw cycle
        public void Draw(GraphicsDevice graphicsDevice, SpriteBatch sb)
        {
            Texture2D tex = firstOver ? developerSplash : publisherSplash;
            sb.Begin();
            sb.Draw(tex, new Microsoft.Xna.Framework.Rectangle(0, 0, 1920, 1080), 
                Color.Lerp(Color.Black, Color.White, Math.Min(Math.Min(1, (splashFrames-endFrames-frames)/(float)fadeFrames), frames/(float)fadeFrames)));
            sb.End();
        }

        //update cycle
        public GameState Update()
        {
            if (Blaze.WasPressed(Keys.Escape)) frames = splashFrames-1;
            frames++;
            if (frames == splashFrames) {
                if (firstOver) return new MainMenu();
                else {
                    firstOver = true;
                    frames = 0;
                }
            }
            return this;
        }
    }
}
