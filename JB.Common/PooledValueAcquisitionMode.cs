namespace JB
{
    /// <summary>
    /// Defines how <see cref="IPool{TValue}.AcquirePooledValueAsync"/> behaves when no instances are currently available.
    /// </summary>
    public enum PooledValueAcquisitionMode
    {
        /// <summary>
        /// Wait for the next available instance.
        /// </summary>
        WaitForNextAvailableInstance,
        
        /// <summary>
        /// Create a new instance if none is available and thereby increase the overall pooled count.
        /// </summary>
        CreateNewInstanceIfNoneIsAvailable
    }
}