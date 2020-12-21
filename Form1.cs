using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace DeleteUnattainable
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var fd = new OpenFileDialog();
            fd.InitialDirectory = Environment.CurrentDirectory;
            fd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            DialogResult res = fd.ShowDialog();
            if (res == DialogResult.OK)
            {
                DeleteUnattainableFromString(File.ReadAllText(fd.FileName));
            }

        }

        private void DeleteUnattainableFromString(string input)
        {
            var T = new HashSet<char>();
            var N = new HashSet<char>();
            var Rules = new Dictionary<char, Rule>();

            var text = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            char StartN = text[0][0];

            for(int i=0; i<text.Length; i++)
            {
                var parts = text[i].Split(new string[] { "|", "->" }, StringSplitOptions.RemoveEmptyEntries);
                var r = new Rule();
                r.From = parts[0].First();
                N.Add(r.From);
                for (int j = 1; j < parts.Length; j++)
                {
                    if (parts[j].Contains(@"\e"))
                        parts[j] = parts[j].Replace(@"\e", " ");
                    r.To.Add(parts[j]);
                    foreach (char c in parts[j])
                    {
                        if (char.IsUpper(c))
                            N.Add(c);
                        else
                            T.Add(c);
                    }
                }
                Rules.Add(r.From,r);
            }

            DrawAll(T, N, Rules, StartN);

            var TN = T.Concat(N).ToHashSet();
            var W = new HashSet<char>(); W.Add(StartN);
            while (true)
            {
                var cnt = W.Count;
                AddAsPossible(W, Rules);
                if (cnt == W.Count || TN.SetEquals(W))
                    break;
            }

            if (TN.SetEquals(W))
            {
                result_textBox.Text = "Грамматика не содержит недостижимых символов. \r\n";
                DrawAll(T, N, Rules, StartN);
                return;
            }

            var tempRules = new Dictionary<char, Rule>(Rules);
            Rules.Clear();
            foreach (char key in tempRules.Keys)
            {
                if (W.Contains(key))
                {
                    Rules.Add(key, tempRules[key]);
                }
                else
                {
                    N.Remove(key);
                } 
            }

            //var tempT = new HashSet<char>(T);
            T.Clear();
            foreach (Rule r in Rules.Values)
            {
                foreach (string s in r.To)
                    foreach (char c in s)
                        if (!char.IsUpper(c))
                            T.Add(c);
            }

            result_textBox.Text = "Недостижимые удалены. \r\n";
            DrawAll(T, N, Rules, StartN);
        }


        private void AddAsPossible(HashSet<char> W, Dictionary<char,Rule> Rules)
        {
            var tmp = new HashSet<char>(W);
            foreach (Rule r in Rules.Values)
            {
                if (tmp.Contains(r.From))
                    foreach (var s in r.To)
                        foreach (var c in s)
                            W.Add(c);
            }
            
        }

        private void DrawAll(HashSet<char> T, HashSet<char> N, Dictionary<char,Rule> Rules, char StartN)
        {
            string res = "T = ";
            foreach (var x in T)
                res += x == ' '? @"\e, " : x + ", ";
            res += '\r' +"\n";

            res += "N = ";
            foreach (var x in N)
                res += x + ", ";
            res += '\r' + "\n";

            res += "Start = "+StartN+ '\r' + "\n" + '\r' + "\n";

            foreach (var x in Rules)
                res += x.Value.ToString()+'\r' + "\n";

            result_textBox.Text += res;
        }


        private class Rule
        {
            public char From { get; set; }
            public HashSet<string> To { get; set; }

            public Rule()
            {
                To = new HashSet<string>();
            }

            public override string ToString()
            {
                var s = From + "->";
                foreach (var x in To)
                    s += x.Replace(" ", @"\e") + "|";
                return s.Substring(0, s.Length - 1);
            }
        }
    }
}
