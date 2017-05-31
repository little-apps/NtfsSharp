using System;

namespace NtfsSharp.Tests.Driver.Attributes.NonResident
{
    public class VirtualCluster
    {
        public readonly ulong Lcn;
        public readonly ulong Vcn;
        public readonly BaseDriverCluster AbsoluteCluster;
        public readonly NonResidentAttributeBase ParentNonResidentAttribute;

        /// <summary>
        /// Constructor for VirtualCluster
        /// </summary>
        /// <param name="absoluteCluster">Actual cluster</param>
        /// <param name="lcn">Logical cluster number of cluster</param>
        /// <param name="vcn">Virtual cluster number of cluster</param>
        /// <param name="nonResidentAttribute">Non resident attribute containing virtual cluster</param>
        /// <exception cref="ArgumentNullException">Thrown if absoluteCluster is null.</exception>
        public VirtualCluster(BaseDriverCluster absoluteCluster, ulong lcn, ulong vcn, NonResidentAttributeBase nonResidentAttribute)
        {
            Lcn = lcn;
            Vcn = vcn;
            AbsoluteCluster = absoluteCluster ??
                              throw new ArgumentNullException(nameof(absoluteCluster),
                                  "Absolute cluster cannot be null.");
            ParentNonResidentAttribute = nonResidentAttribute;
        }

        
    }
}
