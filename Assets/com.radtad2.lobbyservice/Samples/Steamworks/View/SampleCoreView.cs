using LobbyService.LocalServer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LobbyService.Samples.Steam
{
    public class SampleCoreView : MonoBehaviour, ILobbyCoreView
    {
        private LobbyController _controller;
        private void Start()
        {
            _controller = FindAnyObjectByType<LobbyController>();
            _controller.ConnectView(this);

        }

        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                LocalLobby.Create("Test lobby");
            }
        }

        public void DisplayExistingLobby(IReadonlyLobbyModel snapshot)
        {
            throw new System.NotImplementedException();
        }

        public void DisplayCreateRequested(CreateLobbyRequest request)
        {
            throw new System.NotImplementedException();
        }

        public void DisplayCreateResult(EnterLobbyResult result)
        {
            throw new System.NotImplementedException();
        }

        public void DisplayJoinRequested(JoinLobbyRequest request)
        {
            throw new System.NotImplementedException();
        }

        public void DisplayJoinResult(EnterLobbyResult result)
        {
            throw new System.NotImplementedException();
        }

        public void DisplayLocalMemberLeft(LeaveInfo info)
        {
            throw new System.NotImplementedException();
        }

        public void DisplaySendInvite(InviteSentInfo info)
        {
            throw new System.NotImplementedException();
        }

        public void DisplayReceivedInvite(LobbyInvite invite)
        {
            throw new System.NotImplementedException();
        }

        public void DisplayOtherMemberJoined(MemberJoinedInfo info)
        {
            throw new System.NotImplementedException();
        }

        public void DisplayOtherMemberLeft(LeaveInfo info)
        {
            throw new System.NotImplementedException();
        }

        public void DisplayUpdateOwner(LobbyMember newOwner)
        {
            throw new System.NotImplementedException();
        }

        public void DisplayUpdateLobbyData(LobbyDataUpdate update)
        {
            throw new System.NotImplementedException();
        }

        public void DisplayUpdateMemberData(MemberDataUpdate update)
        {
            throw new System.NotImplementedException();
        }
    }
}