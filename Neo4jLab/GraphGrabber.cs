using System;
using System.Collections;
using Neo4j.Driver;

namespace Neo4jLab
{
    public class GraphGrabber : IDisposable
    {
        private IDriver _driver;
        public GraphGrabber()
        {
            _driver = GraphDatabase.Driver("bolt://localhost:7687", 
                AuthTokens.Basic("neo4j", "311193"));
        }

        public string TestQuery()
        {
            using var session = _driver.Session();
            var timespan = session.ReadTransaction(transaction =>
            {
                //var parameters = new Dictionary<string, string>();
                //parameters.Add("L")
                var L = 1;
                var result = transaction.Run($"MATCH (m:Movie) RETURN m LIMIT 10");
                foreach (var record in result)
                {
                    var node = record["m"].As<INode>();
                    Console.WriteLine(node.Id);
                    foreach (var label in node.Labels)
                    {
                        Console.WriteLine(label);
                    }
                    foreach (var property in node.Properties)
                    {
                        Console.WriteLine($"{property.Key} -- {property.Value}");
                    }

                }
                return result.Consume().ResultAvailableAfter;
            });
            return timespan.ToString();
        }

        public string GetFilmsDirectedBy(string DirectedBy)
        {
            using var session = _driver.Session();
            var timespan = session.ReadTransaction(transaction =>
            {
                string queryString = "MATCH (people:Person {name:\"" + DirectedBy + "\"})-[:DIRECTED]->(m) RETURN m";
                var result = transaction.Run(queryString);
                Console.WriteLine("----------");
                foreach (var record in result)
                {
                    var node = record["m"].As<INode>();
                    foreach (var property in node.Properties)
                    {
                        Console.WriteLine($"{property.Key} -- {property.Value}");
                    }
                    Console.WriteLine("----------");
                }
                return result.Consume().ResultAvailableAfter;
            });
            return timespan.ToString();
        }

        public string GetPeopleWorkedWith(string WorkedWith)
        {
            using var session = _driver.Session();
            var timespan = session.ReadTransaction(transaction =>
            {
                string queryString = "MATCH (people:Person {name:\"" + WorkedWith + "\"})-[]->(m)<-[]-(p) RETURN p.name";
                var result = transaction.Run(queryString);
                Console.WriteLine("----------");
                foreach (var record in result)
                {
                    var name = record["p.name"].As<string>();
                    Console.WriteLine(name);
                }
                Console.WriteLine("----------");
                return result.Consume().ResultAvailableAfter;
            });
            return timespan.ToString();
        }

        public string GetActorsCount()
        {
            using var session = _driver.Session();
            var timespan = session.ReadTransaction(transaction =>
            {
                var result = transaction.Run("MATCH (actor:Person)-[:ACTED_IN]->(m) RETURN m.title, count(actor)");
                Console.WriteLine("----------");
                foreach (var record in result)
                {
                    var title = record["m.title"].As<string>();
                    var count_actors = record["count(actor)"].As<int>();
                    Console.WriteLine($"{title} -- {count_actors}");
                }
                Console.WriteLine("----------");
                return result.Consume().ResultAvailableAfter;
            });
            return timespan.ToString();
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }

    }
}