using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Programowanie_Projekt_Web.Model;
using Programowanie_Projekt_Web.Repo;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Programowanie_Projekt_Web.Pages
{
    public class GameModel : PageModel
    {
        private readonly IAskRepos _askRepos;

        public GameModel(IAskRepos askRepos)
        {
            _askRepos = askRepos;
        }

        public List<Ask> Questions { get; set; }
        public int CurrentQuestionIndex { get; set; }
        public int Score { get; set; }

        [BindProperty]
        public int[] SelectedAnswers { get; set; }

        public async Task<IActionResult> OnGetAsync(int questionIndex = 0, int score = 0)
        {
            // Losowanie pytań tylko na początku gry
            if (questionIndex == 0)
            {
                Questions = (await _askRepos.QuestionsAsync(10)).ToList();
                if (!Questions.Any())
                {
                    return RedirectToPage("/Error");
                }

                TempData["Questions"] = Questions;
            }
            else
            {
                Questions = TempData["Questions"] as List<Ask>;
                TempData.Keep("Questions");
            }

            if (questionIndex >= Questions.Count)
            {
                return RedirectToPage("/Result", new { score });
            }

            CurrentQuestionIndex = questionIndex;
            Score = score;
            return Page();
        }

        public IActionResult OnPost(int questionIndex, int score)
        {
            Questions = TempData["Questions"] as List<Ask>;
            TempData.Keep("Questions");

            var currentQuestion = Questions[questionIndex];

            // Obliczanie wyniku
            foreach (var answer in currentQuestion.Answers)
            {
                if (SelectedAnswers.Contains(answer.Id) && answer.IsCorrect == 1)
                {
                    score++;
                }
                else if (SelectedAnswers.Contains(answer.Id) && answer.IsCorrect == 0)
                {
                    score--;
                }
            }

            questionIndex++;

            // Jeśli to było ostatnie pytanie, przejdź do strony wyników
            if (questionIndex >= Questions.Count)
            {
                return RedirectToPage("/Result", new { score });
            }

            return RedirectToPage("/Game", new { questionIndex, score });
        }
    }
}
