﻿using DiscordAPI.API.Guild.Models;
using GalaSoft.MvvmLight.Ioc;
using Quarrel.Models.Bindables;
using Quarrel.Navigation;
using Quarrel.Services.Rest;
using Quarrel.SubPages.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Quarrel.ViewModels;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Quarrel.SubPages
{
    public sealed partial class AddChannelPage : UserControl, IConstrainedSubPage
    {

        public AddChannelPage()
        {
            this.InitializeComponent();
        }


        public double MaxExpandedHeight { get; } = 300;

        public double MaxExpandedWidth { get; } = 400;

    }
}
