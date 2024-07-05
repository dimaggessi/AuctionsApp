using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
	public static IEnumerable<IdentityResource> IdentityResources =>
		new IdentityResource[]
		{
			new IdentityResources.OpenId(),
			new IdentityResources.Profile(),
		};

	public static IEnumerable<ApiScope> ApiScopes =>
		new ApiScope[]
		{
			new ApiScope("auctionApp", "Auction app full access")
		};

	public static IEnumerable<Client> Clients =>
		new Client[]
		{
			// configuration just for a development
			new Client 
			{
				ClientId = "postman",
				ClientName = "Postman",
				AllowedScopes = {"openid", "profile", "auctionApp"},
				RedirectUris = {"https://www.getpostman.com/oauth2/callback"},
				ClientSecrets = new[] {new Secret("NotASecret".Sha256())},
				AllowedGrantTypes = {GrantType.ResourceOwnerPassword}
			},
			new Client 
			{
				// the name nextApp is used only for a browser based web application
				ClientId = "nextApp",
				ClientName = "nextApp",
				ClientSecrets = {new Secret("secret".Sha256())},
				// do not send the secret to client's browser
				// it's happening behind the scenes (access token returned from identity server)
				AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
				// if using a mobile app, it's not possible store a secret inside a Reactive Native app
				RequirePkce = false, // it's not needed for this case
				RedirectUris = {"http://localhost:3000/api/auth/callback/id-server"},
				AllowOfflineAccess = true,
				// specify the scopes that's client is allowed to access
				AllowedScopes = {"openid", "profile", "auctionApp"},
				AccessTokenLifetime = 3600*24*30 // (one month lifetime) only for developing purpose
			}
		};
}
