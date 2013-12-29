using UnityEngine;
using System;
using System.Collections;

public class CMainGame : MonoBehaviour {

	// Afin de faire fonctionner ClientPeer, il faut ajouter l'assembly Photon3Unity3D.dll dans le projet. On la trouve dans le SDK Photon pour Unity iOS.
	ClientPeer peer;
	cStoredFullGame game;

	// Use this for initialization
	void Start () {

		this.peer = new ClientPeer ();

		// Connection des events avec des fonctions "locales"
		this.peer.Connected += new EventHandler(Peer_ConnectedEvent);
		this.peer.Disconnected += new EventHandler(Peer_DisconnectedEvent);
		this.peer.FullGameReceived += new EventHandler(Peer_FullGameReceived);

		// Initialisation du peer
		this.peer.ServerAddress = "54.194.106.36:4530";	//	Serveur sur le cloud Amazon Web Service. 
		this.peer.Protocol = ExitGames.Client.Photon.ConnectionProtocol.Tcp;
		//
		// Etablissement de la connection...
		this.peer.Connect ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void Peer_ConnectedEvent(object sender, EventArgs e)
	{
		Debug.Log("<color=green>Connected to server !</color>");

		// Demander les données complètes du jeux.
		this.peer.OpGetFullGame (127);		//	En paramètres, l'id du jeu. To do: Lier un id de jeux avec un Player. (GameCenter, Compte Google, etc...)
	}

	void Peer_DisconnectedEvent(object sender, EventArgs e)
	{
		Debug.Log("Disconnected to the server !");
	}

	void Peer_FullGameReceived(object sender, EventArgs e) // Va etre modifié : Je vais créer un custom event afin de recevoir en paramètre une classe cStoredFullGame.
	{
		Debug.Log("Full game data received from the server !");
		// To do : Charger le jeu : Dessiner la map, alimenter les compteurs, etc...
	}
}
