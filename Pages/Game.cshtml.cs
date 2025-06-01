using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
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

        public Ask CurrentQuestion { get; set; }

        [BindProperty]
        public List<Ask> Questions { get; set; } // Przechowujemy całą tablicę pytań

        [BindProperty]
        public int Score { get; set; } // Wynik gracza

        [BindProperty]
        public int[] SelectedAnswers { get; set; } // Wybrane odpowiedzi

        public async Task<IActionResult> OnGetAsync()
        {
            // Losowanie pytań na początku gry
            Questions = (await _askRepos.QuestionsAsync(10)).ToList();
            if (!Questions.Any())
            {
                return RedirectToPage("/Error");
            }

            Score = 0;

            // Zapisz pytania do sesji
            HttpContext.Session.SetObject("Questions", Questions);

            // Pobierz pierwsze pytanie
            CurrentQuestion = Questions[0];

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Pobierz pytania z sesji
            Questions = HttpContext.Session.GetObject<List<Ask>>("Questions");
            if (Questions == null || !Questions.Any())
            {
                return RedirectToPage("/Error");
            }

            // Pobierz aktualne pytanie
            CurrentQuestion = Questions[0];

            // Oblicz wynik
            foreach (var answer in CurrentQuestion.Answers)
            {
                if (SelectedAnswers.Contains(answer.Id) && answer.IsCorrect == 1)
                {
                    Score++;
                }
                else if (SelectedAnswers.Contains(answer.Id) && answer.IsCorrect == 0)
                {
                    Score--;
                }
            }

            // Usuń aktualne pytanie z listy
            Questions.RemoveAt(0);

            // Zaktualizuj pytania w sesji
            HttpContext.Session.SetObject("Questions", Questions);

            // Jeśli lista pytań jest pusta, przejdź do strony wyników
            if (!Questions.Any())
            {
                return RedirectToPage("/Result", new { score = Score });
            }

            // Pobierz następne pytanie
            CurrentQuestion = Questions[0];

            return Page();
        }

    }

    public static class SessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
