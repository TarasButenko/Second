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
using System.Globalization;
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
            public IEnumerable<string> GetWords()
            {
                string space = @" ";
                Text = Text.ToLower();
                Text.Trim(new Char[] { ' ', '*', '.', ',', '?', '&', '!' });
                string[] S = Regex.Split(Text, space);
                S = S.Distinct().ToArray();
                this.Words = S.Where(x => !string.IsNullOrWhiteSpace(x));
                /*foreach (string element in StrArray)
                {
                    if (element != "")
                    {
                        TextBlockAnswer.Text +=  element+' ';
                    
                    }
                }*/
                return this.Words;
            }
        }
        public class Answer
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }
        static private int[] WordsCount = { 0, 1, 2, 3, 3, 4, 5, 6, 6, 7, 8 };
        private string mydocpath;
        private string filePath;
        private string filePath2;
        DateTime localDate;
        CultureInfo culture;
        public MainWindow()
        {
            InitializeComponent();
         //   MessageBox.Show("Вам підходить відповідь: " + "Реферат робити потрібно", "Question",
          //     MessageBoxButton.YesNo, MessageBoxImage.Warning) ;
            mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
             filePath = System.IO.Path.Combine(mydocpath, "Requests-Answers.db");
             localDate = DateTime.Now;
              culture = new CultureInfo("uk-UA");
              ListBoxJournal.Items.Add("Журнал запитаннь та відповідей Дата:" + localDate.ToString(culture));
            /* if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }
           */
        }
        private void ButtonSaveClick(object sender, RoutedEventArgs e)
        {
            string text = "";


            string s = localDate.ToString(culture);
            s = s.Replace(" ", "");
            s = s.Replace(":", "");
            s = s.Replace(".", "");
            mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            filePath2 = System.IO.Path.Combine(mydocpath, s);
            foreach (string txt in ListBoxJournal.Items)
                text += txt;
           
            if (!File.Exists(filePath2))
            {
              //  File.Create(filePath2 + ".txt");
            }
            
            System.IO.File.WriteAllText(filePath2+".txt", text);

        }
        private void ButtonStartClick(object sender, RoutedEventArgs e)
        {
           
            if (TextBoxRequest.Text != "")
            {
                if (TextBoxRequest.Text.Trim(new Char[] { ' ', '*', '.', ',', '?', '&', '!' }) != "")
                {
                    TextBlockAnswer.Text = GetAnswer(TextBoxRequest.Text);
                    ListBoxJournal.Items.Add("Запитання:" + TextBoxRequest.Text + " - " + TextBlockAnswer.Text);
                }
                else
                {
                    TextBoxRequest.Text = "Ви ввели некоректний рядок";
                }
            }
            else 
            {
                TextBoxRequest.Text = "Ви не ввели запитання";
            }
           // TextBoxRequest.Text = "Ви ввели більше 10 слів";
        }
        private IEnumerable<string> GetWords(string input)
        {
            string space=@" ";

            string[] S = Regex.Split(input, space);
            var StrArray = S.Where(x => !string.IsNullOrWhiteSpace(x));
            /*foreach (string element in StrArray)
            {
                if (element != "")
                {
                    TextBlockAnswer.Text +=  element+' ';
                    
                }
            }*/
            return StrArray;
        }
        public string GetAnswer(string S)
        {
            string AnswerText;
            int IdAnswer;
            bool Done=false;
            IEnumerable<string> RequestWords;
            RequestWords = GetWords(S);
            int wordsCount = RequestWords.Count();
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
                        AnswerText = "Відповідь :"+((answers.Find(x => x.Id == IdAnswer)).ElementAt(0)).Text;
                        Done = true;
                    }
                    else 
                    {
                        AnswerText = "Питання в черзі на розгляд експертом";
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
                    foreach (var req in requests.FindAll())
                    {
                        
                            foreach (string word in RequestWords)
                            {
                                for (int j = 0; j < GetWords(req.Text).Count(); j++)
                                {
                                    if (word.Equals(GetWords(req.Text).ElementAt(j)))
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
                    if(Max>=WordsCount[wordsCount])
                    {
                           IdAnswer = requests.FindById(Maxi + 1).AnwserId;
                        if (IdAnswer < 0)
                        {
                            AnswerText = "Відповідь не знайденна, запитання відправлено тьютору";
                            //MessageBox.Show(RequestWords.ElementAt(0) + RequestWords.ElementAt(1));
                            var request = new Request { AnwserId = -1, Text = S, Words = RequestWords };
                            requests.Insert(request);

                        }
                        else
                        {
                            if (Max == wordsCount)
                            {
                                AnswerText ="Відповідь: "+ answers.FindById(IdAnswer).Text;
                            }
                            else
                            {
                                if (MessageBox.Show("Вам підходить відповідь: " + answers.FindById(IdAnswer).Text, "Question",
               MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                                { 
                                    AnswerText = "Відповідь: "+ answers.FindById(IdAnswer).Text;
                                }
                                else
                                {
                                    AnswerText = "Відповідь не знайденна, запитання відправлено тьютору";
                                }
                            }
                        }
                    }
                    else
                    {
                        AnswerText = "Відповідь не знайденна, запитання відправлено тьютору";
                        //MessageBox.Show(RequestWords.ElementAt(0) + RequestWords.ElementAt(1));
                        var request = new Request { AnwserId = -1, Text = S, Words = RequestWords };
                        requests.Insert(request);
                    }
                    
                }
            }
            return AnswerText;
        }
    }
}
