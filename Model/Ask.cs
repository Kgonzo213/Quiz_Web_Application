using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Programowanie_Projekt_Web.Model;

namespace Programowanie_Projekt_Web.Model
{
    public class Ask
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Question { get; set; }
        public byte[]? Image { get; set; }
        public Answer[] Answers { get; set; }
    }

    public class Answer
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int IsCorrect { get; set; }
        public int QuestionId { get; set; }
    }
}
