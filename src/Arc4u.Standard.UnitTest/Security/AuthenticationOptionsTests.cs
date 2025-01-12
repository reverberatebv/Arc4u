using Arc4u.Configuration;
using Arc4u.OAuth2;
using Arc4u.OAuth2.Extensions;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Arc4u.UnitTest.Security;

[Trait("Category", "CI")]
public class AuthenticationOptionsTests
{
    public AuthenticationOptionsTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void Oauth2_Key_Values_With_Authority_Should()
    {
        var options = _fixture.Create<OAuth2SettingsOption>();
        var authority = _fixture.Build<AuthorityOptions>().With(p => p.Url, _fixture.Create<Uri>()).Create();

        var configDic = new Dictionary<string, string?>
        {
            ["OAuth2.Settings:Authority:Url"] = authority.Url.ToString(),
        };
        foreach (var audience in options.Audiences)
        {
            configDic.Add($"OAuth2.Settings:Audiences:{options.Audiences.IndexOf(audience)}", audience);
        }
        foreach (var scope in options.Scopes)
        {
            configDic.Add($"OAuth2.Settings:Scopes:{options.Scopes.IndexOf(scope)}", scope);
        }
        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(configDic).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.ConfigureOAuth2Settings(configuration, "OAuth2.Settings");

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get(Constants.OAuth2OptionsName);

        sut.Should().NotBeNull();
        sut.Values[TokenKeys.Audiences].Should().Be(string.Join(' ', options.Audiences));
        sut.Values[TokenKeys.AuthorityKey].Should().Be(Constants.OAuth2OptionsName);
        sut.Values[TokenKeys.Scope].Should().Be(string.Join(' ', options.Scopes));

        var sutAuthority = serviceProvider.GetService<IOptionsMonitor<AuthorityOptions>>()!.Get(Constants.OAuth2OptionsName);

        sutAuthority.Url.Should().NotBeNull();
        sutAuthority.Url.Should().Be(authority.Url);

    }

    [Fact]
    public void Oauth2_Key_Values_Without_Authority_Should()
    {
        var options = _fixture.Create<OAuth2SettingsOption>();

        var configDic = new Dictionary<string, string?>();
        foreach (var audience in options.Audiences)
        {
            configDic.Add($"OAuth2.Settings:Audiences:{options.Audiences.IndexOf(audience)}", audience);
        }
        foreach (var scope in options.Scopes)
        {
            configDic.Add($"OAuth2.Settings:Scopes:{options.Scopes.IndexOf(scope)}", scope);
        }
        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(configDic).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.ConfigureOAuth2Settings(configuration, "OAuth2.Settings");

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get(Constants.OAuth2OptionsName);

        sut.Should().NotBeNull();
        sut.Values[TokenKeys.Audiences].Should().Be(string.Join(' ', options.Audiences));
        sut.Values.ContainsKey(TokenKeys.AuthorityKey).Should().BeFalse();
        sut.Values[TokenKeys.Scope].Should().Be(string.Join(' ', options.Scopes));

        var sutAuthority = serviceProvider.GetService<IOptionsMonitor<AuthorityOptions>>()!.Get(Constants.OAuth2OptionsName);

        sutAuthority.Url.Should().BeNull();

    }

    [Fact]
    public void Test_Oauth2_With_No_Scopes_Key_Values_Should()
    {
        var options = _fixture.Create<OAuth2SettingsOption>();

        var configDic = new Dictionary<string, string?>();
        foreach (var audience in options.Audiences)
        {
            configDic.Add($"OAuth2.Settings:Audiences:{options.Audiences.IndexOf(audience)}", audience);
        }

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(configDic).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.ConfigureOAuth2Settings(configuration, "OAuth2.Settings");

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get("OAuth2");

        sut.Should().NotBeNull();
        sut.Values[TokenKeys.Audiences].Should().Be(string.Join(' ', options.Audiences));
        sut.Values.Should().NotContainKey(TokenKeys.Scopes);
    }

