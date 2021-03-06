﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace GameCloud.Common.Settings
{
    public class SettingsDefaultValueProvider<TSettings> : ISettingsValueProvider
    {
        private static readonly Lazy<SettingsDefaultValueProvider<TSettings>> DefaultProvider = new Lazy<SettingsDefaultValueProvider<TSettings>>(
            () => new SettingsDefaultValueProvider<TSettings>(),
            LazyThreadSafetyMode.PublicationOnly);

        public SettingsDefaultValueProvider()
        {
            this.SettingValues = typeof(TSettings).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetCustomAttribute<DefaultValueAttribute>() != null)
                .AsParallel()
                .Select(
                    p =>
                        new SettingsValuePair
                        {
                            Name = p.Name,
                            Value = p.GetCustomAttribute<DefaultValueAttribute>().Value
                        })
                .ToList();
        }

        public static SettingsDefaultValueProvider<TSettings> Default
        {
            get { return DefaultProvider.Value; }
        }

        public ICollection<SettingsValuePair> SettingValues { get; }
    }
}