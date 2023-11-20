
using Godot;

public partial class Player : RigidBody3D {
  Node3D cameraPivot;
  Camera3D camera;

  RayCast3D groundDetector;

  float fwdAmt;
  float strafeAmt;
  float jumpAmt;
  Vector3 walkForce;
  Vector3 jumpForce;

  Vector2 look;
  float lookSensitivity = 0.005f;

  public override void _Ready() {
    this.cameraPivot = GetNode<Node3D>("CameraPivot");
    this.camera = this.cameraPivot.GetNode<Camera3D>("PlayerCamera");
    this.groundDetector = GetNode<RayCast3D>("GroundDetector");

    this.fwdAmt = 0;
    this.strafeAmt = 0;
    this.jumpAmt = 0;
    this.walkForce = new Vector3();
    this.look = new Vector2();
    this.jumpForce = new Vector3();
  }

  public override void _Input(InputEvent @event) {
    if (@event is InputEventMouseButton emb) {
      if (Input.MouseMode != Input.MouseModeEnum.Captured) {
        Input.MouseMode = Input.MouseModeEnum.Captured;
      }
    } else if (@event is InputEventMouseMotion emm) {
      if (Input.MouseMode == Input.MouseModeEnum.Captured) {
        look.X = emm.Relative.X;
        look.Y = emm.Relative.Y;
      }
    }
  }

  public override void _Process(double delta) {
    if (Input.IsActionPressed("MouseReleaseCapture")) {
      Input.MouseMode = Input.MouseModeEnum.Visible;
    }
    if (Input.IsActionPressed("walk_fwd")) {
      this.fwdAmt = -1f;
    } else if (Input.IsActionPressed("walk_bwd")) {
      this.fwdAmt = 1f;
    } else {
      this.fwdAmt = 0f;
    }

    if (Input.IsActionPressed("walk_lft")) {
      this.strafeAmt = -1f;
    } else if (Input.IsActionPressed("walk_rgt")) {
      this.strafeAmt = 1f;
    } else {
      this.strafeAmt = 0f;
    }

    if (Input.IsActionPressed("jump") && this.groundDetector.IsColliding()) {
      this.jumpAmt = 2f;
    } else {
      this.jumpAmt = 0f;
    }

    this.camera.RotateX(-this.look.Y * this.lookSensitivity);
    this.cameraPivot.RotateY(-this.look.X * this.lookSensitivity);

    var Basis = this.cameraPivot.Transform.Basis;
    var fwd = Basis.Z.Normalized() * this.fwdAmt;
    var strafe = Basis.X.Normalized() * this.strafeAmt;
    this.jumpForce = Basis.Y.Normalized() * this.jumpAmt;

    this.walkForce = (fwd+strafe).Normalized() * 20;

    this.look.X = 0; //consume look movement
    this.look.Y = 0;
    this.jumpAmt = 0;
  }

  public override void _IntegrateForces(PhysicsDirectBodyState3D state)
  {
    state.ApplyForce(this.walkForce);
    state.ApplyImpulse(this.jumpForce);
    base._IntegrateForces(state);

  }
}
