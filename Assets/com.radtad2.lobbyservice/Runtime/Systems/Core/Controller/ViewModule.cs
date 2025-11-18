using System;
using System.Collections.Generic;
using UnityEngine;

namespace LobbyService
{
    public class ViewModule : ICoreView, IChatView, IBrowserView, IFriendView
    {
        private readonly LobbyController _controller;
        private readonly List<IView> _views = new();

        public ViewModule(LobbyController controller)
        {
            _controller = controller;
        }
        
        /// <summary>
        /// Connects a view to the lobby.
        /// </summary>
        public void Connect(IView view)
        {
            if (view == null) return;
            
            _views.Add(view);
            view.Reset(_controller.Capabilities);
            
            if (_controller.Model.InLobby)
            {
                if (view is ICoreView core) core.DisplayExistingLobby(_controller.Model);
            }
        }

        /// <summary>
        /// Disconnects a view from the lobby.
        /// </summary>
        /// <param name="view"></param>
        public void Disconnect(IView view)
        {
            if (view == null) return;
            _views.Remove(view);
        }

        private void Display<T>(Action<T> call) where T : IView
        {
            foreach (var v in _views)
            {
                if (v is T typed) call(typed);
            }
        }
        
        public void Reset(ILobbyCapabilities capabilities)
        {
            Display<IView>(v => v.Reset(capabilities));
        }

        public void DisplayExistingLobby(IReadonlyLobbyModel snapshot)
        {
            Display<ICoreView>(v => v.DisplayExistingLobby(snapshot));
        }

        public void DisplayCreateRequested(CreateLobbyRequest request)
        {
            Display<ICoreView>(v => v.DisplayCreateRequested(request));
        }

        public void DisplayCreateResult(EnterLobbyResult result)
        {
            Display<ICoreView>(v => v.DisplayCreateResult(result));
        }

        public void DisplayJoinRequested(JoinLobbyRequest request)
        {
            Display<ICoreView>(v => v.DisplayJoinRequested(request));
        }

        public void DisplayJoinResult(EnterLobbyResult result)
        {
            Display<ICoreView>(v => v.DisplayJoinResult(result));
        }

        public void DisplayLocalMemberLeft(LeaveInfo info)
        {
            Display<ICoreView>(v => v.DisplayLocalMemberLeft(info));
        }

        public void DisplaySentInvite(InviteSentInfo info)
        {
            Display<ICoreView>(v => v.DisplaySentInvite(info));
        }

        public void DisplayReceivedInvite(LobbyInvite invite)
        {
            Display<ICoreView>(v => v.DisplayReceivedInvite(invite));
        }

        public void DisplayOtherMemberJoined(MemberJoinedInfo info)
        {
            Display<ICoreView>(v => v.DisplayOtherMemberJoined(info));
        }

        public void DisplayOtherMemberLeft(LeaveInfo info)
        {
            Display<ICoreView>(v => v.DisplayOtherMemberLeft(info));
        }

        public void DisplayUpdateOwner(LobbyMember newOwner)
        {
            Display<ICoreView>(v => v.DisplayUpdateOwner(newOwner));
        }

        public void DisplayUpdateLobbyData(LobbyDataUpdate update)
        {
            Display<ICoreView>(v => v.DisplayUpdateLobbyData(update));
        }

        public void DisplayUpdateMemberData(MemberDataUpdate update)
        {
            Display<ICoreView>(v => v.DisplayUpdateMemberData(update));
        }

        public void DisplayMessage(LobbyChatMessage message)
        {
            Display<IChatView>(v => v.DisplayMessage(message));
        }

        public void DisplayDirectMessage(LobbyChatMessage message)
        {
            Display<IChatView>(v => v.DisplayDirectMessage(message));
        }

        public void DisplayStartedBrowsing()
        {
            Display<IBrowserView>(v => v.DisplayStartedBrowsing());
        }

        public void DisplayBrowsingResult(List<LobbyDescriptor> lobbies)
        {
            Display<IBrowserView>(v => v.DisplayBrowsingResult(lobbies));
        }

        public void DisplayAddedNumberFilter(LobbyNumberFilter filter)
        {
            Display<IBrowserView>(v => v.DisplayAddedNumberFilter(filter));
        }

        public void DisplayAddedStringFilter(LobbyStringFilter filter)
        {
            Display<IBrowserView>(v => v.DisplayAddedStringFilter(filter));
        }

        public void DisplayRemovedNumberFilter(string key)
        {
            Display<IBrowserView>(v => v.DisplayRemovedNumberFilter(key));
        }

        public void DisplayRemovedStringFilter(string key)
        {
            Display<IBrowserView>(v => v.DisplayRemovedStringFilter(key));
        }

        public void DisplaySetSlotsAvailableFilter(int numAvailable)
        {
            Display<IBrowserView>(v => v.DisplaySetSlotsAvailableFilter(numAvailable));
        }

        public void DisplayClearedSlotsAvailableFilter()
        {
            Display<IBrowserView>(v => v.DisplayClearedSlotsAvailableFilter());
        }

        public void DisplaySetLimitResponsesFilter(int limit)
        {
            Display<IBrowserView>(v => v.DisplaySetLimitResponsesFilter(limit));
        }

        public void DisplayClearLimitResponsesFilter()
        {
            Display<IBrowserView>(v => v.DisplayClearLimitResponsesFilter());
        }

        public void DisplayAddedDistanceFilter(LobbyDistance filter)
        {
            Display<IBrowserView>(v => v.DisplayAddedDistanceFilter(filter));
        }

        public void DisplayClearedDistanceFilter()
        {
            Display<IBrowserView>(v => v.DisplayClearedDistanceFilter());
        }

        public void DisplayClearedAllFilters()
        {
            Display<IBrowserView>(v => v.DisplayClearedAllFilters());
        }

        public void DisplayAddedSorter(ILobbySorter sorter, string key)
        {
            Display<IBrowserView>(v => v.DisplayAddedSorter(sorter, key));
        }

        public void DisplayRemovedSorter(string key)
        {
            Display<IBrowserView>(v => v.DisplayRemovedSorter(key));
        }

        public void DisplayClearedAllSorters()
        {
            Display<IBrowserView>(v => v.DisplayClearedAllSorters());
        }

        public void DisplayUpdatedFriendList(IReadOnlyList<LobbyMember> friends)
        {
            Display<IFriendView>(v => v.DisplayUpdatedFriendList(friends));
        }

        public void DisplayFriendAvatar(LobbyMember member, Texture2D avatar)
        {
            Display<IFriendView>(v => v.DisplayFriendAvatar(member, avatar));
        }
    }
}