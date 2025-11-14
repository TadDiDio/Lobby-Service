using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyService.LocalServer
{
    public class MemberCard : MonoBehaviour
    {
        public TMP_Text memberName;
        public RawImage avatarImage;
        public Button kickButton;
        public Image ownerCrown;
        
        public LobbyMember Member;
        
        private void Awake()
        {
            kickButton.gameObject.SetActive(false);
            ownerCrown.gameObject.SetActive(false);
        }

        public void Initialize(LobbyController controller, LobbyMember member)
        {
            Member = member;
            memberName.text = member.DisplayName;
            _ = GetAvatar(controller, member);
        }

        private async Task GetAvatar(LobbyController controller, LobbyMember member)
        {
            try
            {
                var avatar = await controller.GetFriendAvatar(member, destroyCancellationToken);
                if (avatar) avatarImage.texture = avatar;    
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public Button EnableKickButton(bool buttonEnabled)
        {
            kickButton.gameObject.SetActive(buttonEnabled);
            return kickButton;
        }

        public void SetOwner(bool isOwner)
        {
            ownerCrown.gameObject.SetActive(isOwner);
        }
    }
}