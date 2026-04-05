using Microsoft.Extensions.Logging;

namespace Testing_Serilog
{
    public partial class MainForm : Form
    {
        private readonly ILogger<MainForm> _logger;

        public MainForm(ILogger<MainForm> logger)
        {
            InitializeComponent();

            _logger = logger;

            _logger.LogInformation("MainForm started.");

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Simulate some work
                _logger.LogInformation("MainForm is loading...");
                Thread.Sleep(2000); // Simulate work by sleeping for 2 seconds
                TestException(0); // This will throw an exception
                _logger.LogInformation("MainForm loaded successfully.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading MainForm.");
            }
        }

        private void TestException(int v)
        {
            throw new NotImplementedException();
        }
    }
}
