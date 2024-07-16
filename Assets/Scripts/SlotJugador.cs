using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
public class SlotJugador : MonoBehaviour
{
    [SerializeField] private TMP_Text nickname;

    //Importar la librería using Photon.Realtime;
    private Player player;

    public Player Player
    {
        get => player;
        set
        {
            player = value;
            if (player != null)
            {
                nickname.text = player.NickName;
            }
        }
    }


}