using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;

namespace ABCBot.Services
{
    public class TwitterService : ITwitterService
    {
        ITwitterCredentials twitterCredentials;

        public TwitterService(IConfigurationSection configurationSection) {
            this.twitterCredentials = new TwitterCredentials(configurationSection["ConsumerKey"], configurationSection["ConsumerSecret"], configurationSection["AccessToken"], configurationSection["AccessTokenSecret"]);
        }

        private bool CredentialsValid() {
            return (!string.IsNullOrEmpty(twitterCredentials.AccessToken) && !string.IsNullOrEmpty(twitterCredentials.AccessTokenSecret) &&
                    !string.IsNullOrEmpty(twitterCredentials.ConsumerKey) && !string.IsNullOrEmpty(twitterCredentials.ConsumerSecret));
        }

        public Task<string> GetProfileImageUrl(string handle) {
            if (!CredentialsValid()) {
                return Task.FromResult(string.Empty);
            }

            var user = Auth.ExecuteOperationWithCredentials(twitterCredentials, () => User.GetUserFromScreenName(handle));

            if (!user.DefaultProfileImage) {
                return Task.FromResult(user.ProfileImageUrlFullSize);
            } else {
                return Task.FromResult(string.Empty);
            }
        }
    }
}
