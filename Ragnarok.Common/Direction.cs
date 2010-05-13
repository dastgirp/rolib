namespace Ragnarok
{
    /// <summary>
    /// Specifies the direction of an object inside Ragnarok Online.
    /// </summary>
    public enum Direction : byte
    {
        /*
            [1][8][7]
            [2][0][6]
            [3][4][5]
        */

        /// <summary>
        /// Facing northwest.
        /// </summary>
        Northwest = 1,
        /// <summary>
        /// Facing west.
        /// </summary>
        West,
        /// <summary>
        /// Facing southwest.
        /// </summary>
        Southwest,
        /// <summary>
        /// Facing south.
        /// </summary>
        South,
        /// <summary>
        /// Facing southeast.
        /// </summary>
        Southeast,
        /// <summary>
        /// Facing east.
        /// </summary>
        East,
        /// <summary>
        /// Facing northeast.
        /// </summary>
        Northeast,
        /// <summary>
        /// Facing north.
        /// </summary>
        North
    }
}
