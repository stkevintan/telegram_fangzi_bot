namespace Fangzi.Bot.Attributes
{
	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class CommandAttribute : System.Attribute
	{
		public string Name;
		public string Description = string.Empty;

		public bool Hidden = false;

		public CommandAttribute(string name)
		{
			Name = name;
		}
	}
}