﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardSort.Data;
using CardSort.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace CardSort.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var model = new HomeIndexViewModel();

            using (var db = new CardSortEntities())
            {
                model.Participants = db.Participants.Select(x => new ParticipantItemViewModel()
                {
                    UniqueId = x.UniqueId,
                    TotalCategories = x.Categories.Count(),
                    TotalStrategies = x.Categories.Sum(y => y.Strategies.Count())
                }).ToList();
            }

            return View(model);
        }

        public ActionResult Process(HttpPostedFileBase file)
        {
            using (var db = new CardSortEntities())
            {
                db.Database.Delete();
            }

            using (var textReader = new StreamReader(file.InputStream))
            using (var db = new CardSortEntities())
            {
                var csv = new CsvReader(textReader, new CsvConfiguration()
                    {
                        HasHeaderRecord = false // This is so we can manually parse the header
                    });
                
                // Read header
                csv.Read();
                var strategyMap = new Dictionary<int, int>();

                var startColumn = 6;
                var endColumn = csv.CurrentRecord.Count();

                for (var i = startColumn; i < endColumn; i++)
                {
                    var newStrategy = new Strategy()
                    {
                        Description = csv.GetField(i)
                    };
                    db.Strategies.Add(newStrategy);
                    db.SaveChanges();
                    strategyMap.Add(i, newStrategy.Id);
                }

                var allStrategies = db.Strategies.ToList();

                // Read Rows
                while (csv.Read())
                {
                    var uniqueId = csv.GetField<int>(0);
                    var participant = db.Participants.FirstOrDefault(x => x.UniqueId == uniqueId);

                    if (participant == null)
                    {
                        participant = new Participant()
                        {
                            UniqueId = uniqueId
                        };
                        db.Participants.Add(participant);
                    }

                    var category = new Category()
                    {
                        Name = csv.GetField(1),
                    };
                    participant.Categories.Add(category);

                    for (var i = startColumn; i < endColumn; i++)
                    {
                        var value = csv.GetField<int>(i);

                        if (value == 1)
                        {
                            var strategy = allStrategies.FirstOrDefault(x => x.Id == strategyMap[i]);

                            category.Strategies.Add(strategy);
                        }
                    }
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult ClusteringAnalysis()
        {
            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);

            using (var db = new CardSortEntities())
            {
                var csv = new CsvWriter(streamWriter);

                var allStrategies = db.Strategies.ToList();

                // Build Header Row
                csv.WriteField(""); // Blank for the pivot cell
                foreach (var strategy in allStrategies)
                {
                    csv.WriteField(strategy.Description);
                }
                csv.NextRecord();

                // Build data rows
                for (var y = 0; y < allStrategies.Count(); y++)
                {
                    csv.WriteField(allStrategies[y].Description); // Row Strategy

                    for (var x = 0; x < allStrategies.Count(); x++)
                    {
                        if (x != y)
                        {
                            var yStrategyId = allStrategies[y].Id;
                            var xStrategyId = allStrategies[x].Id;

                            var clusterCount = db.Categories.Count(j => j.Strategies.Any(k => k.Id == yStrategyId)
                                                                        &&
                                                                        j.Strategies.Any(k => k.Id == xStrategyId));

                            csv.WriteField(clusterCount);
                        }
                        else
                        {
                            csv.WriteField("");
                        }

                    }
                    csv.NextRecord();
                }

                streamWriter.Flush();

                // "return File" automatically disposes the stream and writer.
                streamWriter.BaseStream.Position = 0;
                return File(streamWriter.BaseStream, "text/csv", string.Format("ClusterAnalysis_{0}.csv", DateTime.Now.ToString("yyyy-MM-dd_hhmmss")));
            }            
        }

        public ActionResult PersonClusters()
        {
            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);

            using (var db = new CardSortEntities())
            {
                var csv = new CsvWriter(streamWriter);

                var allStrategies = db.Strategies.ToList();
                var participants = db.Participants.AsQueryable()
                    .Include(x => x.Categories.Select(y => y.Strategies))
                    .ToList();
                
                //Write Headers
                csv.WriteField("Participant_ID");
                for (int y = 0; y < allStrategies.Count; y++)
                {
                    for (int x = y + 1; x < allStrategies.Count; x++)
                    {
                        csv.WriteField($"{allStrategies[y].Id}_{allStrategies[x].Id}");
                    }
                }
                csv.NextRecord();

                for (int p = 0; p < participants.Count; p++)
                {
                    csv.WriteField(participants[p].UniqueId);

                    var dict = new List<string>();

                    for (int c = 0; c < participants[p].Categories.Count; c++)
                    {
                        var category = participants[p].Categories.ElementAt(c);

                        var categoryStrategies = category.Strategies.OrderBy(x => x.Id).ToList();

                        for (int s = 0; s < categoryStrategies.Count; s++)
                        {
                            var strategy = categoryStrategies.ElementAt(s);
                                
                            for (int s2 = s + 1; s2 < categoryStrategies.Count; s2++)
                            {
                                var otherStrategy = categoryStrategies.ElementAt(s2);

                                if (strategy.Id != otherStrategy.Id)
                                {
                                    dict.Add($"{strategy.Id}_{otherStrategy.Id}");
                                }
                            }
                        }
                    }

                    for (int y = 0; y < allStrategies.Count; y++)
                    {
                        for (int x = y + 1; x < allStrategies.Count; x++)
                        {
                            var result = dict.Contains($"{allStrategies[y].Id}_{allStrategies[x].Id}");
                            csv.WriteField(result ? 1 : 0);
                        }
                    }

                    csv.NextRecord();
                }

                streamWriter.Flush();

                // "return File" automatically disposes the stream and writer.
                streamWriter.BaseStream.Position = 0;
                return File(streamWriter.BaseStream, "text/csv", string.Format("PersonClusterAnalysis_{0}.csv", DateTime.Now.ToString("yyyy-MM-dd_hhmmss")));
            }
        }
    }
}