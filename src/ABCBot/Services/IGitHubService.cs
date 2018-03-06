using Octokit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Services
{
    public interface IGitHubService
    {
        Task<Issue> GetIssue(int id);
    }
}
