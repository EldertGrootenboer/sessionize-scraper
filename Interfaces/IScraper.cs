using System;

using RvdB.Scrapionize.Models;

namespace RvdB.Scrapionize.Interfaces
{
	public interface IScraper
	{
		SessionizeData Scrape(Uri url);
	}
}
