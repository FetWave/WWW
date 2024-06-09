﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FetWaveWWW.Services
{
    public class GoogleServices : IEmailSender
    {
        public readonly CalendarService Calendar;
        public readonly GmailService Gmail;
        private IMemoryCache cache;

        public GoogleServices(IConfiguration Configuration, IMemoryCache memoryCache)
        {
            cache= memoryCache;

            string[] Scopes = { CalendarService.Scope.Calendar, GmailService.Scope.GmailSend, GmailService.Scope.MailGoogleCom };

            ServiceAccountCredential? credential;

            using var stream = new FileStream(Configuration["Google_API_PRIVATE_KEYFILE"]!, FileMode.Open, FileAccess.Read);
            var confg = Google.Apis.Json.NewtonsoftJsonSerializer.Instance.Deserialize<JsonCredentialParameters>(stream);
            credential = GoogleCredential.FromJsonParameters(confg)
                .CreateScoped(Scopes)
                .CreateWithUser("board@tngaz.org")
                .UnderlyingCredential as ServiceAccountCredential;

            var baseClient = new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "TNG WWW Calendar Integration",
            };

            Calendar = new CalendarService(baseClient);

            Gmail = new GmailService(baseClient);
        }

        private static string GetEmailRaw(string toEmail, string subject, string body)
            => Base64UrlEncoder.Encode($"To: {toEmail}\r\nSubject: {subject}\r\nContent-Type: text/html;charset=utf-8\r\n\r\n{body}");

        private static string GetEmailBccRaw(string toEmail, string subject, string body)
            => Base64UrlEncoder.Encode($"Bcc: {toEmail}\r\nSubject: {subject}\r\nContent-Type: text/html;charset=utf-8\r\n\r\n{body}");

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await Task.FromResult(() => Gmail.Users.Messages.Send(new Message { Raw = GetEmailRaw(email, subject, htmlMessage) }, "me").Execute());
        }

        public async Task EmailListAsync(IEnumerable<string> emails, string subject, string body)
        {
            for (int i = 0; i < emails.Count(); i += 100)
            {
                var batchEmails = emails.Skip(i).Take(100);
                await Task.FromResult(() => Gmail.Users.Messages.Send(new Message { Raw = GetEmailBccRaw(string.Join(", ", batchEmails), subject, body) }, "me").Execute());
            }
            
        }

        private HashSet<string> cacheKeys = new HashSet<string>();

        public Event? GetEvent(string calendarId, string eventId)
        {
            try
            {
                var key = $"event:{calendarId}:{eventId}";
                if (cache.TryGetValue(key, out Event? cachedEvent))
                {
                    return cachedEvent;
                }
                var newEvent = Calendar.Events.Get(calendarId, eventId).Execute();
                cache.Set(key, newEvent);
                cacheKeys.Add(key);
                return newEvent;
            }
            catch { }
            return null;
        }

        public void ClearEventCache()
        {
            foreach(var key in cacheKeys)
            {
                cache.Remove(key);
            }
            cacheKeys = [];
        }
    }
}
