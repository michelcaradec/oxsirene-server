using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;

namespace OxSirene.API
{
    public sealed class Configuration
    {
        private IConfiguration _configuration;
        private ILogger _logger;

        public string this[string key] => _configuration?[key];

        /// <summary>
        /// Save and Load intermediate results on local file system.
        /// </summary>
        public bool UseLocalCache =>
            "true".Equals(this["api:local_cache:activate"], StringComparison.InvariantCultureIgnoreCase);
        public string LocaCacheRoot => this["api:local_cache:root"] ?? string.Empty;

        public const int ConnectionLeaseTimeoutDefault = 600;
        private TimeSpan _connectionLeaseTimeout;
        public TimeSpan ConnectionLeaseTimeout { get => _connectionLeaseTimeout; set => _connectionLeaseTimeout = value; }

        private string _requestHeaderFrom;
        public string RequestHeaderFrom { get => _requestHeaderFrom; set => _requestHeaderFrom = value; }     

        /// <summary>
        /// API public access token.
        /// </summary>
        /// <remarks>
        /// Not stored in a variable so that each request will get it from configuration store.
        /// </remarks>
        public string APIAccessToken => this["api:token"];

        private Configuration(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _connectionLeaseTimeout
                = TimeSpan.FromSeconds(int.Parse(this["api:http:request:connectionLeaseTimeout"] ?? ConnectionLeaseTimeoutDefault.ToString()));
            _requestHeaderFrom = this["api:http:request:from"];
            _logger = logger;
        }

        private static object _lock = new object();
        private static Configuration _instance;
        public static Configuration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            CreateInstance(
                                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                "config.json",
                                false,
                                null
                            );
                        }
                    }
                }

                return _instance;
            }

            private set
            {
                lock (_lock)
                {
                    _instance = value;
                }
            }
        }

        public static void CreateInstance(
            string basePath,
            string configFile,
            bool optional,
            ILogger logger
        )
        {
            var configuration
                = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                .AddJsonFile(configFile, optional: optional, reloadOnChange: true)
                .AddUserSecrets("8ba3f169-8e86-4c35-afed-354708c2a5a9") // Same value as UserSecretsId in .csproj.
                .AddEnvironmentVariables()
                .Build();

            Instance = new Configuration(configuration, logger);
        }

        #region Log

        public void LogDebug(string message, params object[] args) => _logger?.LogDebug(message, args);

        public void LogTrace(string message, params object[] args) => _logger?.LogTrace(message, args);

        public void LogInformation(string message, params object[] args) => _logger?.LogInformation(message, args);

        public void LogWarning(string message, params object[] args) => _logger?.LogWarning(message, args);

        public void LogError(string message, params object[] args) => _logger?.LogError(message, args);

        public void LogCritical(string message, params object[] args) => _logger?.LogCritical(message, args);

        #endregion  
    }
}