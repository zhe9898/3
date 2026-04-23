using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.NarrativeProjection;

public sealed partial class NarrativeProjectionModule : ModuleRunner<NarrativeProjectionState>
{
    private sealed class NarrativeProjectionQueries : INarrativeProjectionQueries

    {

        private readonly NarrativeProjectionState _state;


        public NarrativeProjectionQueries(NarrativeProjectionState state)

        {

            _state = state;

        }


        public NarrativeNotificationSnapshot GetRequiredNotification(NotificationId notificationId)

        {

            NarrativeNotificationState notification = _state.Notifications.Single(notification => notification.Id == notificationId);

            return Clone(notification);

        }


        public IReadOnlyList<NarrativeNotificationSnapshot> GetNotifications()

        {

            return _state.Notifications

                .OrderByDescending(static notification => notification.CreatedAt.Year)

                .ThenByDescending(static notification => notification.CreatedAt.Month)

                .ThenByDescending(static notification => notification.Id.Value)

                .Select(Clone)

                .ToArray();

        }


        private static NarrativeNotificationSnapshot Clone(NarrativeNotificationState notification)

        {

            return new NarrativeNotificationSnapshot

            {

                Id = notification.Id,

                CreatedAt = notification.CreatedAt,

                Tier = notification.Tier,

                Surface = notification.Surface,

                Title = notification.Title,

                Summary = notification.Summary,

                WhyItHappened = notification.WhyItHappened,

                WhatNext = notification.WhatNext,

                SourceModuleKey = notification.SourceModuleKey,

                IsRead = notification.IsRead,

                Traces = notification.Traces.Select(CloneTrace).ToArray(),

            };

        }


        private static NotificationTraceSnapshot CloneTrace(NarrativeTraceState trace)

        {

            return new NotificationTraceSnapshot

            {

                SourceModuleKey = trace.SourceModuleKey,

                EventType = trace.EventType,

                EventSummary = trace.EventSummary,

                DiffDescription = trace.DiffDescription,

                EntityKey = trace.EntityKey,

            };

        }

    }
}
