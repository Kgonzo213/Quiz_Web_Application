using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Programowanie_Projekt_Web.Model;
using Programowanie_Projekt_Web.Repo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Programowanie_Projekt_Web.Pages
{
    public class QuestionsModel : PageModel
    {
        private readonly IAskRepos _askRepos;

        public List<Ask> Questions { get; set; }

        public QuestionsModel(IAskRepos askRepos)
        {
            _askRepos = askRepos;
        }

        public async Task OnGetAsync()
        {
            Questions = await _askRepos.ShowQuestionsAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (await _askRepos.DeleteAskAsync(id))
            {
                return RedirectToPage("Questions");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Nie udało się usunąć pytania.");
                return Page();
            }
        }
    }
}

