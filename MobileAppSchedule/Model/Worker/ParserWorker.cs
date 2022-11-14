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
        public event Action<object, string> OnGroupSchedule;
        public event Action<object, string> OnGroups;
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
        /// 
        public async Task LoadScheduleByGroupName(int index)
        {
            await GetSchedule(index);
        }
        public async Task LoadGroupNames()
        {
            await GetGroupNames();
        }


        private async Task GetGroupNames()
        {
            var source = await loader.GetRowGroups();
            if (source == "Ответ пустой")
            {
                OnGroups?.Invoke(this, "Ответ пустой");
                return;
            }
            OnGroups?.Invoke(this, source);
        }
        public async Task GetSchedule(int index)
        {
            var source = await loader.GetScheduleByGroupName(parserSettings.GroupNames[index]);
            if (source == "Ответ пустой")
            {
                OnGroupSchedule?.Invoke(this, "Ответ пустой");
                return;
            }
            OnGroupSchedule?.Invoke(this, source);
        }
    }
}
