 
using System;
using Microsoft.AspNetCore.Authentication;

using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Net.Http.Headers;
using System.Security.Claims;


namespace MLFZUssdApplication
{

    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options,
                logger, encoder, clock)
        { 
        }

          SecuritySystemCheck.SecurityCheck _scSecurityCheck  = new  SecuritySystemCheck.SecurityCheck();


        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Headers.ContainsKey("Uuid-Key"))
            {
                var UuidKeyHolder = AuthenticationHeaderValue.Parse(Request.Headers["Uuid-Key"]);

                if (Request.Headers.ContainsKey("Authorization"))
                {
                    var SubcriptionKeyHolder = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);

                    var VerificationResponse = await Task.Run(() => _scSecurityCheck.SecurityCheckPoint(UuidKeyHolder.Scheme, SubcriptionKeyHolder.Parameter));

                    string _statusCode = VerificationResponse.statusCode;
                    string _notification = VerificationResponse.notification;
                  

                    if (_statusCode == "OT001")
                    {
                      
                        var _ClientClaims = new[] { new Claim(ClaimTypes.Name, _notification) };
                        var _ClientIdentity = new ClaimsIdentity(_ClientClaims, Scheme.Name);
                        var _Pricipal = new ClaimsPrincipal(_ClientIdentity);
                        var _tickets = new AuthenticationTicket(_Pricipal, Scheme.Name);

                        return AuthenticateResult.Success(_tickets);
                    }
                    else
                    {
                        return AuthenticateResult.Fail("Missing Subcription Key");
                    }
                }
                else
                {
                    return AuthenticateResult.Fail("Missing Subcription Key");
                }
            }
            else
            {
                return AuthenticateResult.Fail("Missing Uuid Key");
            }

           // return AuthenticateResult.Fail("");

        }



    }



}
