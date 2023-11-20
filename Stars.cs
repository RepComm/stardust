
using Godot;

public partial class Stars : Node3D {
  RigidBody3D player;
  public override void _Ready () {
    this.player = GetNode<RigidBody3D>("/root/Scene/PlayerPhysics");
  }
  public override void _Process(double delta) {
    if (this.player == null) return;
    this.GlobalPosition = this.player.GlobalPosition;
  }
}
