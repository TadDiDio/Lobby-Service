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
        }

        public void SetAvatar(Texture2D avatar)
        {
            avatarImage.texture = avatar;
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