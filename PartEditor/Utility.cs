using SFS.Parts;
using SFS.Variables;
using UnityEngine;

namespace PartEditor
{
    // ReSharper disable InconsistentNaming
    public class Part_Local : Obs<Part>
    {
        protected override bool IsEqual(Part a, Part b) => a == b;
    }

    public class Vector2Int_Local : Obs<Vector2Int>
    {
        protected override bool IsEqual(Vector2Int a, Vector2Int b) => a == b;
    }
}