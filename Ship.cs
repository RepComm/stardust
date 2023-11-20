
using System.Collections.Generic;
using Godot;

public class Thruster {
  public Vector3 force;
  public Vector3 position;
  public Thruster () {
    this.position = new();
    this.force = new(0,2,0);
  }
}

public partial class Ship : RigidBody3D {
  
  List<Thruster> thrusters;
  Vector3 thrusterForce;
  Vector3 thrusterGlobalPos;

  public Ship () {
    this.thrusters = new();
    this.thrusterForce = new();
    this.thrusterGlobalPos = new();
  }

  public void clearThrusters () {
    this.thrusters.Clear();
  }
  public Thruster createThruster () {
    var result = new Thruster();
    this.thrusters.Add(result);
    return result;
  }

  public override void _Ready() {
    // this.thrusters = new();

  }
  public override void _IntegrateForces(PhysicsDirectBodyState3D state) {
    
    var IT = this.Transform.Basis.Inverse();

    foreach (var thruster in this.thrusters) {
      //force experienced from point of view of the thruster
      //aka global space
      this.thrusterForce.X = thruster.force.X;
      this.thrusterForce.Y = thruster.force.Y;
      this.thrusterForce.Z = thruster.force.Z;

      //transform into local space
      this.thrusterForce *= IT;//this.Transform.Basis.Inverse();

      this.thrusterGlobalPos.X = thruster.position.X;
      this.thrusterGlobalPos.Y = thruster.position.Y;
      this.thrusterGlobalPos.Z = thruster.position.Z;

      this.thrusterGlobalPos *= IT;

      state.ApplyForce(
        this.thrusterForce,
        this.thrusterGlobalPos
      );
      // state.ApplyCentralForce(this.thrusterForce);
    }
    base._IntegrateForces(state);

  }
}
