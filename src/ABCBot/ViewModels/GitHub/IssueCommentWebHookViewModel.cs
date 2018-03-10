using System;
using System.Collections.Generic;
using System.Text;

namespace ABCBot.ViewModels.GitHub
{
    public class IssueCommentWebHookViewModel
    {
        public string Action { get; set; }
        public IssueViewModel Issue { get; set; }
        public IssueCommentViewModel Comment { get; set; }
    }
}
