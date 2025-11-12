using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LobbyService.Samples.Steam
{
    public class SampleCoreView : MonoBehaviour, ILobbyCoreView, ILobbyFriendView
    {
        public TMP_Text lobbyNameText;
        
        private LobbyController _controller;
        
        public void SetController(LobbyController controller) => _controller = controller;
        
        public void Create()
        {
            _controller?.Create(new CreateLobbyRequest
            {
                Capacity = 4,
                LobbyType =  LobbyType.Public,
                Name = "My lobby"
            });
        }

        public void Leave()
        {
            _controller?.Leave();
        }
        
        public void DisplayExistingLobby(IReadonlyLobbyModel snapshot)
        {
            Debug.Log("Displaying existing lobby");
        }

        public void DisplayCreateRequested(CreateLobbyRequest request)
        {
            Debug.Log("Creating lobby...");
        }

        public void DisplayCreateResult(EnterLobbyResult result)
        {
            Debug.Log("Created lobby: " + result.Success);
            lobbyNameText.text = $"{result.LocalMember.DisplayName}'s lobby";
        }

        public void DisplayJoinRequested(JoinLobbyRequest request)
        {
            Debug.Log("Joining lobby...");
        }

        public void DisplayJoinResult(EnterLobbyResult result)
        {
            Debug.Log("Joined a lobby");
        }

        public void DisplayLocalMemberLeft(LeaveInfo info)
        {
            lobbyNameText.text = "No lobby";
            Debug.Log("Left lobby");
        }

        public void DisplaySendInvite(InviteSentInfo info)
        {
            if (info.InviteSent) Debug.Log($"Sending invite to {info.Member}...");
        }

        public void DisplayReceivedInvite(LobbyInvite invite)
        {
            Debug.Log($"Got an invite from {invite.Sender}");
        }

        public void DisplayOtherMemberJoined(MemberJoinedInfo info)
        {
            Debug.Log($"{info.Member} joined");
        }

        public void DisplayOtherMemberLeft(LeaveInfo info)
        {
            Debug.Log($"{info.Member} left. Reason: {info.LeaveReason}");
        }

        public void DisplayUpdateOwner(LobbyMember newOwner)
        {
            Debug.Log($"{newOwner} is the new owner");
        }

        public void DisplayUpdateLobbyData(LobbyDataUpdate update)
        {
            Debug.Log("Lobby data updated");
        }

        public void DisplayUpdateMemberData(MemberDataUpdate update)
        {
            Debug.Log($"{update.Member}'s data updated");
        }

        public void DisplayUpdatedFriendList(IReadOnlyList<LobbyMember> friends)
        {
            
        }

        private void Update()
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                _controller.SendInvite(_controller.GetFriends()[0]);
            }
            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                _controller.SendInvite(_controller.GetFriends()[1]);
            }
        }
    }
}