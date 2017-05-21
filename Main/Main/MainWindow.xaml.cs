using System;
using LiteDB;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace Main
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class Request
        {
            public int Id { get; set; }
            public int AnwserId { get; set; }
            public string Text { get; set; }
            public IEnumerable<string> Words;
        }
        public class Answer
        {
            public int AnwserId { get; set; }
            public string Text { get; set; }
        }
        private string mydocpath;
        private string filePath;
        public MainWindow()
        {
            InitializeComponent();
            mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = System.IO.Path.Combine(mydocpath, "Requests-Answers.db");
           /* if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }
           */
        }
        private void ButtonStartClick(object sender, RoutedEventArgs e)
        {

            TextBlockAnswer.Text= GetAnswer(TextBoxRequest.Text );
        }
        private IEnumerable<string> GetWords(string input)
        {
            string space=@" ";

            string[] S = Regex.Split(input, space);
            var StrArray = S.Where(x => !string.IsNullOrWhiteSpace(x));
            foreach (string element in StrArray)
            {
             //   if (element != "")
                {
                    TextBlockAnswer.Text +=  element+' ';
                }
            }
            return StrArray;
        }
        public string GetAnswer(string S)
        {
            string AnswerText;
            int IdAnswer;
            bool Done=false;
            IEnumerable<string> RequestWords;
            RequestWords = GetWords(S);
            filePath = System.IO.Path.Combine(mydocpath, "Requests-Answers.db");
            using (var db = new LiteDatabase(filePath))
            {
                var requests = db.GetCollection<Request>("requests");
                var answers = db.GetCollection<Answer>("answers");

                if (requests.Find(x => x.Text.Equals(S)).Count() != 0)
                {
                    var request = requests.Find(x => x.Text.Equals(S));
                    IdAnswer = request.ElementAt(0).AnwserId;
                    if (IdAnswer != -1)
                    {
                        AnswerText = ((answers.Find(x => x.Equals(IdAnswer))).ElementAt(0)).Text;
                        Done = true;
                    }
                    else 
                    {
                        AnswerText = "Вопрос в очереди на рассмотрение";
                    }
                    
                }
                else
                {
                    int[] Ar = new int[requests.FindAll().Count()];
                    int i = 0;

                    for (i = 0; i < requests.FindAll().Count();i++ )
                    {
                        Ar[i] = 0;
                    }
                    i = 0;
                    int wordsCount = RequestWords.Count();
                    foreach (var req in requests.FindAll())
                    {
                        foreach (string word in RequestWords)
                        { 
                            foreach(string wor in req.Words)
                            {
                                if (word.Equals(wor))
                                {
                                    Ar[i] += 1;
                                }
                            }
                        }
                            i++;
                    }
                    int Max=0, Maxi=-1;
                    for (i = 0; i < requests.FindAll().Count(); i++)
                    {
                        if (Ar[i] > 0)
                        {
                            Maxi = i; Max = Ar[i];
                        }
                    }
                    if (Max == 0)
                    {
                        AnswerText = "Ответ не найден";
                        var request = new Request { AnwserId = -1, Text = S, Words = RequestWords };
                        requests.Insert(request);
                    }
                    else
                    {
                        IdAnswer = requests.FindById(Maxi).AnwserId;
                        AnswerText = answers.FindById(IdAnswer).Text;
                    }
                }
            }
            return AnswerText;
        }
    }
}
