using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Services
{
    public interface ITwitterService
    {
        Task<string> GetProfileImageUrl(string handle);
    }
}
