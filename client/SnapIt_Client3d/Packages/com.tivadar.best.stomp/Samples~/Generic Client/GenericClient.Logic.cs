using Best.HTTP.Shared.PlatformSupport.Memory;
using Best.STOMP.Builders;
using Best.STOMP.Examples.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Best.STOMP.Examples
{
    public partial class GenericClient
    {
        private Client client;

        // UI instances of SubscriptionListItem
        private List<SubscriptionListItem> subscriptionListItems = new List<SubscriptionListItem>();

        public void OnConnectButton()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (this.transportDropdown.value == 0)
            {
                AddText("<color=red>TCP transport isn't available under WebGL!</color>");
                return;
            }
#endif

            SetConnectingUI();

            var host = this.hostInput.GetValue("localhost");
            var parameters = new ConnectParametersBuilder()
                .WithHost(host, this.portInput.GetIntValue(61613))
                .WithVirtualHost(this.virtualHostInput.GetValue("/"))
                .WithTransport((SupportedTransports)this.transportDropdown.value)
                .WithTLS(this.isSecureToggle.GetBoolValue())
                .WithPath(this.pathInput.GetValue("/ws"))
                .WithHeartBeat(preferredOutgoing: TimeSpan.FromSeconds(this.heartBeatPreferred_Out_Input.GetIntValue(10)),
                               preferedIncoming: TimeSpan.FromSeconds(this.heartBeatPreferred_In_Input.GetIntValue(10)),
                               timeout: TimeSpan.FromSeconds(this.heartBeatPreferred_In_Timeout_Input.GetIntValue(2)))
                .Build();

            this.client = new Client();

            this.client.OnConnected += OnConnected;
            this.client.OnDisconnected += OnDisconnected;
            this.client.OnStateChanged += OnStateChanged;

            this.client.BeginConnect(parameters);
        }

        private void OnConnected(Client client, ServerParameters serverParameters, IncomingFrame frame)
        {
            SetConnectedUI();

            AddText($"[{client.Parameters.Host}] Connected to '{serverParameters.Server}', received id '{serverParameters.Id}'");
        }

        private void OnDisconnected(Client client, Error error)
        {
            if (error != null)
                AddText($"[{client.Parameters.Host}] Disconnected with error: {error}");
            else
                AddText($"[{client.Parameters.Host}] Disconnected!");

            SetDisconnectedUI();
        }

        public void OnDisconnectButton()
        {
            this.connectButton.interactable = false;
            this.client?.BeginDisconnect();
        }

        private void OnStateChanged(Client client, States oldState, States newState)
        {
            AddText($"[{client.Parameters.Host}] <color=yellow>{oldState}</color> => <color=green>{newState}</color>");
        }

        public void OnSendButtonClicked()
        {
            string destination = this.send_DestinationInput.GetValue("/queue/test");
            string message = this.send_MessageInput.GetValue("");

            this.client.CreateMessageBuilder(destination)
                .WithContent(message)
                .WithAcknowledgmentCallback(OnMessageSendAcknowledged)
                .BeginSend();

            AddText($"[{client.Parameters.Host}] sending message...");
        }

        private void OnMessageSendAcknowledged(Client client, IncomingFrame frame)
        {
            AddText($"[{client.Parameters.Host}] Message sent!");
        }

        public void OnSubscribeButtonClicked()
        {
            var colorValue = this.subscribe_ColorInput.GetValue("FF0000");
            if (!ColorUtility.TryParseHtmlString("#" + colorValue, out var color))
            {
                AddText($"[{client.Parameters.Host}] <color=red>Couldn't parse '#{colorValue}'</color>");
                return;
            }

            var ackMode = this.subscribe_ACKModeDropdown.GetACKMode();
            var destination = this.subscribe_DestinationInput.GetValue("/queue/test");

            this.client.CreateSubscriptionBuilder(destination)
                .WithAcknowledgmentMode(ackMode)
                .WithAcknowledgmentCallback(OnSubscriptionAcknowledgement)
                .WithCallback(OnApplicationMessage)
                .BeginSubscribe();

            AddText($"[{client.Parameters.Host}] Subscribe request for destination <color=#{colorValue}>{destination}</color> sent...");
        }

        private void OnSubscriptionAcknowledgement(Client client, Subscription subscription, IncomingFrame frame)
        {
            var item = Instantiate<SubscriptionListItem>(this.subscription_ListItem, this.subscribe_ListItemRoot);
            item.Set(this, subscription, "FF0000");

            this.subscriptionListItems.Add(item);

            AddText($"[{client.Parameters.Host}] Subscription request to topic <color=#{item.Color}>{subscription.Destination}</color> finished!");
        }

        public void Unsubscribe(Subscription subscription)
        {
            subscription.BeginUnsubscribe(OnUnsubscribed);

            var instance = FindSubscriptionItem(subscription.Destination);
            AddText($"[{client.Parameters.Host}] Unsubscribe request for topic <color=#{instance.Color}>{subscription.Destination}</color> sent...");
        }

        private void OnUnsubscribed(Client client, Subscription subscription)
        {
            var instance = FindSubscriptionItem(subscription.Destination);
            this.subscriptionListItems.Remove(instance);
            Destroy(instance.gameObject);

            AddText($"[{client.Parameters.Host}] Unsubscribe request to destination <color=#{instance.Color}>{subscription.Destination}</color> finished!");
        }

        private void OnApplicationMessage(Client client, Subscription subscription, Message message)
        {
            string payload = string.Empty;

            // Here we going to try to convert the payload as an UTF-8 string. Note that it's not guaranteed that the payload is a string!
            // While STOMP supports an additional Content-Type field, in this demo we can't rely on its presense.
            if (message.ContentType != null && message.ContentType.StartsWith("text/") && message.Content != BufferSegment.Empty)
            {
                payload = Encoding.UTF8.GetString(message.Content.Data, message.Content.Offset, message.Content.Count);

                const int MaxPayloadLength = 512;
                if (payload.Length > MaxPayloadLength)
                    payload = payload?.Remove(MaxPayloadLength);
            }

            var ui = FindSubscriptionItem(subscription.Destination);
            AddText($"[{client.Parameters.Host}] <color=#{ui.Color}>[{subscription.Destination}] ({message.ContentType}): \"{payload}\"</color>");
        }

        private SubscriptionListItem FindSubscriptionItem(string destination) => this.subscriptionListItems.FirstOrDefault(s => s.SubScription.Destination.Equals(destination));

        private void OnDestroy()
            => this.client?.BeginDisconnect();
    }
}
