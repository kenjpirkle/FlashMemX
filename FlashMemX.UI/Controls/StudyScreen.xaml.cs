using System.Windows.Controls;

namespace FlashMemX.UI
{
    public partial class StudyScreen : UserControl
    {
        public StudyScreen()
        {
            flipButtons = new FlipButtons();
            newCardButtons = new NewCardButtons();
            learningCardButtons = new LearningCardButtons();

            InitializeComponent();

            if(ProgramManager.Scheduler.CardAvailable())
                DisplayNextCard();
        }

        private readonly FlipButtons flipButtons;
        private readonly NewCardButtons newCardButtons;
        private readonly LearningCardButtons learningCardButtons;

        private void DisplayNextCard()
        {
            CardContentControl.Content = ProgramManager.CardViewCreator.Create(
                ProgramManager.Scheduler.GetNext());
        }
    }
}
