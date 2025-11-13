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
        
        public void Initialize(LobbyController controller, LobbyMember member)
        {
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
    }
}