using System.Collections.Generic;

namespace DockerPsMonitor
{
    public class DockerProcessInfoComparer : IEqualityComparer<DockerProcessInfo>
    {
        public bool Equals(DockerProcessInfo p1, DockerProcessInfo p2)
        {
            //Check whether the compared objects reference the same data.
            if (ReferenceEquals(p1, p2)) return true;

            //Check whether any of the compared objects is null.
            if (ReferenceEquals(p1, null) || ReferenceEquals(p2, null))
                return false;

            //Check whether the properties are equal.
            return p1.ID.Equals(p2.ID);
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(DockerProcessInfo dockerProcessInfo)
        {
            //Calculate the hash code
            return dockerProcessInfo.ID == null ? 0 : dockerProcessInfo.ID.GetHashCode();
        }
    }
}
