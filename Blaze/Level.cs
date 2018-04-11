using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNA3D
{
    //class containing data for levels
    class Level
    {

        public List<Box> terrain;
        public List<Light> lights;
        public List<TextBox> text;
        public Vector3 spawn;
        public Exit exit;

        public Level()
        {
            terrain = new List<Box>();
            lights = new List<Light>();
            text = new List<TextBox>();
            spawn = new Vector3();
        }

        public static List<Level> levels;

        //generate levels programmatically
        static Level()
        {
            Program.log.Log("Loading levels");
            levels = new List<Level>();
            for (int i = 1; File.Exists(@"Content/Levels/" + i + @".dat"); i++)
            {
                Program.log.Log($"Loading level {i}");
                levels.Add(LoadLevel(File.ReadAllText(@"Content/Levels/" + i + ".dat")));
            }
            Program.log.Log("Finished loading levels");
        }

        //helper to shorten float.Parse
        static float num(string n) => float.Parse(n);

        //load a level from a block of text
        static Level LoadLevel(string text)
        {
            Level ans = new Level();
            var sp = text.Replace("\r\n", " ").Replace('\n', ' ').Replace("  ", " ").Split(' ');
            int ind = 0;
            while (ind < sp.Length)
            {
                if (sp[ind] == "Lights") break;
                ans.terrain.Add(new TexturedBox(num(sp[ind++]), num(sp[ind++]), num(sp[ind++]), num(sp[ind++]), num(sp[ind++]), num(sp[ind++]), Blaze.platform));
            }
            ind++;
            while (ind < sp.Length)
            {
                if (sp[ind] == "Text") break;
                ans.lights.Add(new Light(num(sp[ind++]), num(sp[ind++]), num(sp[ind++]), sp[ind++]=="true"));
            }
            ind++;
            while (ind < sp.Length)
            {
                if (sp[ind] == "Exit") break;
                ans.text.Add(new TextBox(num(sp[ind++]), num(sp[ind++]), num(sp[ind++]), sp[ind++].Replace("\\s", " "), int.Parse(sp[ind++])));
            }
            ind++;
            ans.exit = new Exit(num(sp[ind++]), num(sp[ind++]), num(sp[ind++]));
            ind++;
            ans.spawn = new Vector3(num(sp[ind++]), num(sp[ind++]), num(sp[ind++]));
            return ans;
        }

    }
}
