using Best.HTTP.Shared;
using Best.STOMP.Examples.Helpers;

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Best.STOMP.Examples
{
    public partial class GenericClient : MonoBehaviour
    {
        class Template
        {
            public string name;

            public string host;
            public int port;
            public SupportedTransports transport;
            public string path = "/ws";
            public bool isSecure;

            public string username = string.Empty;
            public string password = string.Empty;

            public TimeSpan heartBeatPreferredOutgoing = TimeSpan.FromSeconds(10);
            public TimeSpan heartBeatPreferredIncoming = TimeSpan.FromSeconds(10);
            public TimeSpan heartBeatTimeout = TimeSpan.FromSeconds(2);

            public Template()
            {
            }
        }

        private Template[] templates = new Template[]
        {
            // local RabbitMQ
#if !UNITY_WEBGL || UNITY_EDITOR
            new Template { name = "RabbitMQ - TCP - Unsecure - Unauthenticated", host = "localhost", port = 61613, transport = SupportedTransports.TCP, isSecure = false },
#endif
            new Template { name = "RabbitMQ - WebSocket - Unsecure - Unauthenticated", host = "localhost", port = 15674, transport = SupportedTransports.WebSocket, isSecure = false },
        };

#pragma warning disable 0649
        [Header("Connect")]
        [SerializeField]
        private Dropdown templatesDropdown;

        [SerializeField]
        private InputField hostInput;

        [SerializeField]
        private InputField virtualHostInput;

        [SerializeField]
        private InputField portInput;

        [SerializeField]
        private Dropdown transportDropdown;

        [SerializeField]
        private InputField pathInput;

        [SerializeField]
        private Toggle isSecureToggle;

        [SerializeField]
        private InputField userNameInput;

        [SerializeField]
        private InputField passwordInput;

        [SerializeField]
        private InputField heartBeatPreferred_Out_Input;

        [SerializeField]
        private InputField heartBeatPreferred_In_Input;

        [SerializeField]
        private InputField heartBeatPreferred_In_Timeout_Input;

        [SerializeField]
        private Button connectButton;


        [Header("Send")]
        [SerializeField]
        private InputField send_DestinationInput;

        [SerializeField]
        private InputField send_MessageInput;

        [Header("Subscribe")]
        [SerializeField]
        private InputField subscribe_ColorInput;

        [SerializeField]
        private Dropdown subscribe_ACKModeDropdown;

        [SerializeField]
        private InputField subscribe_DestinationInput;

        [SerializeField]
        private Transform subscribe_ListItemRoot;

        [SerializeField]
        private SubscriptionListItem subscription_ListItem;

        [SerializeField]
        private Transform sendAndSubscribePanel;

        [Header("Logs")]
        [SerializeField]
        private InputField logs_MaxEntriesInput;

        [SerializeField]
        private Toggle logs_AutoScroll;

        [SerializeField]
        private TextListItem textListItem;

        [SerializeField]
        private ScrollRect log_view;

        [SerializeField]
        private Transform logRoot;

#pragma warning restore

        private void Awake()
        {
            InitUI();
            PopulateTemplates();
        }

        private void AddText(string text)
        {
            int maxEntries = this.logs_MaxEntriesInput.GetIntValue(100);

            if (this.logRoot.childCount >= maxEntries)
            {
                TrimLogEntries(maxEntries);

                var child = this.logRoot.GetChild(0);
                child.GetComponent<TextListItem>().SetText(text);
                child.SetAsLastSibling();
            }
            else
            {
                var item = Instantiate<TextListItem>(this.textListItem, this.logRoot);
                item.SetText(text);
            }

            bool autoScroll = this.logs_AutoScroll.GetBoolValue();
            if (autoScroll)
            {
                this.log_view.normalizedPosition = new Vector2(0, 0);
            }
        }

        private void TrimLogEntries(int maxEntries)
        {
            while (this.logRoot.childCount > maxEntries)
            {
                var child = this.logRoot.GetChild(0);
                child.transform.SetParent(this.transform);

                Destroy(child.gameObject);
            }
        }

        private void InitUI()
        {
            this.connectButton.GetComponentInChildren<Text>().text = "Begin Connect";
            this.connectButton.interactable = true;
            this.connectButton.onClick.RemoveAllListeners();
            this.connectButton.onClick.AddListener(OnConnectButton);

            foreach (var button in this.sendAndSubscribePanel.GetComponentsInChildren<Button>())
                button.interactable = false;
        }

        private void PopulateTemplates()
        {
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>(templates.Length);
            for (int i = 0; i < templates.Length; i++)
            {
                var template = templates[i];

                options.Add(new Dropdown.OptionData(template.name));
            }

            this.templatesDropdown.AddOptions(options);
            this.templatesDropdown.onValueChanged.AddListener(OnTemplateSelected);
            OnTemplateSelected(0);
        }

        private void OnTemplateSelected(int idx)
        {
            var template = this.templates[idx];

            this.hostInput.text = template.host;
            this.portInput.text = template.port.ToString();
            this.transportDropdown.value = (int)template.transport;
            this.pathInput.text = template.path;
            this.isSecureToggle.isOn = template.isSecure;

            this.userNameInput.text = template.username;
            this.passwordInput.text = template.password;

            this.heartBeatPreferred_Out_Input.text = ((int)template.heartBeatPreferredOutgoing.TotalSeconds).ToString();
            this.heartBeatPreferred_In_Input.text = ((int)template.heartBeatPreferredIncoming.TotalSeconds).ToString();
            this.heartBeatPreferred_In_Timeout_Input.text = ((int)template.heartBeatTimeout.TotalSeconds).ToString();
        }

        private void SetConnectingUI()
        {
            this.connectButton.interactable = false;

            foreach (var button in this.sendAndSubscribePanel.GetComponentsInChildren<Button>())
                button.interactable = false;
        }

        private void SetDisconnectedUI()
        {
            InitUI();
            for (int i = 0; i < this.subscriptionListItems.Count; ++i)
                Destroy(this.subscriptionListItems[i].gameObject);
            this.subscriptionListItems.Clear();
        }

        private void SetConnectedUI()
        {
            this.connectButton.GetComponentInChildren<Text>().text = "Begin Disconnect";
            this.connectButton.interactable = true;
            this.connectButton.onClick.RemoveAllListeners();
            this.connectButton.onClick.AddListener(OnDisconnectButton);

            foreach (var button in this.sendAndSubscribePanel.GetComponentsInChildren<Button>())
                button.interactable = true;
        }

        public void ClearLogEntries()
        {
            TrimLogEntries(0);
        }

        public void OnLogLevelChanged(int idx)
        {
            switch (idx)
            {
                case 0: HTTPManager.Logger.Level = Best.HTTP.Shared.Logger.Loglevels.All; break;
                case 1: HTTPManager.Logger.Level = Best.HTTP.Shared.Logger.Loglevels.Warning; break;
                case 2: HTTPManager.Logger.Level = Best.HTTP.Shared.Logger.Loglevels.None; break;
            }
        }
    }
}
