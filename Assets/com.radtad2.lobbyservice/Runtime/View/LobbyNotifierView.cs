using System;
using System.Collections.Generic;

namespace LobbyService.Samples.Steam
{
    /// <summary>
    /// Captures view events and simply forwards them into events so that non-visual clients can read results too.
    /// Mostly intended to be used for testing.
    /// </summary>
    public class LobbyNotifierView : ILobbyCoreView, ILobbyChatView, ILobbyBrowserView, IDisposable
    {
        public readonly AwaitableEvent<EnterLobbyResult> OnCreationResult = new();
        public readonly AwaitableEvent<EnterLobbyResult> OnJoinResult = new();
        public readonly AwaitableEvent<LeaveInfo> OnLeave = new();
        public readonly AwaitableEvent<InviteSentInfo> OnSendInvite = new();
        public readonly AwaitableEvent<LobbyInvite> OnReceivedInvite = new();
        public readonly AwaitableEvent<LobbyMember> OnOwnerUpdate = new();
        public readonly AwaitableEvent<MemberJoinedInfo> OnMemberJoined = new();
        public readonly AwaitableEvent<LeaveInfo> OnMemberLeft = new();
        public readonly AwaitableEvent<LobbyDataUpdate> OnNewLobbyData = new();
        public readonly AwaitableEvent<MemberDataUpdate> OnNewMemberData = new();

        public readonly AwaitableEvent<LobbyChatMessage> OnChatMessageReceived = new();
        public readonly AwaitableEvent<LobbyChatMessage> OnDirectMessageReceived = new();

        public readonly AwaitableEvent<List<LobbyDescriptor>> OnBrowseResult = new();
        public readonly AwaitableEvent<LobbyNumberFilter> OnAddedNumberFilter = new();
        public readonly AwaitableEvent<LobbyStringFilter> OnAddedStringFilter = new();
        public readonly AwaitableEvent<string> OnRemovedStringFilter = new();
        public readonly AwaitableEvent<string> OnRemovedNumberFilter = new();
        public readonly AwaitableEvent<int> OnSetSlotsAvailableFilter = new();
        public readonly AwaitableEvent OnClearSlotsAvailableFilter = new();
        public readonly AwaitableEvent<int> OnSetLimitResponsesFilter = new();
        public readonly AwaitableEvent OnClearLimitResponsesFilter = new();
        public readonly AwaitableEvent OnClearedAllFilters = new();
        public readonly AwaitableEvent<LobbyDistance> OnAddedDistanceFilter = new();
        public readonly AwaitableEvent OnClearedDistanceFilter = new();
        public readonly AwaitableEvent<LobbyKeyAndSorter> OnAddedSorter = new();
        public readonly AwaitableEvent<string> OnRemovedSorter = new();
        public readonly AwaitableEvent OnClearedAllSorters = new();

        private LobbyController _lobby;

        public LobbyNotifierView(LobbyController lobby)
        {
            _lobby = lobby;
            _lobby.ConnectView(this);
        }

        public void Dispose() =>_lobby.DisconnectView(this);

        public void DisplayExistingLobby(IReadonlyLobbyModel snapshot) { }
        public void DisplayCreateRequested(CreateLobbyRequest request) { }
        public void DisplayJoinRequested(JoinLobbyRequest request) { }
        public void DisplayStartedBrowsing() { }
        public void DisplayCreateResult(EnterLobbyResult result) => OnCreationResult.Raise(result);
        public void DisplayJoinResult(EnterLobbyResult result) => OnJoinResult.Raise(result);
        public void DisplayLocalMemberLeft(LeaveInfo info) => OnLeave.Raise(info);
        public void DisplaySendInvite(InviteSentInfo info) => OnSendInvite.Raise(info);
        public void DisplayReceivedInvite(LobbyInvite invite) => OnReceivedInvite.Raise(invite);
        public void DisplayOtherMemberJoined(MemberJoinedInfo info) => OnMemberJoined.Raise(info);
        public void DisplayOtherMemberLeft(LeaveInfo info) => OnMemberLeft.Raise(info);
        public void DisplayUpdateOwner(LobbyMember newOwner) => OnOwnerUpdate.Raise(newOwner);
        public void DisplayUpdateLobbyData(LobbyDataUpdate update) => OnNewLobbyData.Raise(update);
        public void DisplayUpdateMemberData(MemberDataUpdate update) => OnNewMemberData.Raise(update);
        public void DisplayMessage(LobbyChatMessage message) => OnChatMessageReceived.Raise(message);
        public void DisplayDirectMessage(LobbyChatMessage message) => OnDirectMessageReceived.Raise(message);
        public void DisplayBrowsingResult(List<LobbyDescriptor> lobbies) => OnBrowseResult.Raise(lobbies);
        public void DisplayAddedNumberFilter(LobbyNumberFilter filter) => OnAddedNumberFilter.Raise(filter);
        public void DisplayAddedStringFilter(LobbyStringFilter filter) => OnAddedStringFilter.Raise(filter);
        public void DisplayRemovedNumberFilter(string key) => OnRemovedNumberFilter.Raise(key);
        public void DisplayRemovedStringFilter(string key) => OnRemovedStringFilter.Raise(key);
        public void DisplaySetSlotsAvailableFilter(int numAvailable) => OnSetSlotsAvailableFilter.Raise(numAvailable);
        public void DisplayClearedSlotsAvailableFilter() => OnClearSlotsAvailableFilter.Raise();
        public void DisplaySetLimitResponsesFilter(int limit) => OnSetLimitResponsesFilter.Raise(limit);
        public void DisplayClearLimitResponsesFilter() => OnClearLimitResponsesFilter.Raise();
        public void DisplayClearedAllFilters() => OnClearedAllFilters.Raise();
        public void DisplayAddedDistanceFilter(LobbyDistance value) => OnAddedDistanceFilter.Raise(value);
        public void DisplayClearedDistanceFilter() => OnClearedDistanceFilter.Raise();
        public void DisplayAddedSorter(LobbyKeyAndSorter sorter) => OnAddedSorter.Raise(sorter);
        public void DisplayRemovedSorter(string key) => OnRemovedSorter.Raise(key);
        public void DisplayClearedAllSorters() => OnClearedAllSorters.Raise();
    }
}
