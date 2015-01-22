using System;
using System.Collections.Generic;
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
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var db = new CardSortEntities())
            {
                var csv = new CsvWriter(streamWriter);

                var allStrategies = db.Strategies.ToList();
            }

            throw new NotImplementedException();
        }
    }
}