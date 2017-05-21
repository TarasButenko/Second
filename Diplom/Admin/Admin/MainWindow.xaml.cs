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

namespace Admin
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
            public int Id { get; set; }
            public string Text { get; set; }
        }
        private IEnumerable<string> GetWords(string input)
        {
            string space = @" ";

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
        private string mydocpath;
        private string filePath;
        private Request request;
        private int index;
        public MainWindow()
        {
            InitializeComponent();
            index -= 1;
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
            mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            filePath = System.IO.Path.Combine(mydocpath, "Requests-Answers.db");
        }
        private void ButtonStartClick(object sender, RoutedEventArgs e)
        {
            int count = 0;
            using (var db = new LiteDatabase(filePath))
            {
                bool NotYet = true;
                var requests = db.GetCollection<Request>("requests");
                foreach (var req in requests.FindAll())
                {
                    if (req.AnwserId == -1 && NotYet)
                    {
                        NotYet = !NotYet;
                        count++;
                        request = new Request { AnwserId = -1, Text = req.Text, Words = req.Words };
                        index = req.Id;
                        TextBoxAnswer.Text = req.Id.ToString();
                        TextBlockReq.Text = req.Text;
                    }
                }
                if(count==0)
                {
                    TextBlockReq.Text="Активных вопросов нет";
                }
            }
        }
        private void ButtonReqClick(object sender, RoutedEventArgs e)
        {
             using (var db = new LiteDatabase(filePath))
            {
                var requests = db.GetCollection<Request>("requests");
                var answers = db.GetCollection<Answer>("answers");
                var ans = new Answer { Text = TextBoxRequest.Text };
                answers.Insert(ans);
                int idanswer = answers.Find(x => x.Text == ans.Text).ElementAt(0).Id;
                foreach (var req in requests.FindAll())
                {
                    if (req.Id == index && index != -1)
                    {
                        req.AnwserId = idanswer;
                        requests.Update(req);
                        index = -1;
                        TextBlockReq.Text = "Ответ сохранен";
                    }
                }
            }  
        }
        private void ButtonAnsClick(object sender, RoutedEventArgs e)
        {
            using (var db = new LiteDatabase(filePath))
            {
                var requests = db.GetCollection<Request>("requests");
                var req = request;
                TextBlockInf.Text = req.Id + " " + GetWords(req.Text).ElementAt(0) + " " + req.Text + "";
            }                
        }
        private void ButtonDeleteClick(object sender, RoutedEventArgs e)
        {

            using (var db = new LiteDatabase(filePath))
            {
                var requests = db.GetCollection<Request>("requests");
                foreach (var req in requests.FindAll())
                {
                    if (req.Id == index && index!=-1)
                    {
                        requests.Delete(index);
                        index = -1;
                        TextBlockReq.Text = "Удалено";
                    }
                }
            }
        }
        private void ButtonAnsShowClick(object sender, RoutedEventArgs e)
        {
            RefreshIdDB();
            using (var db = new LiteDatabase(filePath))
            {
                var answers = db.GetCollection<Answer>("answers");
                TextBlockInf.Text = "";
                TextBlockInf2.Text = "";
                foreach (var ans in answers.Find(x => x.Id >= 0))
                {
                    TextBlockInf.Text+= "# "+ ans.Id +" - "+ans.Text +"\n";
                }
                var requests = db.GetCollection<Request>("requests");
                foreach (var req in requests.Find(x => x.Id >= 0))
                {
                    TextBlockInf2.Text += "№ " + req.Id + " - " + req.Text + "\n";
                }
                
            }
            
        }
        private void RefreshIdDB()
        {
            using (var db = new LiteDatabase(filePath))
            {
                var requests = db.GetCollection<Request>("requests");
                var requests2 = db.GetCollection<Request>("requests2");
                
                int i=0;
                int count = requests.FindAll().Count();
                int[] Ar = new int[count];
                
                foreach (var req in requests.FindAll())
                {
                    Ar[i++] = req.Id;
                    Request R = new Request { AnwserId = req.AnwserId, Text = req.Text, Words = req.Words };
                    requests2.Insert(R);
                }
                for(i=0;i<count;i++)
                {
                    int j = Ar[i];
                    requests.Delete(j);
                }
                i=0;
                count = requests2.FindAll().Count();
                int[] Ar2 = new int[count];
                
                foreach (var req in requests2.FindAll())
                {
                    Ar2[i++] = req.Id;
                    Request R = new Request { AnwserId = req.AnwserId, Text = req.Text, Words = req.Words };
                    requests.Insert(R);
                }

                for (i = 0; i < count; i++)
                {
                    int j = Ar2[i];
                    requests2.Delete(j);
                }

                
            }     
        }
    }
}
