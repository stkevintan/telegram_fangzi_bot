using System;
using System.Collections.Generic;

namespace Fangzi.Bot.Services
{
	class ChatStatus
	{
		public bool Running { set; get; }
		public DateTime? FreeAt { set; get; }
	}

	public class RateLimitService
	{
		Dictionary<long, ChatStatus> _chats = new Dictionary<long, ChatStatus>();


		public (bool, string) BeforeCheck(long Id)
		{
			if (!_chats.ContainsKey(Id))
			{
				_chats[Id] = new ChatStatus { Running = true };
				return (true, string.Empty);
			}

			if (_chats[Id].Running)
			{
				return (false, "芳子酱忙碌中...");
			}
			var now = DateTime.Now;
			if (_chats[Id].FreeAt is DateTime freeAt && freeAt > now)
			{
				return (false, $"技能CD中，让芳子酱休息{(int)(freeAt - now).TotalSeconds}秒吧~");
			}
			_chats[Id].Running = true;
			return (true, string.Empty);
		}
		public void AfterDone(long Id, int idleSec = 0)
		{
			if (!_chats.ContainsKey(Id))
			{
				return;
			}
			_chats[Id].FreeAt = DateTime.Now.AddSeconds(idleSec);
			_chats[Id].Running = false;
		}
	}
}