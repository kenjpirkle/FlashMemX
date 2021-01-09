using System.Threading.Tasks;
using FlashMemX.Core;

namespace FlashMemX.UI
{
    internal static class ProgramManager
    {
        public static void Setup()
        {
            Database = new Database();
            FetchBasicStatistics();
            preloader = Task.Run(() => PreloadStudyInformation());

            CreateScreens();

            SetScreen(Screen.HomeScreen);
            hostWindow.Show();
        }

        private static HostWindow hostWindow;
        private static HomeScreen homeScreen;
        private static StudyScreen studyScreen;
        private static CreateSchemaScreen createSchemaScreen;
        private static CreateTemplateScreen CreateTemplateScreen;
        private static Task preloader;

        public static DefaultScheduler Scheduler { get; private set; }
        public static Cache Cache { get; private set; }
        public static CardViewCreator CardViewCreator { get; private set; }
        public static Database Database { get; private set; }

        private static void CreateScreens()
        {
            hostWindow = new HostWindow();
            homeScreen = new HomeScreen();
            createSchemaScreen = new CreateSchemaScreen();
            CreateTemplateScreen = new CreateTemplateScreen();
        }

        private static void FetchBasicStatistics()
        {
        }

        private static void PreloadStudyInformation()
        {
            Scheduler = new DefaultScheduler(Database);
            Cache = new Cache(Scheduler.GetMetadata(), Database);
            CardViewCreator = new CardViewCreator();
        }

        private static void SetScreen(Screen screen)
        {
            if(screen == Screen.HomeScreen)
                hostWindow.Content = homeScreen;
            else if(screen == Screen.StudyScreen)
                hostWindow.Content = studyScreen;
            else if(screen == Screen.CreateSchemaScreen)
                hostWindow.Content = createSchemaScreen;
            else if(screen == Screen.CreateTemplateScreen)
                hostWindow.Content = CreateTemplateScreen;
        }

        public static void StartStudy()
        {
            preloader.Wait();
            studyScreen = new StudyScreen();
            SetScreen(Screen.StudyScreen);
        }

        public static void StartCreateSchema()
        {
            SetScreen(Screen.CreateSchemaScreen);
        }

        public static void StartCreateTemplate()
        {
            SetScreen(Screen.CreateTemplateScreen);
        }
    }
}
