namespace Fangzi.Bot.Attributes
{
	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class RateLimitedAttribute : System.Attribute
	{

		public int? Seconds;

		public RateLimitedAttribute()
		{
		}
		public RateLimitedAttribute(int seconds)
		{
			if (seconds >= 0)
			{
				Seconds = seconds;
			}
			else
			{
				Seconds = 0;
			}
		}
	}
}