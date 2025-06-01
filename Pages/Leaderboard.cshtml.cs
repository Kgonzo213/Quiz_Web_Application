using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Programowanie_Projekt_Web.Repo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Programowanie_Projekt_Web.Pages
{
    public class LeaderboardModel : PageModel
    {
        private readonly IAskRepos _askRepos;

        public LeaderboardModel(IAskRepos askRepos)
        {
            _askRepos = askRepos;
        }

        public List<ScoreEntry> Leaderboard { get; set; } = new List<ScoreEntry>();

        public async Task OnGetAsync()
        {
            // Pobierz wyniki z bazy danych
            var scores = await _askRepos.GetLeaderboardAsync();
            foreach (var score in scores)
            {
                var parts = score.Split(' ');
                if (parts.Length == 3)
                {
                    Leaderboard.Add(new ScoreEntry
                    {
                        Name = parts[1],
                        Score = int.Parse(parts[0]),
                        NumberOfQuestions = int.Parse(parts[2])
                    });
                }
            }
        }
    }

    public class ScoreEntry
    {
        public string Name { get; set; }
        public int Score { get; set; }
        public int NumberOfQuestions { get; set; }
    }
}
