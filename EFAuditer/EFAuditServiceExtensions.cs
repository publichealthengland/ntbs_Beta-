﻿using System;
using System.Linq;
using System.Security.Claims;
using Audit.Core;
using Audit.EntityFramework;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFAuditer
{
    public static class EFAuditServiceExtensions
    {
        public static void AddEFAuditer(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            var svcProvider = services.BuildServiceProvider();
            var contextOptions = new DbContextOptionsBuilder<AuditDatabaseContext>()
                .UseSqlServer(connectionString)
                .Options;

            Audit.Core.Configuration.AddCustomAction(ActionType.OnScopeCreated, scope =>
            {
                // Get user from http context
                // Solution follows idea from https://github.com/thepirat000/Audit.NET/issues/136#issuecomment-402532587
                var user = svcProvider.GetService<IHttpContextAccessor>().HttpContext?.User;

                if (user != null)
                {
                    var userName = user?.FindFirstValue(ClaimTypes.Email);
                    // Fallbacks if user doesn't have an email associated with them - as is the case with our test users
                    if (string.IsNullOrEmpty(userName)) userName = user.Identity.Name;
                    scope.SetCustomField(CustomFields.AppUser, userName);
                }
            });

            Action<AuditEvent, EventEntry, AuditLog> auditAction = AuditAction;

            Audit.Core.Configuration.Setup()
                .UseEntityFramework(ef => ef
                    .UseDbContext<AuditDatabaseContext>(contextOptions)
                    .AuditTypeMapper(t => typeof(AuditLog))
                    .AuditEntityAction<AuditLog>(auditAction)
                    .IgnoreMatchedProperties(true)
                );
        }

        public static void AuditAction(AuditEvent ev, Audit.EntityFramework.EventEntry entry, AuditLog audit)
        {
            audit.AuditData = entry.ToJson();
            audit.OriginalId = entry.PrimaryKey.First().Value.ToString();
            audit.EntityType = entry.EntityType.Name;
            audit.EventType = entry.Action;
            audit.AuditDetails = GetCustomKey(ev, CustomFields.AuditDetails);
            audit.AuditDateTime = DateTime.Now;
            audit.AuditUser = GetCustomKey(ev, CustomFields.AppUser) ?? ev.Environment.UserName;
            audit.RootEntity = GetCustomKey(ev, CustomFields.RootEntity);
            audit.RootId = GetCustomKey(ev, CustomFields.RootId);
        }

        private static string GetCustomKey(AuditEvent ev, string key)
        {
            if (ev.CustomFields.ContainsKey(key))
            {
                return ev.CustomFields[key]?.ToString();
            }
            
            return null;
        }
    }
}
