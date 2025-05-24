using System.Collections.Generic;
using System.Threading.Tasks;
using Programowanie_Projekt_Web.Model;

namespace Programowanie_Projekt_Web.Repo
{
    public interface IAskRepos
    {
        Task<bool> IsAskNullAsync(Ask ask);
        Task<bool> AddAskAsync(Ask ask);
        Task<bool> DeleteAskAsync(int id);
        Task<bool> UpdateAskAsync(Ask ask);
        Task<Ask[]> QuestionsAsync(int numberOfQuestions);
        Task<bool> AddScoreAsync(int score, int numberOfQuestions, string name);
        Task<string[]> ShowScoresAsync();
        Task<List<Ask>> ShowQuestionsAsync();
        Task<Ask?> GetAskAsync(int id);
    }
}
