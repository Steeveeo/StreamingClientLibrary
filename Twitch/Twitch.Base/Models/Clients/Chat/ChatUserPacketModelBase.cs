﻿using System.Collections.Generic;

namespace Twitch.Base.Models.Clients.Chat
{
    /// <summary>
    /// Base class for user-based chat packets.
    /// </summary>
    public class ChatUserPacketModelBase : ChatPacketModelBase
    {
        /// <summary>
        /// The user's display name.
        /// </summary>
        public string UserDisplayName { get; set; }

        /// <summary>
        /// The user's badge information.
        /// </summary>
        public string UserBadgeInfo { get; set; }

        /// <summary>
        /// The user's badges.
        /// </summary>
        public string UserBadges { get; set; }

        /// <summary>
        /// Hexadecimal RGB color code of the message, if any.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Creates a new instance of the ChatUserStatePacketModel class.
        /// </summary>
        /// <param name="packet">The Chat packet</param>
        public ChatUserPacketModelBase(ChatRawPacketModel packet)
            : base(packet)
        {
            this.UserDisplayName = packet.GetTagString("display-name");
            this.UserBadgeInfo = packet.GetTagString("badge-info");
            this.UserBadges = packet.GetTagString("badges");
            this.Color = packet.GetTagString("color");
        }

        /// <summary>
        /// A dictionary containing the user's badges and associated versions.
        /// </summary>
        public Dictionary<string, int> BadgeDictionary { get { return this.ParseBadgeDictionary(this.UserBadges); } }

        /// <summary>
        /// A dictionary containing the user's badges and associated versions.
        /// </summary>
        public Dictionary<string, int> BadgeInfoDictionary { get { return this.ParseBadgeDictionary(this.UserBadgeInfo); } }

        private Dictionary<string, int> ParseBadgeDictionary(string list)
        {
            Dictionary<string, int> results = new Dictionary<string, int>();
            if (!string.IsNullOrEmpty(list))
            {
                string[] splits = list.Split(new char[] { ',', '/' });
                if (splits != null && splits.Length > 0 && splits.Length % 2 == 0)
                {
                    for (int i = 0; i < splits.Length; i = i + 2)
                    {
                        if (int.TryParse(splits[i + 1], out int version))
                        {
                            results[splits[i]] = version;
                        }
                    }
                }
            }
            return results;
        }
    }
}
