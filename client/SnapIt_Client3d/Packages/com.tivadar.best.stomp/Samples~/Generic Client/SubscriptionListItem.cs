using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Best.STOMP.Examples.Helpers
{
    public class SubscriptionListItem : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private Text _text;
#pragma warning restore

        public GenericClient Parent { get; private set; }
        public Subscription SubScription { get; private set; }
        public string Color { get; private set; }

        public void Set(GenericClient parent, Subscription subscription, string color)
        {
            this.Parent = parent;
            this.SubScription = subscription;
            this.Color = color;

            this._text.text = $"<color=#{color}>{this.SubScription.Destination}</color>";
        }

        public void AddLeftPadding(int padding)
        {
            this.GetComponent<LayoutGroup>().padding.left += padding;
        }

        public void OnUnsubscribeButton()
        {
            this.Parent.Unsubscribe(this.SubScription);
        }
    }
}
