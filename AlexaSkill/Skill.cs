using Alexa.NET;
using Alexa.NET.LocaleSpeech;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using AlexaSkill.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlexaSkill
{
    public static class Skill
    {
        [FunctionName("SkillName")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {
            var json = await req.ReadAsStringAsync();
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);

            // Verifies that the request is indeed coming from Alexa.
            var isValid = await skillRequest.ValidateRequestAsync(req, log);
            if (!isValid)
            {
                return new BadRequestResult();
            }

            // Setup language resources.
            var store = SetupLanguageResources();
            var locale = skillRequest.CreateLocale(store);

            var request = skillRequest.Request;
            SkillResponse response = null;

            try
            {
                if (request is LaunchRequest launchRequest)
                {
                    log.LogInformation("Session started");

                    var welcomeMessage = await locale.Get(LanguageKeys.Welcome, null);
                    var welcomeRepromptMessage = await locale.Get(LanguageKeys.WelcomeReprompt, null);
                    response = ResponseBuilder.Ask(welcomeMessage, RepromptBuilder.Create(welcomeRepromptMessage));
                }
                else if (request is IntentRequest intentRequest)
                {
                    // Checks whether to handle system messages defined by Amazon.
                    var systemIntentResponse = await HandleSystemIntentsAsync(intentRequest, locale);
                    if (systemIntentResponse.IsHandled)
                    {
                        response = systemIntentResponse.Response;
                    }
                    else
                    {
                        // Processes request according to intentRequest.Intent.Name...
                        var message = await locale.Get(LanguageKeys.Response, null);
                        response = ResponseBuilder.Tell(message);

                        // Note: The ResponseBuilder.Tell method automatically sets the
                        // Response.ShouldEndSession property to true, so the session will be
                        // automatically closed at the end of the response.
                    }
                }
                else if (request is SessionEndedRequest sessionEndedRequest)
                {
                    log.LogInformation("Session ended");
                    response = ResponseBuilder.Empty();
                }
            }
            catch
            {
                var message = await locale.Get(LanguageKeys.Error, null);
                response = ResponseBuilder.Tell(message);
                response.Response.ShouldEndSession = false;
            }

            return new OkObjectResult(response);
        }

        private static async Task<(bool IsHandled, SkillResponse Response)> HandleSystemIntentsAsync(IntentRequest request, ILocaleSpeech locale)
        {
            SkillResponse response = null;

            if (request.Intent.Name == IntentNames.Cancel)
            {
                var message = await locale.Get(LanguageKeys.Cancel, null);
                response = ResponseBuilder.Tell(message);
            }
            else if (request.Intent.Name == IntentNames.Help)
            {
                var message = await locale.Get(LanguageKeys.Help, null);
                response = ResponseBuilder.Ask(message, RepromptBuilder.Create(message));
            }
            else if (request.Intent.Name == IntentNames.Stop)
            {
                var message = await locale.Get(LanguageKeys.Stop, null);
                response = ResponseBuilder.Tell(message);
            }

            return (response != null, response);
        }

        private static DictionaryLocaleSpeechStore SetupLanguageResources()
        {
            // Creates the locale speech store for each supported languages.
            var store = new DictionaryLocaleSpeechStore();

            store.AddLanguage("en", new Dictionary<string, object>
            {
                [LanguageKeys.Welcome] = "Welcome to the skill!",
                [LanguageKeys.WelcomeReprompt] = "You can ask help if you need instructions on how to interact with the skill",
                [LanguageKeys.Response] = "This is just a sample answer",
                [LanguageKeys.Cancel] = "Canceling...",
                [LanguageKeys.Help] = "Help...",
                [LanguageKeys.Stop] = "Bye bye!",
                [LanguageKeys.Error] = "I'm sorry, there was an unexpected error. Please, try again later."
            });

            store.AddLanguage("it", new Dictionary<string, object>
            {
                [LanguageKeys.Welcome] = "Benvenuto nella skill!",
                [LanguageKeys.WelcomeReprompt] = "Se vuoi informazioni sulle mie funzionalità, prova a chiedermi aiuto",
                [LanguageKeys.Response] = "Questa è solo una risposta di prova",
                [LanguageKeys.Cancel] = "Sto annullando...",
                [LanguageKeys.Help] = "Aiuto...",
                [LanguageKeys.Stop] = "A presto!",
                [LanguageKeys.Error] = "Mi dispiace, si è verificato un errore imprevisto. Per favore, riprova di nuovo in seguito."
            });

            return store;
        }
    }
}
