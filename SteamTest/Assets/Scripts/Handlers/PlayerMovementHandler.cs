using System;
using UnityEngine;
using System.Collections.Generic;
using Mirror;

namespace SteamLobbyTutorial
{
    public class PlayerMovementHandler : NetworkBehaviour
    {
        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }
            
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            Vector3 movement = new Vector3(h * 0.25f, v * 0.25f, 0f);
            transform.position = transform.position + movement;
        }
    }
    
}
