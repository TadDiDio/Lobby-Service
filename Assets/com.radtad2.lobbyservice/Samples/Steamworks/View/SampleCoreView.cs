using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LobbyService.Samples.Steam
{
    public class SampleCoreView : MonoBehaviour, ILobbyCoreView, ILobbyFriendView
    {
        public TMP_Text lobbyNameText;
        public TMP_Text localUserText;

        public Button createButton;
        public Button friendsButton;
        
        public TMP_InputField nameInput;
        public Slider capacitySlider;
        public TMP_Text capacityText;
        
        public GameObject friendsPanel;
        public GameObject friendContainer;
        public GameObject friendCardPrefab;
        
        private LobbyController _controller;

        public void SetController(LobbyController controller)
        {
            _controller = controller;
        }

        private void Awake()
        {
            createButton.onClick.AddListener(Create);
            friendsButton.onClick.AddListener(ToggleFriends);
            capacitySlider.onValueChanged.AddListener(UpdateCapacity);
        }

        private void OnDestroy()
        {
            createButton.onClick.RemoveAllListeners();
            friendsButton.onClick.RemoveAllListeners();
            capacitySlider.onValueChanged.RemoveAllListeners();
        }

        private void ToggleFriends()
        {
            friendsPanel.SetActive(!friendsPanel.activeSelf);
        }

        private void UpdateCapacity(float value)
        {
            capacityText.text = value.ToString();
        }
        
        private void Create()
        {
            _controller?.Create(new CreateLobbyRequest
            {
                Capacity = (int)capacitySlider.value,
                LobbyType =  LobbyType.Public,
                Name = nameInput.text
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
            if (!result.Success) return;
            
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

        private struct FriendCard
        {
            public GameObject Card;
            public LobbyMember Member;
            public LobbyController Controller;
            public void Invite()
            {
                Controller.SendInvite(Member);
            }
            public void Destroy()
            {
                Card.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                Object.Destroy(Card);
            }
        }
        
        private List<FriendCard> _friendCards = new();
        public void DisplayUpdatedFriendList(IReadOnlyList<LobbyMember> friends)
        {
            _friendCards.ForEach(c => c.Destroy());
            _friendCards.Clear();
            
            foreach (var friend in friends)
            {
                var card = new FriendCard
                {
                    Card = Instantiate(friendCardPrefab, friendContainer.transform),
                    Member = friend,
                    Controller = _controller
                };

                card.Card.GetComponentInChildren<TMP_Text>().text = $"Invite {friend} to lobby";
                card.Card.GetComponentInChildren<Button>().onClick.AddListener(card.Invite);
                _friendCards.Add(card);
            }
        }

        public void Reset()
        {
            localUserText.text = $"You are {_controller.LocalMember.DisplayName}";
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