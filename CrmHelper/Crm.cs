using CrmHelper.Extensions;
using Microsoft.Xrm.Sdk.Discovery;

namespace CrmHelper
{
    public class Crm
    {
        public static OrganizationDetailCollection GetOrganizationDetailCollection(string connectionString)
        {
            var connection = CrmConnectionString.Deserialize(connectionString);
            return new OrganizationDetailCollection();
        }
    }
}
