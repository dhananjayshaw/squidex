﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.Log.Adapter
{
    internal sealed class SemanticLogLogger : ILogger
    {
        private readonly ISemanticLog semanticLog;

        public SemanticLogLogger(ISemanticLog semanticLog)
        {
            this.semanticLog = semanticLog;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            SemanticLogLevel semanticLogLevel;

            switch (logLevel)
            {
                case LogLevel.Trace:
                    semanticLogLevel = SemanticLogLevel.Trace;
                    break;
                case LogLevel.Debug:
                    semanticLogLevel = SemanticLogLevel.Debug;
                    break;
                case LogLevel.Information:
                    semanticLogLevel = SemanticLogLevel.Information;
                    break;
                case LogLevel.Warning:
                    semanticLogLevel = SemanticLogLevel.Warning;
                    break;
                case LogLevel.Error:
                    semanticLogLevel = SemanticLogLevel.Error;
                    break;
                case LogLevel.Critical:
                    semanticLogLevel = SemanticLogLevel.Fatal;
                    break;
                default:
                    semanticLogLevel = SemanticLogLevel.Debug;
                    break;
            }

            var context = (eventId, state, exception, formatter);

            semanticLog.Log(semanticLogLevel, context, (ctx, writer) =>
            {
                var message = ctx.formatter(ctx.state, ctx.exception);

                if (!string.IsNullOrWhiteSpace(message))
                {
                    writer.WriteProperty(nameof(message), message);
                }

                if (ctx.eventId.Id > 0)
                {
                    writer.WriteObject("eventId", ctx.eventId, (innerEventId, eventIdWriter) =>
                    {
                        eventIdWriter.WriteProperty("id", innerEventId.Id);

                        if (!string.IsNullOrWhiteSpace(innerEventId.Name))
                        {
                            eventIdWriter.WriteProperty("name", innerEventId.Name);
                        }
                    });
                }

                if (ctx.state is IReadOnlyList<KeyValuePair<string, object>> parameters)
                {
                    foreach (var kvp in parameters)
                    {
                        if (kvp.Value != null)
                        {
                            var key = kvp.Key.Trim('{', '}', ' ');

                            if (key.Length > 2 && !string.Equals(key, "originalFormat", StringComparison.OrdinalIgnoreCase))
                            {
                                writer.WriteProperty(key.ToCamelCase(), kvp.Value.ToString());
                            }
                        }
                    }
                }

                if (ctx.exception != null)
                {
                    writer.WriteException(ctx.exception);
                }
            });
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NoopDisposable.Instance;
        }
    }
}
