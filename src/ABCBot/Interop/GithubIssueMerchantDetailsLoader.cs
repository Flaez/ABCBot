using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ABCBot.Models;
using ABCBot.Services;
using Optional;

namespace ABCBot.Interop
{
    public class GithubIssueMerchantDetailsLoader : IMerchantDetailsLoader
    {
        IGitHubService githubService;

        public GithubIssueMerchantDetailsLoader(IGitHubService githubService) {
            this.githubService = githubService;
        }

        public async Task<Option<MerchantDetails>> ExtractDetails(int identifier) {
            var issue = await githubService.GetIssue(identifier);

            var merchantDetails = new MerchantDetails();

            var titleResults = TryExtractDetailsFromTitle(issue.Title, merchantDetails);
            if (!titleResults) {
                return Option.None<MerchantDetails>();
            }

            return Option.Some(merchantDetails);
        }

        public bool TryExtractDetailsFromTitle(string title, MerchantDetails merchantDetails) {
            // RegEx pattern to match strings inside quotes (including escaped quotes) taken from
            // https://stackoverflow.com/a/171499
            var regex = new Regex(@"([""'])(?:(?=(\\?))\2.)*?\1", RegexOptions.IgnoreCase);

            var matches = regex.Matches(title);

            if (matches.Count != 2) {
                // Ensure there are only two matches - otherwise the title is in an unknown format
                return false;
            }

            // Ensure both matches were successful
            if (!matches[0].Success && !matches[1].Success) {
                return false;
            }

            merchantDetails.Name = matches[0].Value.Trim('\'');
            merchantDetails.Category = matches[1].Value.Trim('\'');

            return true;
        }
    }
}
