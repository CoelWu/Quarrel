﻿// Copyright (c) Quarrel. All rights reserved.

using DiscordAPI.Models;
using DiscordAPI.Voice;
using DiscordAPI.Voice.DownstreamEvents;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Quarrel.ViewModels.Messages.Gateway;
using Quarrel.ViewModels.Messages.Voice;
using Quarrel.ViewModels.Models.Bindables.Channels;
using Quarrel.ViewModels.Services.Discord.Channels;
using Quarrel.ViewModels.Services.Discord.Rest;
using Quarrel.ViewModels.Services.DispatcherHelper;
using Quarrel.ViewModels.Services.Gateway;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Quarrel.ViewModels.Services.Voice
{
    /// <summary>
    /// Manages all voice state data.
    /// </summary>
    public sealed class VoiceService : IVoiceService
    {
        private readonly IChannelsService _channelsService;
        private readonly IDiscordService _discordService;
        private readonly IDispatcherHelper _dispatcherHelper;
        private readonly IGatewayService _gatewayService;
        private readonly IWebrtcManager _webrtcManager;
        private VoiceConnection _voiceConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoiceService"/> class.
        /// </summary>
        /// <param name="channelsService">The app's channel service.</param>
        /// <param name="discordService">The app's discord service.</param>
        /// <param name="dispatcherHelper">The app's dispatcher helper.</param>
        /// <param name="gatewayService">The app's gateway service.</param>
        /// <param name="webrtcManager">The app's webrtc manager.</param>
        public VoiceService(
            IChannelsService channelsService,
            IDiscordService discordService,
            IDispatcherHelper dispatcherHelper,
            IGatewayService gatewayService,
            IWebrtcManager webrtcManager)
        {
            _channelsService = channelsService;
            _discordService = discordService;
            _dispatcherHelper = dispatcherHelper;
            _gatewayService = gatewayService;
            _webrtcManager = webrtcManager;

            Messenger.Default.Register<GatewayVoiceStateUpdateMessage>(this, m =>
            {
                _dispatcherHelper.CheckBeginInvokeOnUi(() =>
                {
                    if (VoiceStates.ContainsKey(m.VoiceState.UserId))
                    {
                        var oldChannel = _channelsService.GetChannel(VoiceStates[m.VoiceState.UserId].Model.ChannelId);
                        oldChannel?.ConnectedUsers.Remove(m.VoiceState.UserId);

                        if (m.VoiceState.ChannelId == null)
                        {
                            if (m.VoiceState.UserId == _discordService.CurrentUser.Id)
                            {
                                DisconnectFromVoiceChannel();
                                CurrentVoiceChanel = null;
                                Messenger.Default.Send(new VoiceChannelConnectedMessage(CurrentVoiceChanel));
                            }
                            else
                            {
                                VoiceStates.Remove(m.VoiceState.UserId);
                            }
                        }
                        else
                        {
                            VoiceStates[m.VoiceState.UserId].Model = m.VoiceState;
                            VoiceStates[m.VoiceState.UserId].UpateProperties();

                            if (m.VoiceState.UserId == _discordService.CurrentUser.Id)
                            {
                                if (CurrentVoiceChanel == null ||
                                    CurrentVoiceChanel.Model.Id == m.VoiceState.ChannelId)
                                {
                                    CurrentVoiceChanel = _channelsService.GetChannel(m.VoiceState.ChannelId);
                                    Messenger.Default.Send(new VoiceChannelConnectedMessage(CurrentVoiceChanel));
                                }
                            }
                        }
                    }
                    else
                    {
                        BindableVoiceUser voiceUser = new BindableVoiceUser(m.VoiceState);
                        VoiceStates.Add(m.VoiceState.UserId, voiceUser);
                    }

                    if (m.VoiceState.ChannelId != null)
                    {
                        var channel = SimpleIoc.Default.GetInstance<IChannelsService>().GetChannel(m.VoiceState.ChannelId);
                        channel.ConnectedUsers.Add(m.VoiceState.UserId, VoiceStates[m.VoiceState.UserId]);
                    }
                });
            });

            Messenger.Default.Register<SpeakMessage>(this, e =>
            {
                if (e.EventData.UserId != null && VoiceStates.ContainsKey(e.EventData.UserId))
                {
                    _dispatcherHelper.CheckBeginInvokeOnUi(() => { VoiceStates[e.EventData.UserId].Speaking = e.EventData.Speaking > 0; });
                }
            });

            Messenger.Default.Register<GatewayVoiceServerUpdateMessage>(this, m =>
            {
                if (!VoiceStates.ContainsKey(_discordService.CurrentUser.Id))
                {
                    VoiceStates.Add(
                        _discordService.CurrentUser.Id,
                        new BindableVoiceUser(
                            new VoiceState()
                            {
                                ChannelId = m.VoiceServer.ChannelId,
                                GuildId = m.VoiceServer.GuildId,
                                UserId = _discordService.CurrentUser.Id,
                            }));
                }

                ConnectToVoiceChannel(m.VoiceServer, VoiceStates[_discordService.CurrentUser.Id].Model);
            });
        }

        /// <inheritdoc/>
        public IDictionary<string, BindableVoiceUser> VoiceStates { get; } = new ConcurrentDictionary<string, BindableVoiceUser>();

        /// <inheritdoc/>
        public BindableChannel CurrentVoiceChanel { get; private set; }

        /// <inheritdoc/>
        public async void ToggleDeafen()
        {
            var state = _voiceConnection._state;
            state.SelfDeaf = !state.SelfDeaf;
            await _gatewayService.Gateway.VoiceStatusUpdate(state.GuildId, state.ChannelId, state.SelfMute, state.SelfDeaf);
        }

        /// <inheritdoc/>
        public async void ToggleMute()
        {
            var state = _voiceConnection._state;
            state.SelfMute = !state.SelfMute;
            await _gatewayService.Gateway.VoiceStatusUpdate(state.GuildId, state.ChannelId, state.SelfMute, state.SelfDeaf);
        }

        private async void ConnectToVoiceChannel(VoiceServerUpdate data, VoiceState state)
        {
            _voiceConnection = new VoiceConnection(data, state, _webrtcManager);
            _voiceConnection.Speak += Speak;
            await _voiceConnection.ConnectAsync();
        }

        private void DisconnectFromVoiceChannel()
        {
            _webrtcManager.Destroy();
        }

        private void Speak(object sender, VoiceConnectionEventArgs<Speak> e)
        {
            Messenger.Default.Send(new SpeakMessage(e.EventData));
        }

        private void SpeakingChanged(object sender, int e)
        {
            _voiceConnection.SendSpeaking(e);
        }
    }
}