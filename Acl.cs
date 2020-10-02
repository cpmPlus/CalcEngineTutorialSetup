using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABB.Vtrin;

namespace CalcEngineTutorialSetup
{
    class Acl
    {
        static public void CreateAcl(string objectName, string objectRef)
        {
            var aclCache = Context.Driver.Classes["UIAccessControlListEntry"].Instances;

            var aclEntrySet = Context.Driver.Classes["UIAccessControlListEntry"].Instances.GetInstanceSet($"Object LIKE '{objectName}'");

            var aclEntry = aclEntrySet.Length > 0 ? aclEntrySet[0]?.BeginUpdate() : null;
            if (aclEntry == null)
            {
                aclEntry = aclCache.Add();
            }

            aclEntry.SetRawPropertyValue("ObjectRef", objectRef);
            aclEntry["GroupOrUserName"] = Context.Group;
            aclEntry.SetRawPropertyValue("Allow", cDbPermissions.Read | cDbPermissions.Write | cDbPermissions.Execute);

            aclEntry.CommitChanges();
        }

        static public void RemoveAclEntries(string objectLike)
        {
            var aclEntries = Context.Driver.Classes["UIAccessControlListEntry"].Instances.GetInstanceSet($"Object LIKE '{objectLike}'");

            foreach (var aclEntry in aclEntries)
            {
                aclEntry.Remove();
            }
        }
    }
}
