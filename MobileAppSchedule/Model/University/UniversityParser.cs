using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Html.Dom;
using System.Text.RegularExpressions;
using MobileAppSchedule.Model.Parser;
using MobileAppSchedule.Model.ScheduleModel;
using DayOfWeek = MobileAppSchedule.Model.ScheduleModel.DayOfWeek;

namespace MobileAppSchedule.Model.University
{
    internal class UniversityParser : IParser<Schedule>
    {
        Schedule schedule = new Schedule();

        public UniversityParser(Schedule schedule)
        {
            this.schedule = schedule;
        }

        /// <summary>
        /// Этот метод будет заполнять дни недели
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public Schedule Parse(IHtmlDocument document)
        {
            var sourceString = document.Source.Text;

            //Получаем имя группы
            var groupName = document.QuerySelectorAll("table").Where(x => x.ClassName == "selectors").FirstOrDefault();
            schedule.GroupName = groupName.TextContent.Substring(groupName.TextContent.LastIndexOf("Группа:") + 10).Trim();
            

            //var dayse = sourceString.Split("colspan=6>", StringSplitOptions.None).ToList<string>();
            var dayse = new Regex("colspan=6>").Split(sourceString).ToList();
            
            dayse.RemoveAt(0);

            //добавляем каждые день недели
            foreach (var day in dayse)
            {
                string nameOfDay = day.Substring(0, day.IndexOf('<'));
                List<Discipline> disciplines = new List<Discipline>();

                
                //var sourceDisciplines = day.Split("<tr>", StringSplitOptions.None).ToList<string>();
                var sourceDisciplines = new Regex("<tr>").Split(day).ToList();

                sourceDisciplines.RemoveAt(0);
                sourceDisciplines.RemoveAt(0);
                sourceDisciplines.RemoveAt(sourceDisciplines.Count - 1);

                foreach (var discipline in sourceDisciplines)
                {
                    if (discipline.Contains("Полнодневные занятия"))
                        break;

                    //var str = discipline.Split("</td>\n\t<td>", StringSplitOptions.None).ToList();
                    var str = new Regex("</td>\n\t<td>").Split(discipline).ToList();

                    string period = discipline.Substring(discipline.IndexOf(">") + 1, 13);
                    string nameOfDiscipline = str[0].Substring(str[0].IndexOf("wrap") + 6);
                    string typeDescipline = str[1];
                    string frequency = str[2];
                    string audienceNumber = str[3];
                    string nameOfTeacher = Regex.Replace(str[4].Substring(0, str[4].IndexOf("<")), @"\s+", " ");

                    disciplines.Add(new Discipline()
                    {
                        AudienceNumber = audienceNumber,
                        Period = period,
                        NameOfDiscipline = nameOfDiscipline,
                        TypeDescipline = typeDescipline,
                        Frequency = frequency,
                        NameOfTeacher = nameOfTeacher
                    });
                }

                schedule.Weekday.Add(new DayOfWeek
                {
                    NameOfDay = nameOfDay,
                    Disciplines = disciplines
                });
            }
            return schedule;
        }
    }
}
