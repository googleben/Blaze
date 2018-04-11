using System.Xml.Serialization;

namespace XNA3D
{
    //container class for settings and save data
    public class Settings
    {
        [XmlAttribute()] public int screenWidth = 1280;

        [XmlAttribute()] public int screenHeight = 720;

        [XmlAttribute()] public bool fullscreen = false;

        [XmlAttribute()] public Difficulty difficulty = Difficulty.Medium;

        [XmlAttribute()] public int levelUnlocked = 0;
    }
}
