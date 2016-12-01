using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using System.Configuration;
using System.Linq;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Identity.Client;
using Microsoft.VisualStudio.Services.Licensing;
using Microsoft.VisualStudio.Services.Licensing.Client;
using Microsoft.VisualStudio.Services.Common;
using System.IO;

namespace VSTS_MSID_TO_ORGID_MIGRATION
{
    class Program
    {
        static void Main(string[] args)
        {
            //reading values from app config
            string _personalAccessToken = ConfigurationSettings.AppSettings["appsetting.pat"].ToString();
            string _credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _personalAccessToken)));
            string _vstsaccountname = ConfigurationSettings.AppSettings["appsetting.vstsacountname"].ToString();
            string filepath = ConfigurationSettings.AppSettings["appsetting.filepath"].ToString();
            var csvdata = from line in File.ReadAllLines(filepath).Skip(1)
                          let columns = line.Split(',')
                          select new
                          {
                              msid = columns[0],
                              orgid = columns[1]

                          };
            foreach (var row in csvdata)
            {
                AddUserToAccount(row.orgid); //currently incomplete as Assigning Work items won't work till users added to project team

                //WIQL query for querying all work items based on Assigned to field
                Object wiql = new
                {
                    query = "Select ID " +
                         "From WorkItems " +
                         "Where [System.AssignedTo] = '" + row.msid + "'"

                };
                //Using REST API to Update the Feild
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                    var postValue = new StringContent(JsonConvert.SerializeObject(wiql), Encoding.UTF8, "application/json");


                    var method = new HttpMethod("POST");


                    var request = new HttpRequestMessage(method, "https://" + _vstsaccountname + ".visualstudio.com/" + "_apis/wit/wiql?api-version=2.2") { Content = postValue };
                    var response = client.SendAsync(request).Result;
                    var result = response.Content.ReadAsAsync<GetWorkItemsResponse.Results>().Result;
                    Object[] patchDocument = new Object[1];
                    patchDocument[0] = new { op = "add", path = "/fields/System.AssignedTo", value = row.orgid };
                    method = new HttpMethod("PATCH");
                    for (int i = 0; i < result.workItems.Length; i++)
                    {

                        var patchValue = new StringContent(JsonConvert.SerializeObject(patchDocument), Encoding.UTF8, "application/json-patch+json");
                        request = new HttpRequestMessage(method, "https://" + _vstsaccountname + ".visualstudio.com/_apis/wit/workitems/" + result.workItems[i].id + "?api-version=2.2") { Content = patchValue };
                        response = client.SendAsync(request).Result;

                    }


                }
            }
        }

      
        static void AddUserToAccount(string username)
        {
            //Reference taken from https://blogs.msdn.microsoft.com/buckh/2014/10/07/how-to-add-licensed-users-to-vs-online-via-the-api/
            try
            {
                string _personalAccessToken = ConfigurationSettings.AppSettings["appsetting.pat"].ToString();
                var creds = new VssBasicCredential("", _personalAccessToken);
                string _vstsaccountname = ConfigurationSettings.AppSettings["appsetting.vstsacountname"].ToString();
                var vssConnection = new VssConnection(new Uri("https://" + _vstsaccountname + ".vssps.visualstudio.com"), creds);
                string aadtenantid = ConfigurationSettings.AppSettings["appsetting.aadtenantid"].ToString();

                var licensingClient = vssConnection.GetClient<LicensingHttpClient>();
                var identityClient = vssConnection.GetClient<IdentityHttpClient>();


                var collectionScope = identityClient.GetScopeAsync(_vstsaccountname).Result;


                var licensedUsersGroupDescriptor = new IdentityDescriptor(IdentityConstants.TeamFoundationType,
                                                                          GroupWellKnownSidConstants.LicensedUsersGroupSid);


                var identifier = String.Concat(SidIdentityHelper.GetDomainSid(collectionScope.Id),
                                               SidIdentityHelper.WellKnownSidType,
                                               licensedUsersGroupDescriptor.Identifier.Substring(SidIdentityHelper.WellKnownSidPrefix.Length));


                var collectionLicensedUsersGroupDescriptor = new IdentityDescriptor(IdentityConstants.TeamFoundationType,
                                                                                    identifier);



                var upnIdentity = string.Format("upn:{0}\\{1}", aadtenantid, username);


                var newUserDesciptor = new IdentityDescriptor(IdentityConstants.BindPendingIdentityType,
                                                                  upnIdentity);



                bool result = identityClient.AddMemberToGroupAsync(collectionLicensedUsersGroupDescriptor,
                                                                   newUserDesciptor).Result;
                var userIdentity = identityClient.ReadIdentitiesAsync(IdentitySearchFilter.AccountName,
                                                                 username).Result.FirstOrDefault();



                var entitlement = licensingClient.AssignEntitlementAsync(userIdentity.Id, AccountLicense.Auto).Result;


            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException.Message);
                }
            }
        }
    }
}
