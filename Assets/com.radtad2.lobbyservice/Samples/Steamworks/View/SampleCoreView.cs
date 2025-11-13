using System.Collections.Generic;
using LobbyService.LocalServer;
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

        public Button acceptInviteButton;
        public Button rejectInviteButton;
        public GameObject invitePanel;
        public TMP_Text inviteText;
        
        public MemberCard memberCardPrefab;
        public GameObject memberCardContainer;

        private LobbyInvite? _invite;
        
        private LobbyController _controller;
        private Dictionary<LobbyMember, MemberCard> _members = new();
        
        public void SetController(LobbyController controller)
        {
            _controller = controller;
        }

        private void Awake()
        {
            createButton.onClick.AddListener(Create);
            friendsButton.onClick.AddListener(ToggleFriends);
            capacitySlider.onValueChanged.AddListener(UpdateCapacity);
            
            acceptInviteButton.onClick.AddListener(AcceptInvite);
            rejectInviteButton.onClick.AddListener(RejectInvite);
        }

        private void OnDestroy()
        {
            createButton.onClick.RemoveAllListeners();
            friendsButton.onClick.RemoveAllListeners();
            capacitySlider.onValueChanged.RemoveAllListeners();
            acceptInviteButton.onClick.RemoveAllListeners();
            rejectInviteButton.onClick.RemoveAllListeners();
        }

        private void ToggleFriends()
        {
            friendsPanel.SetActive(!friendsPanel.activeSelf);
        }

        private void UpdateCapacity(float value)
        {
            capacityText.text = value.ToString();
        }
        
        private void AddMember(LobbyMember member)
        {
            var card = Instantiate(memberCardPrefab, memberCardContainer.transform);
            card.Initialize(_controller, member);
            _members.Add(member, card);
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

        private void AcceptInvite()
        {
            invitePanel.SetActive(false);
            if (!_invite.HasValue) return;
            
            _controller.Join(new JoinLobbyRequest
            {
                LobbyId = _invite.Value.LobbyId
            });
        }

        private void RejectInvite()
        {
            invitePanel.SetActive(false);
            _invite = null;
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

            AddMember(result.LocalMember);
            lobbyNameText.text = $"{result.LocalMember.DisplayName}'s lobby";
        }

        public void DisplayJoinRequested(JoinLobbyRequest request)
        {
            Debug.Log("Joining lobby...");
        }

        public void DisplayJoinResult(EnterLobbyResult result)
        {
            if (!result.Success) return;

            foreach (var member in result.Members)
            {
                AddMember(member);
            }
            
            lobbyNameText.text = $"{result.LocalMember.DisplayName}'s lobby";
        }

        public void DisplayLocalMemberLeft(LeaveInfo info)
        {
            Reset();
        }

        public void DisplaySendInvite(InviteSentInfo info)
        {
            if (info.InviteSent) Debug.Log($"Sending invite to {info.Member}...");
        }

        public void DisplayReceivedInvite(LobbyInvite invite)
        {
            invitePanel.SetActive(true);
            inviteText.text = $"Accept invitation from {invite.Sender}";
            _invite = invite;
        }

        public void DisplayOtherMemberJoined(MemberJoinedInfo info)
        {
            AddMember(info.Member);
        }
        
        public void DisplayOtherMemberLeft(LeaveInfo info)
        {
            if (!_members.TryGetValue(info.Member, out var card)) return;
            
            Destroy(card.gameObject);
            _members.Remove(info.Member);
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
            lobbyNameText.text = "Create or join a lobby";

            foreach (var member in _members.Values)
            {
                Destroy(member.gameObject);
            }
            _members.Clear();

            foreach (var friend in _friendCards)
            {
                friend.Destroy();
            }
            _friendCards.Clear();
            
            nameInput.text = string.Empty;
            capacitySlider.value = 4;
            capacityText.text = "4";
            friendsPanel.SetActive(false);
            invitePanel.SetActive(false);
            inviteText.text = "Accept invitation from ";
            _invite = null;
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