    [Fact]
    public void Test_Oauth2_With_No_Audiences_To_Validate_Key_Values_Should()
    {
        var options = _fixture.Create<OAuth2SettingsOption>();

        var configDic = new Dictionary<string, string?>
        {
            { $"OAuth2.Settings:ValidateAudience", true.ToString() }
        };
        foreach (var audience in options.Audiences)
        {
            configDic.Add($"OAuth2.Settings:Audiences:{options.Audiences.IndexOf(audience)}", audience);
        }
        foreach (var scope in options.Scopes)
        {
            configDic.Add($"OAuth2.Settings:Scopes:{options.Scopes.IndexOf(scope)}", scope);
        }

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(configDic).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.ConfigureOAuth2Settings(configuration, "OAuth2.Settings");

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get("OAuth2");

        sut.Should().NotBeNull();
        sut.Values[TokenKeys.Audiences].Should().Be(string.Join(' ', options.Audiences));
        sut.Values.Should().NotContainKey(TokenKeys.Scopes);
        sut.Values.Should().ContainKey(TokenKeys.Scope);
        sut.Values[TokenKeys.Scope].Should().Be(string.Join(' ', options.Scopes));

    }

    [Fact]
    public void Test_OpenID_With_No_ValidateAudience_Key_Values_Should()
    {
        var options = _fixture.Create<OpenIdSettingsOption>();

        var configDic = new Dictionary<string, string?>
        {
            { $"OpenId.Settings:ClientId", options.ClientId },
            { $"OpenId.Settings:ClientSecret", options.ClientSecret }
        };
        foreach (var audience in options.Audiences)
        {
            configDic.Add($"OpenId.Settings:Audiences:{options.Audiences.IndexOf(audience)}", audience);
        }
        foreach (var scope in options.Scopes)
        {
            configDic.Add($"OpenId.Settings:Scopes:{options.Scopes.IndexOf(scope)}", scope);
        }

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(configDic).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.ConfigureOpenIdSettings(configuration, "OpenId.Settings");

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get(Constants.OpenIdOptionsName);

        sut.Should().NotBeNull();
        sut.Values[TokenKeys.Audiences].Should().Be(string.Join(' ', options.Audiences));
        sut.Values.Should().NotContainKey(TokenKeys.Scopes);
        sut.Values[TokenKeys.Scope].Should().Be(string.Join(' ', options.Scopes));
    }

    [Fact]
    public void Test_OpenID_With_ValidateAudience_Key_Is_True_Should()
    {
        var options = _fixture.Create<OpenIdSettingsOption>();

        var configDic = new Dictionary<string, string?>
        {
            { $"OpenId.Settings:ClientId", options.ClientId },
            { $"OpenId.Settings:ClientSecret", options.ClientSecret },
            { $"OpenId.Settings:ValidateAudience", true.ToString() }
        };
        foreach (var audience in options.Audiences)
        {
            configDic.Add($"OpenId.Settings:Audiences:{options.Audiences.IndexOf(audience)}", audience);
        }
        foreach (var scope in options.Scopes)
        {
            configDic.Add($"OpenId.Settings:Scopes:{options.Scopes.IndexOf(scope)}", scope);
        }

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(configDic).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.ConfigureOpenIdSettings(configuration, "OpenId.Settings");

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get(Constants.OpenIdOptionsName);

        sut.Should().NotBeNull();
        sut.Values[TokenKeys.Audiences].Should().Be(string.Join(' ', options.Audiences));
        sut.Values.Should().NotContainKey(TokenKeys.Scopes);
        sut.Values[TokenKeys.Scope].Should().Be(string.Join(' ', options.Scopes));
    }

    [Fact]
    public void Test_OpenID_With_ValidateAudience_Key_Is_False_Should()
    {
        var options = _fixture.Create<OpenIdSettingsOption>();

        var configDic = new Dictionary<string, string?>
        {
            { $"OpenId.Settings:ClientId", options.ClientId },
            { $"OpenId.Settings:ClientSecret", options.ClientSecret },
            { $"OpenId.Settings:ValidateAudience", false.ToString() }
        };

        foreach (var scope in options.Scopes)
        {
            configDic.Add($"OpenId.Settings:Scopes:{options.Scopes.IndexOf(scope)}", scope);
        }

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(configDic).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.ConfigureOpenIdSettings(configuration, "OpenId.Settings");

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get(Constants.OpenIdOptionsName);

        sut.Should().NotBeNull();
        sut.Values.Should().NotContainKey(TokenKeys.Audiences);
        sut.Values.Should().NotContainKey(TokenKeys.Scopes);
        sut.Values.Should().ContainKey(TokenKeys.Scope);
        sut.Values[TokenKeys.Scope].Should().Be(string.Join(' ', options.Scopes));
    }
}
