
using System;
using System.Text;
using Godot;

public partial class Client : Node {
  private WebSocketPeer ws;
  private bool wsOpenTracker = false;
  private Timer wsTimer;
  private int maxMsgProcessPerTick;

  public Client () {
    this.maxMsgProcessPerTick = 5;
  }

  public override void _Ready() {
    GD.Print("Attempting to connect");
    this.connect("localhost", 10209);
  }

  public override void _Process(double delta) {
    if (this.ws == null) return;

    this.ws.Poll();
    var s = this.ws.GetReadyState();

    switch (s) {
      case WebSocketPeer.State.Open:
        if (!this.wsOpenTracker) {
          this.wsOpenTracker = true;
          this.onOpen();
        }
        var available = this.ws.GetAvailablePacketCount();    
        var max = Math.Min(this.maxMsgProcessPerTick, available);
        for (int i=0; i<max; i++) {
          var p = this.ws.GetPacket();
          var wasStr = this.ws.WasStringPacket();
          if (!wasStr) {
            this.onWsBuffer(p);
          } else {
            var str = Encoding.UTF8.GetString(p);
            this.onWsString(str);
          }
        }
      break;
      case WebSocketPeer.State.Closing:
        //keep polling so the connection closes properly
      break;
      case WebSocketPeer.State.Closed:
        var code = this.ws.GetCloseCode();
        var reason = this.ws.GetCloseReason();
        this.ws = null; //stop polling
        this.wsOpenTracker = false;
        this.onClose(code, reason);
        break;
    }
  }
  private void onClose (int code, string reason) {

  }
  public void disconnect () {
    this.ws?.Close(0);
  }
  public void connect (string hostname, int hostport) {
    if (this.ws != null) {
      throw new Exception("WS already instanced, call disconnect() first and wait for onClose");
    }
    this.ws = new WebSocketPeer();
    this.ws.ConnectToUrl("ws://" + hostname + ":" + hostport);
  }

  private void onOpen () {
    GD.Print("Connected");
    this.ws.SendText("{ \"id\": 0, \"type\": \"auth\" }");
  }
  private void onWsBuffer (byte[] raw) {
    
  }
  private void onWsString (string msg) {
    GD.Print("Received data from server", msg);
  }
}
