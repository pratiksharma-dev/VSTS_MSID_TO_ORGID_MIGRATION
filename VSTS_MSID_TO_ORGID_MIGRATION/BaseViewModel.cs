using System.Net;

namespace VSTS_MSID_TO_ORGID_MIGRATION

{
    public class BaseViewModel
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public string Message { get; set; }
    }
}
