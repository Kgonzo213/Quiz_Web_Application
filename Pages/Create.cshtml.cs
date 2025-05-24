using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Programowanie_Projekt_Web.Model;
using Programowanie_Projekt_Web.Repo;

namespace Programowanie_Projekt_Web.Pages
{
    public class CreateModel : PageModel
    {
        private readonly IAskRepos _askRepos;

        public CreateModel(IAskRepos askRepos)
        {
            _askRepos = askRepos;
        }

        public async Task<IActionResult> OnPostAsync(string questionText, string answerA, string answerB, string answerC, string answerD, int[] correctAnswers, IFormFile? questionImage)
        {
            if (string.IsNullOrWhiteSpace(questionText) ||
                string.IsNullOrWhiteSpace(answerA) ||
                string.IsNullOrWhiteSpace(answerB) ||
                string.IsNullOrWhiteSpace(answerC) ||
                string.IsNullOrWhiteSpace(answerD))
            {
                ModelState.AddModelError(string.Empty, "Wszystkie pola są wymagane.");
                return Page();
            }

            Ask ask = new Ask
            {
                Question = questionText,
                Answers = new[]
                {
                    new Answer { Content = answerA, IsCorrect = correctAnswers.Contains(0) ? 1 : 0 },
                    new Answer { Content = answerB, IsCorrect = correctAnswers.Contains(1) ? 1 : 0 },
                    new Answer { Content = answerC, IsCorrect = correctAnswers.Contains(2) ? 1 : 0 },
                    new Answer { Content = answerD, IsCorrect = correctAnswers.Contains(3) ? 1 : 0 }
                }
            };

            if (questionImage != null)
            {
                ask.Image = await Utility.ImageToByteArrayAsync(questionImage);
            }

            bool success = await _askRepos.AddAskAsync(ask);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Nie udało się dodać pytania.");
                return Page();
            }

            return RedirectToPage("/Questions");
        }
    }
}
