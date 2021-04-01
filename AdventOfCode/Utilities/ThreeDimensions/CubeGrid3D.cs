using Garyon.DataStructures;

namespace AdventOfCode.Utilities.ThreeDimensions
{
    // Not today, my friend
    public class CubeGrid3D<T> : Grid3D<T>
    {
        public int Size => Width;

        #region Constructors
        protected CubeGrid3D(int size, T defaultValue, ValueCounterDictionary<T> valueCounters)
            : base(size, size, size, defaultValue, valueCounters) { }

        // TODO: More constructors might be needed
        public CubeGrid3D(int size, T defaultValue = default)
            : base(size, defaultValue) { }
        public CubeGrid3D(CubeGrid3D<T> other)
            : base(other) { }
        #endregion
    }
}
