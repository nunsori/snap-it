using System;

//using Best.STOMP.Builders;

using UnityEngine.UI;

namespace Best.STOMP.Examples
{
    public static class UIExtensions
    {
        public static string GetValue(this InputField field)
        {
            var value = field.text;
            return value;
        }

        public static string GetValue(this InputField field, string defaultValue)
        {
            var value = field.text;

            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        public static int GetIntValue(this InputField field, int defaultValue)
        {
            return int.TryParse(field.text, out var value) ? value : defaultValue;
        }

        public static bool GetBoolValue(this Toggle toggle)
        {
            return toggle.isOn;
        }

        public static AcknowledgmentModes GetACKMode(this Dropdown dropdown)
        {
            return ((AcknowledgmentModes[])Enum.GetValues(typeof(AcknowledgmentModes)))[dropdown.value];
        }
    }
}
