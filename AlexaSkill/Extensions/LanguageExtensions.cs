using Alexa.NET.LocaleSpeech;
using Alexa.NET.Request;
using Alexa.NET.Response;
using System.Threading.Tasks;

namespace AlexaSkill.Extensions
{
    public static class LanguageExtensions
    {
        public static ILocaleSpeech CreateLocale(this SkillRequest skillRequest, DictionaryLocaleSpeechStore store)
        {
            var localeSpeechFactory = new LocaleSpeechFactory(store);
            var locale = localeSpeechFactory.Create(skillRequest);

            return locale;
        }

        public static Task<IOutputSpeech> Get(this ILocaleSpeech locale, string key) => locale.Get(key, null);
    }
}
