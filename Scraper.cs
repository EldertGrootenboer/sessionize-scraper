﻿// Adapted from https://github.com/rickvdbosch/scrapionize

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using HtmlAgilityPack;

using RvdB.Scrapionize.Interfaces;
using RvdB.Scrapionize.Models;

namespace RvdB.Scrapionize
{
    public class Scraper : IScraper
    {
        /// <summary>
        /// Gets all of the Sessionize data from the passed in URL.
        /// </summary>
        /// <param name="url"><see cref="Uri"/> for the Sessionize CFP page.</param>
        /// <returns>A <see cref="SessionizeData"/> instance containing the data for the provided <paramref name="url"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="url"/> is null.</exception>
        public SessionizeData Scrape(Uri url)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            var result = new SessionizeData();

            var doc = new HtmlWeb().Load(url);
            var descendants = doc.DocumentNode.Descendants().ToList();

            result.EventName = descendants.Where(d => d.HasClass(Constants.NAME_TITLE)).SelectMany(d => d.Descendants(Constants.NAME_HEADING4)).FirstOrDefault()?.InnerText;
            var contentRows = descendants.Where(d => d.HasClass(Constants.NAME_CONTENT)).ToList();
            var leftColumn = contentRows.ElementAt(1);
            var rightColumn = contentRows.ElementAt(2);

            int locationIndex = 2;
            int eventUrlIndex = 3;
            var leftHeaders = leftColumn.Descendants(Constants.NAME_HEADING2).ToList();
            result.EventStartDate = ParseSessionizeDate(leftHeaders.ElementAt(0).InnerText);
            result.EventEndDate = ParseSessionizeDate(leftHeaders.ElementAt(1).InnerText);
            if (result.EventEndDate == DateTime.MinValue)
            {
                result.EventEndDate = result.EventStartDate;
				locationIndex = 1;
				eventUrlIndex = 2;
            }

            result.Location = string.Join(Constants.CARRIAGERETURN_LINEFEED, leftHeaders.ElementAt(locationIndex).Descendants(Constants.NAME_SPAN).Select(d => d.InnerText.Trim()));
            result.EventUrl = leftHeaders.ElementAt(eventUrlIndex).Descendants(Constants.NAME_LINK).Single().Attributes[Constants.NAME_HREF].Value;

            var rightHeaders2 = rightColumn.Descendants(Constants.NAME_HEADING2).ToList();
            result.CfpStartDate = ParseSessionizeDate(rightHeaders2.ElementAt(0).InnerText);
            result.CfpEndDate = ParseSessionizeDate(rightHeaders2.ElementAt(1).InnerText);

            var rightHeaders3 = rightColumn.Descendants(Constants.NAME_HEADING3).ToList();
            rightHeaders3 = rightHeaders3.Skip(rightHeaders3.Count - 3).ToList();

            var headers = new Dictionary<string, string>();

            for(int i = 0; i < rightHeaders3.Count; i++)
            {
                var headerName = rightHeaders3.ElementAt(i).InnerText;
                var headerValue = rightHeaders3.ElementAt(i)?.NextSibling.NextSibling.InnerText;
                headers.Add(headerName, headerValue);
            }

            result.Travel = headers.ContainsKey(Constants.NAME_TRAVEL) ? headers[Constants.NAME_TRAVEL] : string.Empty;
            result.Accommodation = headers.ContainsKey(Constants.NAME_ACCOMMODATION) ? headers[Constants.NAME_ACCOMMODATION] : string.Empty;
            result.EventFee = headers.ContainsKey(Constants.NAME_EVENTFEE) ? headers[Constants.NAME_EVENTFEE] : string.Empty;

            return result;
        }

        #region Helper methods

        private static DateTime ParseSessionizeDate(string date)
        {
            DateTime dateTime = DateTime.MinValue;
            DateTime.TryParseExact(date, Constants.DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
            return dateTime;
        }

        #endregion
    }
}
