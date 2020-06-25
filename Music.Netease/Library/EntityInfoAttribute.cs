using System;
namespace Music.Netease.Library
{

    [AttributeUsage(AttributeTargets.Class)]
    public class EntityInfoAttribute : Attribute
    {
        public int Type { get; private set; }

        public string Name { get; set; }
        public EntityInfoAttribute(int type)
        {
            Type = type;
        }
    }
}