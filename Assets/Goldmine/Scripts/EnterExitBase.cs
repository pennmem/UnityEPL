using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterExitBase : MonoBehaviour {
    private GameObject player;
    private ControlPlayer controlPlayer;

    void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
        controlPlayer = player.GetComponent<ControlPlayer>();
    }

    void OnTriggerEnter(Collider collision) {
        if (collision.gameObject.tag == "Player") {
            controlPlayer.playerAtBase = !controlPlayer.playerAtBase;
        }
    }
}
