using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Apprise
{
    public class AppriseSettingsValidator : AbstractValidator<AppriseSettings>
    {
        public AppriseSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).IsValidUrl();

            RuleFor(c => c.ConfigurationKey).NotEmpty()
                .When(c => c.StatelessUrls.IsNullOrWhiteSpace())
                .WithMessage("Use either Configuration Key or Stateless Urls");

            RuleFor(c => c.ConfigurationKey).Matches("^[a-z0-9-]*$")
                .WithMessage("Allowed characters a-z, 0-9 and -");

            RuleFor(c => c.StatelessUrls).NotEmpty()
                .When(c => c.ConfigurationKey.IsNullOrWhiteSpace())
                .WithMessage("Use either Configuration Key or Stateless Urls");

            RuleFor(c => c.StatelessUrls).Empty()
                .When(c => c.ConfigurationKey.IsNotNullOrWhiteSpace())
                .WithMessage("Use either Configuration Key or Stateless Urls");

            RuleFor(c => c.Tags).Empty()
                .When(c => c.StatelessUrls.IsNotNullOrWhiteSpace())
                .WithMessage("Stateless Urls do not support tags");
        }
    }

    public class AppriseSettings : IProviderConfig
    {
        private static readonly AppriseSettingsValidator Validator = new ();

        public AppriseSettings()
        {
            Tags = Array.Empty<string>();
        }

        [FieldDefinition(1, Label = "Apprise Base URL", Type = FieldType.Url, Placeholder = "http://localhost:8000", HelpText = "Apprise server Base URL, including http(s):// and port if needed", HelpLink = "https://github.com/caronc/apprise-api")]
        public string BaseUrl { get; set; }

        [FieldDefinition(2, Label = "Apprise Configuration Key", Type = FieldType.Textbox, HelpText = "Configuration Key for the Persistent Storage Solution. Leave empty if Stateless Urls is used.", HelpLink = "https://github.com/caronc/apprise-api#persistent-storage-solution")]
        public string ConfigurationKey { get; set; }

        [FieldDefinition(3, Label = "Apprise Stateless Urls", Type = FieldType.Textbox, HelpText = "One or more URLs separated by commas identifying where the notification should be sent to. Leave empty if Persistent Storage is used.", HelpLink = "https://github.com/caronc/apprise#productivity-based-notifications")]
        public string StatelessUrls { get; set; }

        [FieldDefinition(4, Label = "Apprise Tags", Type = FieldType.Tag, HelpText = "Optionally notify only those tagged accordingly.")]
        public IEnumerable<string> Tags { get; set; }

        [FieldDefinition(5, Label = "Auth Username", Type = FieldType.Textbox, HelpText = "HTTP Basic Auth Username", Privacy = PrivacyLevel.UserName)]
        public string AuthUsername { get; set; }

        [FieldDefinition(6, Label = "Auth Password", Type = FieldType.Password, HelpText = "HTTP Basic Auth Password", Privacy = PrivacyLevel.Password)]
        public string AuthPassword { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
