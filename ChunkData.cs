
using System;
using Godot;

public class ChunkData {
  public readonly int dimensionSize = 8;
  int blockByteCount = 1;

  /** 1d fixed length block data storage
    * readonly to disallow assigning by other code, but feel free to assign to it's data
    * use [read|write]BlockAt[Pos|Index] methods, or access the data directly
    */
  public readonly uint[] data;

  int blockCount;
  int dataByteCount;

  /** data as read or written to an individual block
    * use [read|write]BlockAt[Pos|Index] methods to populate/write this field
    * readonly to disallow assigning by other code, but feel free to assign to it's data
    */
  public readonly uint[] readBlockData;

  public ChunkData () {
    //calc num of blocks based on dimensions
    this.blockCount = (
      this.dimensionSize *
      this.dimensionSize *
      this.dimensionSize
    );

    //calculate data alloc size based on block count and bytes per block
    this.dataByteCount = this.blockCount * blockByteCount;

    //alloc data for blocks in 1 dimensional fixed array
    this.data = new uint[this.dataByteCount];

    this.readBlockData = new uint[this.blockByteCount];
  }

  public int posToIdx(Vector3 v) {
    return (int)(
      Math.Floor(v.X) +
      Math.Floor(v.Y) * this.dimensionSize +
      Math.Floor(v.Z) * this.dimensionSize * this.dimensionSize
    );
  }

  public void idxToPos(int index, out Vector3 v) {
    v.Z = index % this.dimensionSize;
    v.Y = (index / this.dimensionSize) % this.dimensionSize;
    v.X = index / (this.dimensionSize * this.dimensionSize);
  }

  public bool isPosBounded(Vector3 v) {
    return (
      v.X > -1 && v.X < this.dimensionSize &&
      v.Y > -1 && v.Y < this.dimensionSize &&
      v.Z > -1 && v.Z < this.dimensionSize
    );
  }

  public void readBlockAtIndex (int idx) {

    //blockOffset to byte offset
    idx *= this.blockByteCount;

    //copy the data
    for (int i=0; i<this.blockByteCount; i++) {
      this.readBlockData[i] = this.data[idx + i];
    }
  }
  
  public void readBlockAtPos (Vector3 pos) {
    int idx = this.posToIdx(pos);
    this.readBlockAtIndex(idx);
  }

  public void writeBlockAtIndex (int idx) {
    //blockOffset to byte offset
    idx *= this.blockByteCount;

    //copy the data
    for (int i=0; i<this.blockByteCount; i++) {
      this.data[idx + i] = this.readBlockData[i];
    }
  }

  public void writeBlockAtPos (Vector3 pos) {
    int idx = this.posToIdx(pos);
    // GD.Print("V", pos, "->",idx);
    this.writeBlockAtIndex(idx);
  }

}
