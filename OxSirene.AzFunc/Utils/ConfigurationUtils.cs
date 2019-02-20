using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace OxSirene.AzFunc
{
    internal static class ConfigurationUtils
    {
        public static void Initialize(ExecutionContext context, ILogger logger)
        {
            API.Configuration.CreateInstance(
                context.FunctionAppDirectory,
                "local.settings.json",
                true,
                logger
            );
        }
    } 
}