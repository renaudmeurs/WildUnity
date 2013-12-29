using System;
using System.Timers;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using ExitGames.Client.Photon;
using ExitGames.Client.Photon.Lite;

class ClientPeer : IPhotonPeerListener
{
    private bool connected;

    private string serverAddress;

    private ConnectionProtocol protocol;

    private string applicationName;

    private System.Timers.Timer myTime;

    protected PhotonPeer peer;

    protected OperationRequest request;

    protected OperationResponse response;


    // Event invoqué par la classe
    // ===========================

    // Connected : Lancé lorsque le peer est connecté au server.
    public event EventHandler Connected;

    // Disconnected : Lancé lorsque le peer est déconnecté du serveur.
    public event EventHandler Disconnected;

    // PingReceived : Déclenché lorsque la réponse a un ping est arrivée.
    public event EventHandler PingReceived;

    public event EventHandler FullGameReceived;

    public ClientPeer()
    {
        this.applicationName = "GameServer";
    }

    // Get - Set
    // =========
    public string ServerAddress { get { return this.serverAddress; } set { this.serverAddress = value; } }

    public ConnectionProtocol Protocol { get { return this.protocol; } set { this.protocol = value; } }

    // Fonction de base
    // ================

    // Connection au serveur
    public void Connect()
    {
        this.peer = new PhotonPeer(this, this.protocol);

        try
        {
            Console.WriteLine("Connecting to " + this.serverAddress + " by using " + this.protocol);
            this.peer.Connect(this.serverAddress, this.applicationName);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        myTime = new System.Timers.Timer(50);
        myTime.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerElapsed);
        myTime.Enabled = true;
    }

    // Déconnection du serveur
    public void Disconnect()
    {
        this.peer.Disconnect();
        this.myTime.Enabled = false;
        this.connected = false;
    }

    // Bloque le thread jusqu'a la connection avec le serveur
    private void WaitForConnect()
    {
        while (!this.connected)
        {
            System.Threading.Thread.Sleep(10);
        }
        return;
    }

    // Envoi un message personalisé au serveur (RPC personalisé)
    protected void sendRequest(OperationRequest request, bool sendReliable)
    {
        try
        {
            this.peer.OpCustom(request.OperationCode, request.Parameters, sendReliable);
            this.peer.Service();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }


    // Call-back : Fonction appélée par photon.
    // =========

    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.WriteLine("DebugReturn: " + message);
    }

    // Traite un message de retour
    public void OnOperationResponse(OperationResponse r)
    {
        Debug.WriteLine(r.ToStringFull());
        this.response = r;
        switch (r.OperationCode)
        {
            case 255:
                Debug.WriteLine("Response received");
                object pingStr;
                r.Parameters.TryGetValue(100, out pingStr);
                string str = (string)pingStr;
                Console.WriteLine(str);
                this.PingReceived.Invoke(this, EventArgs.Empty);
                break;

            case 254:
                Debug.WriteLine("Response GetFullGame");

                foreach (KeyValuePair<byte, object> author in r.Parameters)
                {
                    Console.WriteLine("Key: {0}, Value: {1}", author.Key, author.Value.ToString());
                }
                this.FullGameReceived.Invoke(this, EventArgs.Empty);
                object theGame;
                r.Parameters.TryGetValue(100, out theGame);
                byte[] byteGame = (byte[])theGame;
                cStoredFullGame game;
                game = this.ByteArrayToStructure(byteGame);
                Console.WriteLine(game.Map1.mapCells[0].sPosX);
                Console.WriteLine(game.Map1.mapCells[0].ePosX);
                Console.WriteLine(game.Map1.mapCells[0].sPosY);
                Console.WriteLine(game.Map1.mapCells[0].ePosY);
                Console.WriteLine(game.Map1.mapCells[0].idBuilding);
                Console.WriteLine(game.Map1.mapCells[0].levelBuilding);
                break;

            case 253:
                Debug.WriteLine("Response SetFullGame");
                break;
        }
    }

    // Traite un changement de status
    public void OnStatusChanged(StatusCode status)
    {
        switch (status)
        {
            case StatusCode.Connect:
                Debug.WriteLine("Connected");
                this.connected = true;
                this.Connected.Invoke(this, EventArgs.Empty);
                break;
            case StatusCode.Disconnect:
                Debug.WriteLine("Disconnected");
                this.connected = false;
                this.Disconnected.Invoke(this, EventArgs.Empty);
                break;
        }
    }

    // Traite un événement en provenance du serveur
    public void OnEvent(EventData e)
    {

    }

    // Maintient la connection active, traite les messages entrants et sortants.
    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        peer.Service();
    }

    // Operations : Fonction appelable depuis la classe parente. Représente les besoins du jeux
    // ==========

    // Ping
    public void OpPing(string pingString)
    {
        this.request = new OperationRequest
        {
            OperationCode = 255,
            Parameters = new Dictionary<byte, object> { { 100, "Ping" } }
        };
        this.sendRequest(this.request, true);
    }

    // Demande au serveur l'état du jeux
    public void OpGetFullGame(Int32 gameId)
    {
        OperationRequest op = new OperationRequest
        {
            OperationCode = 254,
            Parameters = new Dictionary<byte, object> { { 100, gameId } }
        };
        this.sendRequest(op, true);
    }

    // Envoi du jeux sur le serveur
    public void OpSetFullGame(cStoredFullGame game)
    {
        byte[] buffer = new byte[Marshal.SizeOf(game)];
        Console.WriteLine(Marshal.SizeOf(game));
        buffer = this.StructureToByteArray(game);

        /*for (int i = 0; i < buffer.Length; i++)
            Console.Write(buffer[i]);*/

        OperationRequest op = new OperationRequest
        {
            OperationCode = 253,
            Parameters = new Dictionary<byte, object> { { 100, buffer } }
        };
        try
        {
            this.sendRequest(op, true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    byte[] StructureToByteArray(cStoredFullGame game)
    {

        int len = Marshal.SizeOf(game);

        Console.WriteLine("Len: " + len);

        byte[] arr = new byte[len];

        IntPtr ptr = Marshal.AllocHGlobal(len);

        try
        {
            Marshal.StructureToPtr(game, ptr, false);
            Marshal.Copy(ptr, arr, 0, len);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        return arr;
    }

    cStoredFullGame ByteArrayToStructure(byte[] bytearray)
    {
        GCHandle pinnedPacket = GCHandle.Alloc(bytearray, GCHandleType.Pinned);
        cStoredFullGame game = (cStoredFullGame)Marshal.PtrToStructure(
            pinnedPacket.AddrOfPinnedObject(),
            typeof(cStoredFullGame));
        pinnedPacket.Free();
        return game;
    }

}

