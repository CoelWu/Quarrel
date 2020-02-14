﻿using DiscordAPI.API.Guild.Models;
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace Quarrel.Converters.AuditLog
{
    public class ChangeToMarkdownConverter : IValueConverter
    {
        #region Method

        public string GetFormat(Change change)
        {
            string append = (change.OldValue != null ? "Change" : "Set");
            string format = ResourceLoader.GetForCurrentView("AuditLog").GetString(change.Key + append);

            if (string.IsNullOrEmpty(format))
                format = ResourceLoader.GetForCurrentView("AuditLog").GetString("Generic" + append);

            return format;
        }

        #endregion

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Change change)
            {
                string format = GetFormat(change);

                switch (change.Key)
                {
                    case "name":
                    case "code":
                        if (change.NewValue != null)
                            format = format.Replace("<new>", string.Format("**{0}**", change.NewValue));
                        if (change.OldValue != null)
                            format = format.Replace("<old>", change.OldValue.ToString());
                        return format;

                    case "nsfw":
                        if (change.NewValue != null)
                            format = format.Replace("<new>", (bool)change.NewValue ? "**NSFW**" : "**SFW**");
                        return format;

                    case "color":
                        if (change.NewValue != null)
                            format = format.Replace("<new>", string.Format("<@$QUARREL-color{0}>", change.NewValue));
                        if (change.OldValue != null)
                            format = format.Replace("<old>", string.Format("<@$QUARREL-color{0}>", change.OldValue));
                        return format;

                    case "channel_id":
                        if (change.NewValue != null)
                            format = format.Replace("<new>", string.Format("<#{0}>", change.NewValue));
                        return format;

                    case "max_age":
                        if (change.NewValue != null)
                        {
                            if ((long)change.NewValue == 0)
                                format = ResourceLoader.GetForCurrentView("AuditLog").GetString("max_age0");
                            else
                                format = format.Replace("<new>", string.Format("**{0}**", change.NewValue));
                        }
                        return format;

                    case "uses":
                        if (change.NewValue != null)
                        {
                            if ((long)change.NewValue == 0)
                                format = ResourceLoader.GetForCurrentView("AuditLog").GetString("uses0");
                            else if ((long)change.NewValue == 1)
                                format = ResourceLoader.GetForCurrentView("AuditLog").GetString("uses1");
                            else
                                format = format.Replace("<new>", string.Format("**{0}**", change.NewValue));
                        }
                        return format;

                    case "max_uses":
                        if (change.NewValue != null)
                        {
                            if ((long)change.NewValue == 0)
                                format = ResourceLoader.GetForCurrentView("AuditLog").GetString("max_uses0");
                            else if ((long)change.NewValue == 1)
                                format = ResourceLoader.GetForCurrentView("AuditLog").GetString("max_uses1");
                            else
                                format = format.Replace("<new>", string.Format("**{0}**", change.NewValue));
                        }
                        return format;

                    default:
                        format = format.Replace("<property>", change.Key);
                        if (change.NewValue != null)
                            format = format.Replace("<new>", string.Format("**{0}**", change.NewValue));
                        if (change.OldValue != null)
                            format = format.Replace("<old>", change.OldValue.ToString());
                        return format;
                }
            }
            return "Unknown change";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
