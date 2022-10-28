using AngleSharp.Html.Parser;
using MobileAppSchedule.Model.Parser;
using System;
using System.Threading.Tasks;

namespace MobileAppSchedule.Model.Worker
{
    class ParserWorker<T> where T : class
    {
        private bool _isLoad;
        IParser<T> parser;
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

        public async Task LoadDataByGroupName()
        {
            _isLoad = true;
            await Worker();
        }

        public async Task Worker()
        {
            var source = await loader.GetScheduleByGroupName(parserSettings.GroupNames[124]);
            if (source == "Ответ пустой")
            {
                OnComplete?.Invoke("Ответ пустой");
                _isLoad = false;
                return;
            }

            var domParser = new HtmlParser();
            var document = await domParser.ParseDocumentAsync(source);

            var result = parser.Parse(document); //вернётся объект типа Schedule

            _isLoad = false;
            OnGroupSchedule?.Invoke(this, result);
            OnComplete?.Invoke("Ответ не пустой");
        }
    }
}
