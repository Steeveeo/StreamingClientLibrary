﻿using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.Webhook;
using Twitch.Base.Models.NewAPI.Webhooks;

namespace Twitch.Base.Services.NewAPI
{
    /// <summary>
    /// The APIs for Webhook-based services.
    /// </summary>
    public class WebhooksService : NewTwitchAPIServiceBase
    {
        /// <summary>
        /// Creates an instance of the WebhooksService
        /// </summary>
        /// <param name="connection">The Twitch connection to use</param>
        public WebhooksService(TwitchConnection connection) : base(connection) { }

        /// <summary>
        /// Gets the Webhook subscriptions of a user identified by a Bearer token, in order of expiration.  Requires App Token.
        /// </summary>
        /// <param name="maxResults">Maximum results to return. Must be a value from 10 to 100.</param>
        /// <returns>The subscribed webhooks for the user identified by a Bearer token in the Twitch connection</returns>
        public async Task<IEnumerable<WebhookSubscriptionModel>> GetWebhookSubscriptions(int maxResults=100)
        {
            Validator.Validate(maxResults >= 10, "maxResults must be greater than or equal to 10.");
            Validator.Validate(maxResults <= 100, "maxResults must be less than or equal to 100.");
            return await this.GetPagedDataResultAsync<WebhookSubscriptionModel>("webhooks/subscriptions", maxResults);
        }

        /// <summary>
        /// Subscribe to a webhook topic
        /// </summary>
        /// <param name="callback">URL where notifications will be delivered.</param>
        /// <param name="topic">URL for the topic to subscribe to or unsubscribe from. topic maps to a new Twitch API endpoint.</param>
        /// <param name="lease_seconds">Number of seconds until the subscription expires. Default: 0. Maximum: 864000.</param>
        /// <param name="secret">Secret used to sign notification payloads. The X-Hub-Signature header is generated by sha256(secret, notification_bytes). Can be null but not recommended.</param>
        /// <returns>An awaitable task</returns>
        /// <exception cref="HttpRestRequestException">Throw in the event of a failed request</exception>
        public async Task SubscribeWebhookTopic(string callback, string topic, int lease_seconds, string secret)
        {
            Validator.ValidateString(callback, "callback");
            Validator.ValidateString(callback, "topic");
            Validator.Validate(lease_seconds >= 0 && lease_seconds <= 864000, "lease_seconds must be be between 0 and 864000");
            await processWebhookTopicRequest(callback, "subscribe", topic, lease_seconds, secret);
        }

        /// <summary>
        /// Unsubscribe from a webhook topic
        /// </summary>
        /// <param name="callback">URL where notifications will be delivered.</param>
        /// <param name="topic">URL for the topic to subscribe to or unsubscribe from. topic maps to a new Twitch API endpoint.</param>
        /// <param name="secret">Secret used to sign notification payloads. The X-Hub-Signature header is generated by sha256(secret, notification_bytes). Can be null but not recommended.</param>
        /// <returns>An awaitable task</returns>
        /// <exception cref="HttpRestRequestException">Throw in the event of a failed request</exception>
        public async Task UnsubscribeWebhookTopic(string callback, string topic, string secret)
        {
            Validator.ValidateString(callback, "callback");
            Validator.ValidateString(callback, "topic");
            await processWebhookTopicRequest(callback, "unsubscribe", topic, 0, secret);
        }

        /// <summary>
        /// Processes the webhook topic request
        /// </summary>
        /// <param name="callback">URL where notifications will be delivered.</param>
        /// <param name="mode">Whether the request is a subscribing or unsubscribing</param>
        /// <param name="topic">URL for the topic to subscribe to or unsubscribe from. topic maps to a new Twitch API endpoint.</param>
        /// <param name="lease_seconds">Number of seconds until the subscription expires. Default: 0. Maximum: 864000.</param>
        /// <param name="secret">Secret used to sign notification payloads. The X-Hub-Signature header is generated by sha256(secret, notification_bytes).</param>
        /// <returns>An awaitable task</returns>
        /// <exception cref="HttpRestRequestException">Throw in the event of a failed request</exception>
        private async Task processWebhookTopicRequest(string callback, string mode, string topic, int lease_seconds, string secret)
        {
            HttpResponseMessage response = await this.PostAsync("webhooks/hub", AdvancedHttpClient.CreateContentFromObject(new WebhookSubscriptionRegistrationModel(callback, mode, topic, lease_seconds, secret)));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRestRequestException(response);
            }
        }
    }
}
