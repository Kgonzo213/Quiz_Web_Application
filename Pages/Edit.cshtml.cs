using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Programowanie_Projekt_Web.Model;
using Programowanie_Projekt_Web.Repo;

namespace Programowanie_Projekt_Web.Pages
{
    public class EditModel : PageModel
    {
        private readonly IAskRepos _askRepos;

        public EditModel(IAskRepos askRepos)
        {
            _askRepos = askRepos;
        }

        [BindProperty]
        public Ask Ask { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var ask = await _askRepos.GetAskAsync(id);
            if (ask == null)
            {
                return NotFound();
            }

            Ask = ask;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile? questionImage, string questionText, string[] answerContents, int[] correctAnswers)
        {
            if (string.IsNullOrWhiteSpace(questionText))
            {
                ModelState.AddModelError(nameof(questionText), "Treść pytania nie może być pusta.");
                return Page();
            }

            if (answerContents == null || answerContents.Length < 2)
            {
                ModelState.AddModelError(nameof(answerContents), "Musisz podać co najmniej dwie odpowiedzi.");
                return Page();
            }

            for (int i = 0; i < answerContents.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(answerContents[i]))
                {
                    ModelState.AddModelError(nameof(answerContents) + $"[{i}]", "Odpowiedź nie może być pusta.");
                }
            }

            // Update the question
            Ask.Question = questionText;

            // Update the answers
            if (Ask.Answers == null || Ask.Answers.Length != answerContents.Length)
            {
                Ask.Answers = new Answer[answerContents.Length];
            }

            for (int i = 0; i < answerContents.Length; i++)
            {
                Ask.Answers[i] = new Answer
                {
                    Content = answerContents[i],
                    IsCorrect = correctAnswers.Contains(i) ? 1 : 0, // Mark as correct if the index is in correctAnswers
                    QuestionId = Ask.Id
                };
            }

            // Update the image if provided
            if (questionImage != null)
            {
                Ask.Image = await Utility.ImageToByteArrayAsync(questionImage);
            }

            // Save changes to the database
            var success = await _askRepos.UpdateAskAsync(Ask);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Nie udało się zaktualizować pytania.");
                return Page();
            }

            return RedirectToPage("/Questions");
        }
    }
}
