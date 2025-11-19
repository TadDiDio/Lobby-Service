using System.Collections.Generic;
using LobbyService.LocalServer;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LobbyService.Example
{
    public class SampleView : MonoBehaviour, ICoreView, IFriendView
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

        public Button leaveButton;
        public Button closeButton;
        
        private LobbyInvite? _invite;
        
        private Dictionary<LobbyMember, MemberCard> _members = new();
        
        private void Awake()
        {
            createButton.onClick.AddListener(Create);
            friendsButton.onClick.AddListener(ToggleFriends);
            capacitySlider.onValueChanged.AddListener(UpdateCapacity);
            
            acceptInviteButton.onClick.AddListener(AcceptInvite);
            rejectInviteButton.onClick.AddListener(RejectInvite);
            
            leaveButton.onClick.AddListener(Leave);
            closeButton.onClick.AddListener(Close);
        }

        private void OnDestroy()
        {
            createButton.onClick.RemoveAllListeners();
            friendsButton.onClick.RemoveAllListeners();
            capacitySlider.onValueChanged.RemoveAllListeners();
            acceptInviteButton.onClick.RemoveAllListeners();
            rejectInviteButton.onClick.RemoveAllListeners();
            leaveButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();
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
            card.Initialize(member);

            if (Lobby.IsOwner && Lobby.LocalMember != member)
            {
                card.EnableOwnerButtons(true);
                card.kickButton.onClick.AddListener(() => Lobby.KickMember(member));
                card.promoteButton.onClick.AddListener(() => Lobby.SetOwner(member));
            }
            
            card.SetOwner(member == Lobby.Model.Owner);
            
            _members.Add(member, card);
        }
        
        private void Create()
        {
            Lobby.Create(new CreateLobbyRequest
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
            
            Lobby.Join(new JoinLobbyRequest
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
            Lobby.Leave();
        }

        public void Close()
        {
            Lobby.Close();
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

            OnEnterLobby();
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

            OnEnterLobby();
        }

        private void OnEnterLobby()
        {
            lobbyNameText.text = Lobby.GetLobbyDataOrDefault(LobbyKeys.NameKey, $"{Lobby.Model.Owner}'s lobby");

            if (string.IsNullOrEmpty(lobbyNameText.text) && Lobby.IsOwner)
            {
                Lobby.SetLobbyData(LobbyKeys.NameKey, $"{Lobby.LocalMember}'s lobby");
            }
            
            leaveButton.gameObject.SetActive(true);
            SetViewIsOwner(Lobby.IsOwner);
        }
        
        public void DisplayLocalMemberLeft(LeaveInfo info)
        {
            localUserText.text = $"You are {Lobby.LocalMember.DisplayName}";
            lobbyNameText.text = "Create or join a lobby";

            leaveButton.gameObject.SetActive(false);
            SetViewIsOwner(false);
            
            foreach (var member in _members.Values)
            {
                Destroy(member.gameObject);
            }
            _members.Clear();
        }

        public void DisplaySentInvite(InviteSentInfo info)
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
            if (info.LeaveReason is LeaveReason.Kicked) Debug.Log($"{info.Member} was Kicked");
            if (!_members.TryGetValue(info.Member, out var card)) return;
            
            Destroy(card.gameObject);
            _members.Remove(info.Member);
        }

        public void DisplayUpdateOwner(LobbyMember newOwner)
        {
            SetViewIsOwner(Lobby.IsOwner);
            
            foreach (var card in _members.Values)
            {
                card.SetOwner(newOwner == card.Member);
                card.EnableOwnerButtons(Lobby.IsOwner && Lobby.LocalMember != card.Member);

                if (Lobby.IsOwner && Lobby.LocalMember != card.Member)
                {
                    card.kickButton.onClick.AddListener(() => Lobby.KickMember(card.Member));
                    card.promoteButton.onClick.AddListener(() => Lobby.SetOwner(card.Member));
                }
            }
        }

        private void SetViewIsOwner(bool isOwner)
        {
            closeButton.gameObject.SetActive(isOwner);
        }
        
        public void DisplayUpdateLobbyData(LobbyDataUpdate update)
        {
            lobbyNameText.text = update.Data.GetOrDefault(LobbyKeys.NameKey, "UNKNOWN NAME");
        }

        public void DisplayUpdateMemberData(MemberDataUpdate update)
        {
            Debug.Log($"{update.Member} ready: {update.Data.GetOrDefault(LobbyKeys.ReadyKey, "false")}");
        }

        private struct FriendCard
        {
            public GameObject Card;
            public LobbyMember Member;
            public void Invite()
            {
                Lobby.SendInvite(Member);
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
                };

                card.Card.GetComponentInChildren<TMP_Text>().text = $"Invite {friend} to lobby";
                card.Card.GetComponentInChildren<Button>().onClick.AddListener(card.Invite);
                _friendCards.Add(card);
            }
        }

        public void DisplayFriendAvatar(LobbyMember member, Texture2D avatar) { }

        public void ResetView(ILobbyCapabilities capabilities)
        {
            localUserText.text = $"You are {Lobby.LocalMember.DisplayName}";
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
                Lobby.SetLobbyData(LobbyKeys.NameKey, "Test name lol");
            }
            
            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                var ready = Lobby.GetMemberDataOrDefault(Lobby.LocalMember, LobbyKeys.ReadyKey, "false");

                var flag = bool.Parse(ready);
                
                Lobby.SetMemberData(LobbyKeys.ReadyKey, (!flag).ToString());
            }
        }
    }
}