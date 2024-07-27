using System;
using System.Windows.Forms;
using ZedGraph;

namespace GraphProject
{
    public partial class Form1 : Form
    {
        private ZedGraphControl zgc;

        public Form1()
        {
            InitializeComponent();
            zgc = new ZedGraphControl();
            zgc.Dock = DockStyle.Fill;
            this.Controls.Add(zgc);
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var graph = Program.ReadGraphFromFile(@"C:\\programing\\projects\\graph-city-csharp\\avlgraph\\graph_data.txt");
            Program.DrawGraph(graph, zgc);
        }
    }
}
