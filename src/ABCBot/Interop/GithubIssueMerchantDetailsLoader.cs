using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ABCBot.Models;
using ABCBot.Services;
using CommonMark;
using Optional;
using Optional.Unsafe;
using Serilog;

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

            var maybeYmlBlock = ExtractYmlCodeBlockFromIssueBody(issue.Body);
            if (!maybeYmlBlock.HasValue) {
                return Option.None<MerchantDetails>();
            }

            var ymlBlock = maybeYmlBlock.ValueOrFailure();

            return Option.Some(merchantDetails);
        }

        public Option<string> ExtractYmlCodeBlockFromIssueBody(string body) {
            var document = CommonMarkConverter.Parse(body);
            foreach (var node in document.AsEnumerable()) {
                if (node.IsOpening && node.Block?.Tag == CommonMark.Syntax.BlockTag.FencedCode) {
                    if (node.Block.FencedCodeData.Info == "yml") {
                        return Option.Some(node.Block.StringContent.ToString().Trim('\n'));
                    }
                }
            }

            return Option.None<string>();
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

            Log.Debug("Extracted \"{name}\" and \"{category}\" from \"{title}\"", merchantDetails.Name, merchantDetails.Category, title);

            return true;
        }
    }
}
