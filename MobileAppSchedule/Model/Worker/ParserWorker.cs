using AngleSharp.Html.Parser;
using MobileAppSchedule.Model.Parser;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MobileAppSchedule.Model.Worker
{
    class ParserWorker<T> where T : class
    {
        private bool _isLoad = false;
        //Здесь будет UniversityParser
        IParser<T> parser;
        //Здесь будет UniversitySettings
        IParserSettings parserSettings;

        HtmlLoader loader;

        #region Properties
        public IParser<T> Parser
        {
            get => parser;
            set => parser = value;
        }

        public IParserSettings ParserSettings
        {
            get => parserSettings;
            set
            {
                parserSettings = value;
                loader = new HtmlLoader(value);
            }
        }
        #endregion

        #region Events
        /// <summary> информирует о получении данных после парсинга распписания </summary>
        public event Action<object, T> OnGroupSchedule;
        /// <summary> Информирование при завершении работы парсера </summary>
        public event Action<object> OnComplete;
        #endregion

        #region Constructors
        public ParserWorker(IParser<T> parser)
        {
            this.parser = parser;
        }

        //this(parser) - вызывает первый конструктор
        public ParserWorker(IParser<T> parser, IParserSettings parserSettings) : this(parser)
        {
            this.parserSettings = parserSettings;
            loader = new HtmlLoader(parserSettings);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task LoadDataByGroupName(int index)
        {
            _isLoad = true;
            await Worker(index);
        }

        public async Task Worker(int index)
        {
            var source = await loader.GetScheduleByGroupName(parserSettings.GroupNames[index]);
            if (source == "Ответ пустой")
            {
                OnComplete?.Invoke("Ответ пустой");
                _isLoad = false;
                return;
            }

            //Заносим в память телефона 
            var path = FileSystem.CacheDirectory;
            var fullpath = Path.Combine(path, "mobileschedule_schedule.txt");
            File.WriteAllText(fullpath, source);

            var domParser = new HtmlParser();
            var document = await domParser.ParseDocumentAsync(source);

            var result = parser.Parse(document); //вернётся объект типа Schedule

            _isLoad = false;
            OnGroupSchedule?.Invoke(this, result);
            OnComplete?.Invoke("Ответ не пустой");
        }
    }
}
