namespace JB
{
    /// <summary>
    /// Defines how <see cref="Pool{TValue}.AcquirePooledValueAsync"/> behaves when no instances are currently available.
    /// </summary>
    public enum PooledValueAcquisitionMode
    {
        /// <summary>
        /// Return an available instance if available, and if not, return the <see cref="Pool{TValue}">default value</see>.
        /// </summary>
        AvailableInstanceOrDefaultValue,

        /// <summary>
        /// Wait for the next available instance.
        /// </summary>
        AvailableInstanceOrWaitForNextOne,
        
        /// <summary>
        /// Create a new instance if none is available and thereby increase the overall pool size.
        /// </summary>
        AvailableInstanceOrCreateNewOne
    }
}