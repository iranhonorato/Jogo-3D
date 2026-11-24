using UnityEngine;

namespace Guaxinim.EditorTools
{
    /// <summary>Gôndola de 2 m ou de 4 m, ao longo de X (giro 0) ou de Z (giro 90).</summary>
    public readonly struct ShelfPlacement
    {
        /// <summary>Profundidade da gôndola, igual à do collider do kit.</summary>
        public const float Depth = 0.9f;

        private const float LongLength = 4f;
        private const float ShortLength = 2f;
        private const float AlongZYaw = 90f;

        public ShelfPlacement(bool isLong, Placement placement)
        {
            IsLong = isLong;
            Placement = placement;
        }

        public bool IsLong { get; }
        public Placement Placement { get; }

        public float Length => IsLong ? LongLength : ShortLength;

        /// <summary>Área que a gôndola ocupa no chão, em (x, z).</summary>
        public Rect Footprint
        {
            get
            {
                bool isAlongZ = Mathf.Abs(Mathf.DeltaAngle(Placement.YawDegrees, AlongZYaw)) < 1f ||
                    Mathf.Abs(Mathf.DeltaAngle(Placement.YawDegrees, -AlongZYaw)) < 1f;
                var size = isAlongZ ? new Vector2(Depth, Length) : new Vector2(Length, Depth);
                var center = new Vector2(Placement.Position.x, Placement.Position.z);
                return new Rect(center - size / 2f, size);
            }
        }
    }
}
