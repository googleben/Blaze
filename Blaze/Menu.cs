using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace XNA3D
{
    //individual option in a menu
    internal class MenuOption
    {
        public delegate GameState MakeState();

        //function to create the next GameState after selecting this option
        public MakeState makeState;

        public string text;

        public MenuOption(string text, MakeState makeState)
        {
            this.makeState = makeState;
            this.text = text;
        }
    }

    //base class for all menus
    internal abstract class Menu : GameState
    {
        private readonly SpriteFont font;

        public static KeyboardState lastState = Blaze.down;

        protected int index;

        protected List<MenuOption> options;

        protected readonly Menu fromMenu;

        float offset = 0;

        public static SoundEffect click;

        public Menu(Menu fromMenu)
        {
            font = Blaze.fonts["menuFont"];
            index = 0;
            options = new List<MenuOption>();
            this.fromMenu = fromMenu;
            GenerateOptions();
        }

        //draw the menu
        public void Draw(GraphicsDevice graphicsDevice, SpriteBatch sb)
        {
            sb.Begin();
            var ypos = 20;
            var space = ypos;
            float y = 0;
            for (var i = 0; i < index; i++) {
                var opt = options[i];
                var m = font.MeasureString(opt.text);
                y = m.Y;
                ypos += space + (int)m.Y;
            }
            if (ypos > 720 - y) offset = ypos - (720 - y);
            else offset = 0;
            ypos = 2;
            for (var i = 0; i < options.Count; i++) {
                var opt = options[i];
                var m = font.MeasureString(opt.text);
                sb.DrawString(font, opt.text, new Vector2(1920 / 2 - m.X / 2, ypos - offset),
                    i == index ? Color.White : Color.Gray);
                ypos += space + (int)m.Y;
            }
            sb.End();
        }
    
        //run keypress checks
        public virtual GameState Update()
        {
            var state = Blaze.down;
            lastState = Blaze.wasDown;
            if (state.IsKeyDown(Keys.Enter) && !lastState.IsKeyDown(Keys.Enter)) {
                lastState = state;
                click.Play(1, -1, 0);
                return options[index].makeState.Invoke();
            }
            if (state.IsKeyDown(Keys.Escape) && !lastState.IsKeyDown(Keys.Escape)) {
                lastState = state;
                click.Play(1, -1, 0);
                if (fromMenu != null) return fromMenu;
                Blaze.instance.Exit();
            }
            if (state.IsKeyDown(Keys.Down) && !lastState.IsKeyDown(Keys.Down)) {
                index++;
                click.Play();
                if (index == options.Count) index = 0;
            }
            if (state.IsKeyDown(Keys.Up) && !lastState.IsKeyDown(Keys.Up)) {
                index--;
                click.Play();
                if (index == -1) index = options.Count - 1;
            }

            //lastState = state;

            return this;
        }

        //reload the list of available options
        public void RegenerateOptions()
        {
            options.Clear();
            GenerateOptions();
        }

        //load the list of available options
        public virtual void GenerateOptions()
        {
            if (fromMenu == null)
                options.Add(new MenuOption("Exit", () => {
                    Blaze.instance.Exit();
                    return this;
                }));
            else options.Add(new MenuOption("Back", () => fromMenu));
        }
    }

    //main menu
    internal class MainMenu : Menu
    {
        public MainMenu() : base(null) { }

        //generate options
        public override void GenerateOptions()
        {
            options.Add(new MenuOption("Play", () => new LevelSelect(this)));
            options.Add(new MenuOption("Settings", () => new SettingsMenu(this)));
            base.GenerateOptions();
        }
    }

    //menu for level select
    internal class LevelSelect : Menu
    {
        public LevelSelect(Menu fromMenu) : base(fromMenu) { }

        //generate options
        public override void GenerateOptions()
        {
            for (int i = 0; i < Level.levels.Count && i <= Blaze.instance.settings.levelUnlocked; i++) {
                int x = i; //so that C# doen't box i to a reference in the lambda expression
                options.Add(new MenuOption($"Level {i + 1}", () => new Playing(x)));
            }
            base.GenerateOptions();
        }
    }

    //pause menu
    internal class PauseMenu : Menu
    {
        public PauseMenu() : base(null) { }

        public new void Draw(GraphicsDevice gd, SpriteBatch sb)
        {
            base.Draw(gd, sb);
        }

        //generate options
        public override void GenerateOptions()
        {
            options.Add(new MenuOption("Resume", () => Playing.Instance));
            options.Add(new MenuOption("Settings", () => new SettingsMenu(this)));
            options.Add(new MenuOption("Main Menu", () => new MainMenu()));
            base.GenerateOptions();
        }
    }

    //settings menu
    internal class SettingsMenu : Menu
    {
        public SettingsMenu(Menu fromMenu) : base(fromMenu) { }

        //generate options
        public override void GenerateOptions()
        {
            var settings = Blaze.instance.settings;
            base.GenerateOptions();
            options.Add(
                new MenuOption(
                    $"Resolution: {Blaze.instance.settings.screenWidth}x{Blaze.instance.settings.screenHeight}", () => {
                        var size = new Point(settings.screenWidth, settings.screenHeight);
                        var index = Blaze.resolutions.IndexOf(size);
                        index++;
                        if (index == Blaze.resolutions.Count) index = 0;
                        var r = Blaze.resolutions[index];
                        Blaze.instance.settings.screenWidth = r.X;
                        Blaze.instance.settings.screenHeight = r.Y;
                        RegenerateOptions();

                        return this;
                    }));
            options.Add(new MenuOption("Fullscreen: " + (settings.fullscreen ? "On" : "Off"), () => {
                Blaze.instance.settings.fullscreen = !settings.fullscreen;
                RegenerateOptions();
                return this;
            }));
            options.Add(new MenuOption("Difficulty: " + (settings.difficulty), () => {
                Blaze.instance.settings.difficulty = settings.difficulty==Difficulty.Easy ? Difficulty.Medium : settings.difficulty==Difficulty.Medium ? Difficulty.Hard : Difficulty.Easy;
                RegenerateOptions();
                return this;
            }));
            options.Add(new MenuOption("Apply", () => {
                Blaze.instance.ApplySettings();
                return fromMenu;
            }));
        }
    }
}
