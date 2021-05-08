using System;
using System.ComponentModel;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CrmHelper.Extensions
{
    public enum AuthenticationType
    {
        [Description("Active Directory")]
        AD,
        [Description("Internet Facing Deploument")]
        IFD,
        [Description("OAuth")]
        OAuth,
        [Description("Office 365")]
        Office365
    }

    public enum LoginPromptMode
    {
        /// <summary>
        /// Always prompts user to specify credentials
        /// </summary>
        Always,
        /// <summary>
        /// Allows the user to select in the login control interface whether to display the prompt or not.
        /// </summary>
        Auto,
        /// <summary>
        /// Does not prompt the user to specify credentials. If using a connection method does not have a
        /// user interface, you should use this value.
        /// </summary>
        Never
    }

    public class CrmConnectionString : INotifyPropertyChanged
    {
        private const string OrganizationServiceEndpoint = "/XRMServices/2011/Organization.svx";

        private Uri _serviceUri;
        private string _username;
        private TimeSpan _timeout = TimeSpan.FromMinutes(2);
        private string _domain;
        private string _password;
        private string _appkey;
        private string _clientId;
        private string _tokenCacheStorePath;
        private LoginPromptMode _loginPrompt;
        private Uri _redirectUri;
        private AuthenticationType? _authenticationType;
        private Uri _homeRealmUri;
        private string _authority;
        private string _unknownOptions;

        private Uri NormalizeServiceUri(Uri serviceUri)
        {
            if (serviceUri == null) return null;
            return serviceUri.AbsoluteUri.EndsWith(OrganizationServiceEndpoint, StringComparison.InvariantCultureIgnoreCase)
                ? serviceUri
                : new Uri(serviceUri.AbsoluteUri.TrimEnd('/') + OrganizationServiceEndpoint, UriKind.Absolute);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Uri ServiceUri
        {
            get => _serviceUri;
            set => _serviceUri = NormalizeServiceUri(value);
        }

        public TimeSpan Timeout
        {
            get => _timeout;
            set
            {
                if (value.Equals(_timeout)) return;
                _timeout = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if (value.Equals(_username)) return;
                _username = value;
                OnPropertyChanged();
            }
        }

        public string Domain
        {
            get => _domain;
            set
            {
                if (value.Equals(_domain)) return;
                _domain = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (value.Equals(_password)) return;
                _password = value;
                OnPropertyChanged();
            }
        }

        public string AppKey
        {
            get => _appkey;
            set
            {
                if (value.Equals(_appkey)) return;
                _appkey = value;
                OnPropertyChanged();
            }
        }

        public string ClientId
        {
            get => _clientId;
            set
            {
                if (value.Equals(_clientId)) return;
                _clientId = value;
                OnPropertyChanged();
            }
        }

        public LoginPromptMode LoginPrompt
        {
            get => _loginPrompt;
            set
            {
                if (value.Equals(_loginPrompt)) return;
                _loginPrompt = value;
                OnPropertyChanged();
            }
        }

        public string TokenCacheStorePath
        {
            get => _tokenCacheStorePath;
            set
            {
                if (value.Equals(_tokenCacheStorePath)) return;
                _tokenCacheStorePath = value;
                OnPropertyChanged();
            }
        }

        public Uri RedirectUri
        {
            get => _redirectUri;
            set
            {
                if (value.Equals(_redirectUri)) return;
                _redirectUri = value;
                OnPropertyChanged();
            }
        }

        public AuthenticationType? AuthenticationType
        {
            get => _authenticationType;
            set
            {
                if (value.Equals(_authenticationType)) return;
                _authenticationType = value;
                OnPropertyChanged();
            }
        }

        public Uri HomeRealmUri
        {
            get => _homeRealmUri;
            set
            {
                if (value.Equals(_homeRealmUri)) return;
                _homeRealmUri = value;
                OnPropertyChanged();
            }
        }

        public string Authority
        {
            get => _authority;
            set
            {
                if (value.Equals(_authority)) return;
                _authority = value;
                OnPropertyChanged();
            }
        }

        public string UnknownOptions
        {
            get => _unknownOptions;
            set
            {
                if (value == null) return;
                _unknownOptions = value;
                OnPropertyChanged();
            }
        }

        public Uri GetBaseUri()
        {
            if (ServiceUri == null) return null;
            var url = ServiceUri.AbsoluteUri.Substring(0, ServiceUri.AbsoluteUri.Length - OrganizationServiceEndpoint.Length)
                                            .TrimEnd('/');
            return new Uri(url, UriKind.Absolute);
        }

        public Uri GetDiscoveryServiceUri()
        {
            if (ServiceUri == null) return null;

            var baseUri = GetBaseUri();
            var onlineCrm = Regex.Match(baseUri.Host, @"\.(?<region>[^.]+\dynamics\.com)$");
            if (onlineCrm.Success)
            {
                var region = onlineCrm.Groups["region"].Value;
                return new Uri($"https://disco.{region}.dynamics.com/XRMServices/2011/Discovery.svc", UriKind.Absolute);
            }
            else
            {
                return new Uri($"{baseUri.AbsoluteUri.TrimEnd('/')}/XRMServices/2011/Discovery.svc", UriKind.Absolute);
            }
        }

        public static CrmConnectionString Deserialize(string connectionString)
        {
            var builder = new DbConnectionStringBuilder();
            builder.ConnectionString = connectionString;

            return new CrmConnectionString
            {
                ServiceUri = ExtractUri("ServiceUri", "Service Uri", "Url", "Server"),
                HomeRealmUri = ExtractUri("HomeReadlmUri", "Home Realm Uri"),
                RedirectUri = ExtractUri("RedirectUri", "ReplyUrl"),

                Domain = Extract("Domain"),
                Username = Extract("UserName", "User Name", "UserId", "User Id"),
                Password = Extract("Password"),
                Authority = Extract("Authority"),
                ClientId = Extract("ClientId", "AppId", "ApplicationId"),
                TokenCacheStorePath = Extract("TokenCacheStorePath"),
                AppKey = Extract("AppKey"),

                AuthenticationType = ExtractEnum<AuthenticationType>("AuthType", "AuthenticationType", "Authentication Type"),
                LoginPrompt = ExtractEnum<LoginPromptMode>("LoginPrompt") ?? LoginPromptMode.Auto,

                Timeout = ExtractTimeSpan("Timeout") ?? TimeSpan.FromMinutes(2),

                UnknownOptions = builder.ConnectionString,
            };

            string Extract(params string[] keys)
            {
                var value = string.Empty;
                foreach (var key in keys)
                {
                    if (string.IsNullOrEmpty(value) && builder.TryGetValue(key, out var v))
                    {
                        value = v as string;
                    }
                    builder.Remove(key);
                }
                return value;
            }

            Uri ExtractUri(params string[] keys)
            {
                var value = Extract(keys);

                if (value == null) return null;
                return Uri.TryCreate(value, UriKind.Absolute, out var uri)
                    ? uri
                    : null;
            }

            TimeSpan? ExtractTimeSpan(params string[] keys)
            {
                var value = Extract(keys);

                if (value == null) return null;
                return TimeSpan.TryParse(value, out var timespan)
                    ? timespan
                    : (TimeSpan?)null;
            }

            T? ExtractEnum<T>(params string[] keys)
                where T : struct
            {
                var value = Extract(keys);
                if (value == null) return null;
                return Enum.TryParse<T>(value, ignoreCase: true, out var timespan)
                    ? timespan
                    : (T?)null;
            }
        }

        public static string Serialize(CrmConnectionString connectionString)
        {
            if (connectionString == null) return "";

            var builder = new DbConnectionStringBuilder();
            SetValue("Service Uri", connectionString.ServiceUri);
            SetValue("Domain", connectionString.Domain);
            SetValue("User Name", connectionString.Username);
            SetValue("Password", connectionString.Password);
            SetValue("Home Realm Uri", connectionString.HomeRealmUri);
            SetValue("Authentication Type", connectionString.AuthenticationType);
            SetValue("ClientId", connectionString.ClientId);
            SetValue("AppKey", connectionString.AppKey);
            SetValue("RedirectUri", connectionString.RedirectUri);
            SetValue("Authority", connectionString.Authority);
            SetValue("TokenCacheStorePath", connectionString.TokenCacheStorePath);

            if (connectionString.AuthenticationType == Extensions.AuthenticationType.OAuth)
            {
                SetValue("LoginPrompt", connectionString.LoginPrompt, LoginPromptMode.Auto);
            }

            SetValue("Timeout", connectionString.Timeout, TimeSpan.FromMinutes(2));

            return $"{builder.ConnectionString};{connectionString.UnknownOptions}";

            void SetValue<T>(string name, T value, T ignore = default)
            {
                if (Equals(value, ignore)) return;
                builder[name] = value?.ToString();
            }
        }
    }
}
