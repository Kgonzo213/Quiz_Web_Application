using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Programowanie_Projekt_Web.Repo;
using System.Threading.Tasks;

namespace Programowanie_Projekt_Web.Pages
{
    public class ResultModel : PageModel
    {
        private readonly IAskRepos _askRepos;

        public ResultModel(IAskRepos askRepos)
        {
            _askRepos = askRepos;
        }

        [BindProperty]
        public string UserName { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Score { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                ModelState.AddModelError(nameof(UserName), "Nazwa u¿ytkownika jest wymagana.");
                return Page();
            }

            await _askRepos.AddScoreAsync(Score, 10, UserName);
            return RedirectToPage("/Leaderboard");
        }
    }
}