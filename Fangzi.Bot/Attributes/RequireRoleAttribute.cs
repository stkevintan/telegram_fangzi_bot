namespace Fangzi.Bot.Attributes
{
	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class RequireRoleAttribute : System.Attribute
	{
		public string Role = "Admin";

		public RequireRoleAttribute(string role = "Admin")
		{
			Role = role;
		}
	}
}