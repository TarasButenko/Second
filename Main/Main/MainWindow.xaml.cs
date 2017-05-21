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
        public MainWindow()
        {
            InitializeComponent();
            mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = System.IO.Path.Combine(mydocpath, "text.txt");
            if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }
           
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
            int IdAnwser;
            IEnumerable<string> RequestWords;
            RequestWords = GetWords(S);
            using (var db = new LiteDatabase(@"Requests-Answers.db"))
            {
                var requests = db.GetCollection<Request>("requests");
                var answers = db.GetCollection<Answer>("answers");

                if (requests.EnsureIndex(x => x.Equals(S)))
                {
                    var request = requests.Find(x => x.Text.Equals(S));
                    IdAnwser = request.ElementAt(0).AnwserId;
                    AnswerText = ((answers.Find(x => x.Equals(IdAnwser))).ElementAt(0)).Text;
                }
                else
                {
                    AnswerText = "Ответ не найден";
                }
            }
            return AnswerText;
        }
    }
}
