using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KomitWap
{
    public static class LoginFunction
    {
        private static HttpClient httpClient = new HttpClient{
            BaseAddress = new Uri("https://brokerqamdm.liverpool.com.mx/wbi/wbi_personService")
        };

        [FunctionName("LoginFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {
            // Process request
            User user = new User();
            log.LogInformation("Start Komit Login function processed a request.");
            
            try
            {
                string requestBody = (await new StreamReader(req.Body).ReadToEndAsync()).Replace("\'", String.Empty);

                var options = new JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true,
                };
                
                user = JsonSerializer.Deserialize<User>(requestBody, options);

                //TODO: Aqui se hara la parte de la llamada para obtener el apellido meterno

                // Request for Liverpool Person Sercice
                log.LogInformation("Komit Login -> User try request: {0}.", user.UserId);

                var xmlDocument = new XmlDocument();
                var soapRequest = GetXmlStringRequest(user);


                var request = new HttpRequestMessage() {
                    Method = HttpMethod.Post,
                    Content = new StringContent(soapRequest, Encoding.UTF8, "text/xml")
                };

                request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode) {
                    log.LogCritical("Komit Login -> User Request: {0}. Response {1}.", user.UserId, response.StatusCode);
                    return new BadRequestObjectResult($"User, Id: {user.UserId} processed unsuccessfully.");
                }
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is TaskCanceledException)
                {
                    log.LogCritical("Komit Login -> User Request: {0}. Task Canceled Request Exception: {1}", user.UserId, ex.Message);
                    return new BadRequestObjectResult($"User, Id: {user.UserId} processed unsuccessfully.");
                }
                else if (ex.InnerException is HttpRequestException)
                {
                    log.LogCritical("Komit Login -> User Request: {0}. Http Request Exception: {1}", user.UserId, ex.Message);
                    return new BadRequestObjectResult($"User, Id: {user.UserId} processed unsuccessfully.");
                }
                log.LogCritical("Komit Login -> User Request: {0}. Aggregate Exception: {1}", user.UserId, ex.Message);
            }
            catch (Exception ex)
            {
                log.LogCritical("Komit Login -> User Request: {0}. Exception: {1}", user.UserId, ex.Message);
                return new UnprocessableEntityObjectResult($"User, Id: {user.UserId} processed unsuccessfully.");
            }

            log.LogInformation("Komit Login -> User request: {0} processed successfully.", user.UserId);

            return (ActionResult)new OkObjectResult($"User, Id: {user.UserId} processed successfully.");
        }

        internal static String GetXmlStringRequest(User user) {

            const string soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            const string mdm = "http://www.liverpool.com.mx/wbi/MDM_personService";

            user.Gender = user.Gender == "male" ? "M" : "F";
            user.BirthDate = user.BirthDate != null ? user.BirthDate.Replace("-","/") : String.Empty;

            return 
                $"<soapenv:Envelope xmlns:soapenv=\"{soapenv}\" xmlns:mdm=\"{mdm}\">" +
                    "<soapenv:Header/>" +
                    "<soapenv:Body>" +
                        "<mdm:CreateContactReq SystemNumber=\"SWIFI\">" +
                            $"<ExternalId>{user.UserId ?? String.Empty}</ExternalId>" +
                            "<Contact>" +
                                $"<FirstName>{user.FirstName ?? String.Empty}</FirstName>" +
                                $"<LastName>{user.LastName ?? String.Empty}</LastName>" +
                                "<MiddleName></MiddleName>" +
                                $"<MF>{user.Gender}</MF>" +
                                $"<BirthDate>{user.BirthDate}</BirthDate>" +
                                "<SocialSecurityNumber></SocialSecurityNumber>" +
                            "</Contact>" +
                            "<ListOfContact_AlternatePhone>" +
                                "<AlternatePhone>" +
                                    "<AlternatePhone></AlternatePhone>" +
                                    "<AlternatePhoneUseType>CASA</AlternatePhoneUseType>" +
                                    "<AlternateExtension></AlternateExtension>" +
                                    "<AlternateFuente>SWIFI</AlternateFuente>" +
                                "</AlternatePhone>" +
                                "<AlternatePhone>" +
                                    "<AlternatePhone></AlternatePhone>" +
                                    "<AlternatePhoneUseType>CELULAR</AlternatePhoneUseType>" +
                                    $"<AlternateExtension>{user.Phone ?? String.Empty}</AlternateExtension>" +
                                    "<AlternateFuente>SWIFI</AlternateFuente>" +
                                "</AlternatePhone>" +
                                "<AlternatePhone>" +
                                    "<AlternatePhone></AlternatePhone>" +
                                    "<AlternatePhoneUseType>TRABAJO</AlternatePhoneUseType>" +
                                    "<AlternateExtension></AlternateExtension>" +
                                    "<AlternateFuente>SWIFI</AlternateFuente>" +
                                "</AlternatePhone>" +
                            "</ListOfContact_AlternatePhone>" +
                            "<INSPersonalAddress>" +
                                "<INSPersonalApartmentNum></INSPersonalApartmentNum>" +
                                "<INSPersonalStreetAddress></INSPersonalStreetAddress>" +
                                "<INSPersonalDescColonia></INSPersonalDescColonia>" +
                                "<INSPersonalDescCity>CDMX</INSPersonalDescCity>" +
                                "<INSPersonalExterior></INSPersonalExterior>" +
                                "<INSPersonalInterior></INSPersonalInterior>" +
                                "<INSPersonalLocationType></INSPersonalLocationType>" +
                                "<INSPersonalPostalCode></INSPersonalPostalCode>" +
                                "<INSPersonalDescMun></INSPersonalDescMun>" +
                                "<INSPersonalStreetAddress2></INSPersonalStreetAddress2>" +
                                "<INSPersonalCalleY></INSPersonalCalleY>" +
                                "<INSPersonalCountry>MEXICO</INSPersonalCountry>" +
                            "</INSPersonalAddress>" +
                            "<ConstituentIdentification>" +
                                "<NationalId></NationalId>" +
                                "<NationalIdType></NationalIdType>" +
                            "</ConstituentIdentification>" +
                            "<ListOfContact_CommunicationAddress>" +
                                "<CommunicationAddress>" +
                                $"<CommunicationAddressEmail>{user.Email ?? String.Empty}</CommunicationAddressEmail>" +
                                "<CommunicationAddressFuente>SWIFI</CommunicationAddressFuente>" +
                                "<CommunicationAddressUseType>PERSONAL</CommunicationAddressUseType>" +
                                "</CommunicationAddress>" +
                            "</ListOfContact_CommunicationAddress>" +
                        "</mdm:CreateContactReq>" +
                    "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }
    }
}
