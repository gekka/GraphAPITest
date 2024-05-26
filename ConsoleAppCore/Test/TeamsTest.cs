using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azure.Core;
using Azure.Identity; //nuget
using ConsoleApp1;
using Microsoft.Graph; //nuget
using Microsoft.Graph.Models;

namespace ConsoleApp1.Test
{
    internal class TeamsTest
    {
        public static async Task PostMessageAsync(GraphServiceClient graphClient, string messageText = "From Application")
        {
            var chatMessage = new Microsoft.Graph.Models.ChatMessage();
            chatMessage.Body = new Microsoft.Graph.Models.ItemBody() { Content = messageText };

            var channels = await graphClient.Teams[TestSettings.TeamID].AllChannels.GetAsync();

            if (channels?.Value == null || channels.Value.Count() == 0)
            {
                return;
            }
            var channelId = channels.Value[0].Id;

            await graphClient.Teams[TestSettings.TeamID].Channels[channelId].Messages.PostAsync(chatMessage);
        }
    }
}
