using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace XNA3D
{

    //enum representing difficulty settings
    public enum Difficulty
    {
        Easy, Medium, Hard
    }

    //interface representing the current methods for updating and drawing the game - menu, splash, playing, etc.
    public interface GameState
    {

        GameState Update();

        void Draw(GraphicsDevice graphicsDevice, SpriteBatch sb);

    }

    //base game class
    public class Blaze : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //collection of available fonts
        public static Dictionary<string, SpriteFont> fonts;

        public static Blaze instance;

        //target to render to, always 1920x1080
        RenderTarget2D target;

        GameState state;

        //current keyboard state
        public static KeyboardState down = new KeyboardState();
        //keyboard state previous update cycle
        public static KeyboardState wasDown = new KeyboardState();

        public Settings settings;
        //list of resoultions compatible with this screen
        public static List<Point> resolutions;

        public int screenHeight;
        public int screenWidth;

        public static Texture2D platform;
        public static Texture2D black;

        public Blaze()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.graphics.GraphicsProfile = GraphicsProfile.HiDef;
            instance = this;
            this.IsMouseVisible = true;
        }

        //initialize data used during gameplay
        protected override void Initialize() {

            state = new Splash();
            target = new RenderTarget2D(GraphicsDevice, 1920, 1080, true, SurfaceFormat.Rgba64, DepthFormat.Depth24Stencil8);

            resolutions = new List<Point>();
            foreach (var d in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes) {
                var r = new Point(d.Width, d.Height);
                if (!resolutions.Contains<Point>(r) && d.AspectRatio == 16f / 9f) resolutions.Add(r);
            }
            resolutions.Sort((a, b) => a.X - b.X);

            base.Initialize();
        }

        //load assets used during gameplay
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //load settings
            var ser = new XmlSerializer(typeof(Settings));
            try {
                Program.log.Log("Loading settings");
                Stream f = File.OpenRead(@"Content/Settings.xml");
                settings = (Settings)ser.Deserialize(f);
                f.Close();
                Program.log.Log("Loaded settings");
            } catch (FileNotFoundException) {
                Program.log.Log("No settings file, generating a new one");
                settings = new Settings();
            }
            ApplySettings();
            
            //load Content folder
            fonts = new Dictionary<string, SpriteFont>();
            Playing.effect = Content.Load<Effect>("effects");
            fonts["helpFont"] = Content.Load<SpriteFont>("font");
            fonts["menuFont"] = Content.Load<SpriteFont>("menuFont");
            Splash.publisherSplash = Content.Load<Texture2D>("PublisherSplash");
            Splash.developerSplash = Content.Load<Texture2D>("DeveloperSplash");
            platform = Content.Load<Texture2D>("platform");
            XNA3D.Exit.texture = Content.Load<Texture2D>("door");
            black = Content.Load<Texture2D>("black");
            Menu.click = Content.Load<SoundEffect>("button");
            Player.jump = Content.Load<SoundEffect>("jump");
        }

        //empty implementation of UnloadContent -- all content is used for the duration of the game
        protected override void UnloadContent()
        {
            
        }

        //update cycle
        protected override void Update(GameTime gameTime)
        {
            wasDown = down;
            down = Keyboard.GetState();

            state = state.Update();

            base.Update(gameTime);
        }

        //draw cycle
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(target);
            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //GraphicsDevice.Clear(ClearOptions.Target|ClearOptions.DepthBuffer, Color.Black, 1, 0);
            GraphicsDevice.Clear(Color.Black);

            state.Draw(GraphicsDevice, spriteBatch);

            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin();
            spriteBatch.Draw(target, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        //apply changed settings and save to file
        public void ApplySettings()
        {
            if (settings == null) return;
            graphics.PreferredBackBufferHeight = settings.screenHeight;
            graphics.PreferredBackBufferWidth = settings.screenWidth;
            screenHeight = settings.screenHeight;
            screenWidth = settings.screenWidth;
            graphics.IsFullScreen = settings.fullscreen;
            graphics.ApplyChanges();

            SaveSettings();
        }

        //save settings to file
        public void SaveSettings()
        {
            Program.log.Log("Saving settings");
            var set = new XmlSerializer(typeof(Settings));
            File.Delete(@"Content/Settings.xml");
            Stream f = File.OpenWrite(@"Content/Settings.xml");
            set.Serialize(f, settings);
            f.Close();
            Program.log.Log("Saved settings");
        }

        //wraper for Exit() function to log the game exiting
        public new void Exit()
        {
            Program.log.Log("Exit called");
            base.Exit();
        }

        //helper method to check if a key is down but wasn't last frame
        public static bool WasPressed(Keys key)
        {
            return down.IsKeyDown(key) && !wasDown.IsKeyDown(key);
        }

    }
}
