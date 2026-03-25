using System.Text.Json;

namespace AlignmentUI
{
    public partial class MainForm : Form
    {
        private List<Token> HebrewTokens;
        private List<Token> TargetTokens;
        private List<AlignmentPair> Alignments = new List<AlignmentPair>();

        public MainForm()
        {
            InitializeComponent();
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadExample();
        }

        private void LoadExample()
        {
            HebrewTokens = new List<Token>
            {
                new Token { Index = 0, Text = "בראשית" },
                new Token { Index = 1, Text = "ברא" },
                new Token { Index = 2, Text = "אלהים" }
            };

            TargetTokens = new List<Token>
            {
                new Token { Index = 0, Text = "In" },
                new Token { Index = 1, Text = "the" },
                new Token { Index = 2, Text = "beginning" },
                new Token { Index = 3, Text = "God" },
                new Token { Index = 4, Text = "created" }
            };

            hebrewList.DataSource = HebrewTokens;
            targetList.DataSource = TargetTokens;
        }

        private void AddAlignment_Click(object sender, EventArgs e)
        {
            var h = hebrewList.SelectedItems.Cast<Token>().Select(t => t.Index).ToList();
            var t = targetList.SelectedItems.Cast<Token>().Select(t => t.Index).ToList();

            if (h.Count == 0 || t.Count == 0)
                return;

            var pair = new AlignmentPair { hebrew = h, target = t };
            Alignments.Add(pair);

            alignmentList.Items.Add($"H[{string.Join(",", h)}] -> T[{string.Join(",", t)}]");
        }

        private void Save_Click(object sender, EventArgs e)
        {
            var result = new { alignments = Alignments };

            string json = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText("corrected_alignment.json", json);

            MessageBox.Show("Saved!");
        }

    }
    public class Token
    {
        public int Index { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return $"{Index}: {Text}";
        }
    }

    public class AlignmentPair
    {
        public List<int> hebrew { get; set; }
        public List<int> target { get; set; }
    }


}
